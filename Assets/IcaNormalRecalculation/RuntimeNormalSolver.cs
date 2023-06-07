using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace IcaNormal
{
    [RequireComponent(typeof(Renderer))]
    public class RuntimeNormalSolver : MonoBehaviour
    {
        public enum NormalRecalculateMethodEnum
        {
            CachedLite,
            SDBursted
        }

        public enum NormalOutputEnum
        {
            WriteToMesh,
            WriteToMaterial
        }

        public NormalRecalculateMethodEnum CalculateMethod = NormalRecalculateMethodEnum.SDBursted;
        public NormalOutputEnum NormalOutputTarget = NormalOutputEnum.WriteToMesh;

        [Tooltip("Smoothing angle only usable with bursted method")] [Range(0, 180)] public float SmoothingAngle = 120f;

        public bool RecalculateOnStart;

        public bool CalculateBlendShapes;

        [Tooltip("Data cache asset required when using cached method. You can create this on project tab context menu/plugins /Mesh data cache.")] [SerializeField]
        private MeshDataCache _dataCache;

        [Tooltip("Asset of this model in zero pose. Only necessary when using Calculate Blend Shapes option")] public GameObject ModelPrefab;

        private Renderer _renderer;
        private Mesh _mesh;
        private Mesh _tempMesh;

        private UnsafeList<NativeArray<int>> _nativeDuplicatesData;
        private NativeArray<float3> _normals;
        private NativeArray<float4> _tangents;
        private NativeArray<Vector3> _normalsAsVector;
        private NativeArray<Vector4> _tangentsAsVector;
        private Mesh.MeshDataArray _meshDataArray;
        private Mesh.MeshData _mainMeshData;
        private Mesh.MeshData _tempMeshData;

        //compute buffer for passing data into shaders
        private ComputeBuffer _normalsOutBuffer;
        private ComputeBuffer _tangentsOutBuffer;
        private bool _isComputeBuffersCreated;

        private void Update()
        {
            if (Input.GetKey(KeyCode.Space))
                RecalculateNormals();
        }

        private void Awake()
        {
            CacheComponents();
        }

        private void Start()
        {
            AllocateNativeContainers();

            if (CalculateMethod == NormalRecalculateMethodEnum.CachedLite && _dataCache == null)
                Debug.LogWarning("Cached Normal Calculate Method Needs Data File");

            if (NormalOutputTarget == NormalOutputEnum.WriteToMesh)
            {
                _mesh.MarkDynamic();
            }
            else if (NormalOutputTarget == NormalOutputEnum.WriteToMaterial)
            {
                _normalsOutBuffer = new ComputeBuffer(_mesh.vertexCount, sizeof(float) * 3);
                _tangentsOutBuffer = new ComputeBuffer(_mesh.vertexCount, sizeof(float) * 4);
                _isComputeBuffersCreated = true;

                //duplicate all materials
                for (int i = 0; i < _renderer.materials.Length; i++)
                {
                    _renderer.materials[i] = new Material(_renderer.materials[i]);

                    _renderer.materials[i].SetBuffer("normalsOutBuffer", _normalsOutBuffer);
                    _renderer.materials[i].SetFloat("_Initialized", 1);
                }

                _normalsOutBuffer.SetData(_normals);
                _tangentsOutBuffer.SetData(_tangents);
            }

            if (RecalculateOnStart)
                RecalculateNormals();
        }

        private void CacheComponents()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer is SkinnedMeshRenderer smr)
            {
                _mesh = smr.sharedMesh;
            }
            else if (_renderer is MeshRenderer)
            {
                _mesh = GetComponent<MeshFilter>().sharedMesh;
            }
        }

        private void AllocateNativeContainers()
        {
            if (CalculateMethod == NormalRecalculateMethodEnum.CachedLite)
            {
                _nativeDuplicatesData = new UnsafeList<NativeArray<int>>(_dataCache.DuplicatesData.Count, Allocator.Persistent);
                foreach (var data in _dataCache.DuplicatesData)
                {
                    _nativeDuplicatesData.Add(new NativeArray<int>(data.DuplicateVertices, Allocator.Persistent));
                }
            }

            _tempMesh = new Mesh();

            _normals = new NativeArray<float3>(_mesh.vertexCount, Allocator.Persistent);
            _tangents = new NativeArray<float4>(_mesh.vertexCount, Allocator.Persistent);

            _normalsAsVector = _normals.Reinterpret<Vector3>();
            _tangentsAsVector = _tangents.Reinterpret<Vector4>();

            _meshDataArray = Mesh.AcquireReadOnlyMeshData(new[] { _mesh, _tempMesh });
            _mainMeshData = _meshDataArray[0];
            _tempMeshData = _meshDataArray[1];

            _mainMeshData.GetNormals(_normalsAsVector);
            _mainMeshData.GetTangents(_tangentsAsVector);
        }

        private void OnDestroy()
        {
            //Compute buffers need to be destroyed
            if (_isComputeBuffersCreated)
            {
                _normalsOutBuffer.Release();
                _tangentsOutBuffer.Release();
            }

            _normals.Dispose();
            _tangents.Dispose();
            _meshDataArray.Dispose();


            if (CalculateMethod == NormalRecalculateMethodEnum.CachedLite)
            {
                foreach (var nativeArray in _nativeDuplicatesData)
                {
                    nativeArray.Dispose();
                }

                _nativeDuplicatesData.Dispose();
            }
        }

        [ContextMenu("RecalculateNormals")]
        public void RecalculateNormals()
        {
            if (CalculateMethod == NormalRecalculateMethodEnum.SDBursted)
                RecalculateSDBursted();

            else if (CalculateMethod == NormalRecalculateMethodEnum.CachedLite)
                RecalculateCachedLite();
        }

        private void RecalculateSDBursted()
        {
            if (CalculateBlendShapes && _renderer is SkinnedMeshRenderer smr)
            {
                var tempObj = Instantiate(ModelPrefab);
                var tempSmr = tempObj.GetComponentInChildren<SkinnedMeshRenderer>();

                tempSmr.sharedMesh = _mesh;
                for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
                {
                    tempSmr.SetBlendShapeWeight(i, smr.GetBlendShapeWeight(i));
                }

                tempSmr.BakeMesh(_tempMesh);

                SDBurstedMethod.CalculateNormalData(_tempMeshData, SmoothingAngle, ref _normals, ref _tangents);
                Destroy(tempObj);
            }
            else
            {
                SDBurstedMethod.CalculateNormalData(_mainMeshData, SmoothingAngle, ref _normals, ref _tangents);
            }

            SetNormalsAndTangents(_normals, _tangents);
        }

        private void RecalculateCachedLite()
        {
            var mapCount = _dataCache.DuplicatesData.Count;

            if (mapCount == 0)
            {
                Debug.Log("Mesh Data of " + _mesh.name + " not found. Please do not forget the cache data on context menu or on start method before recalculating normals");
                return;
            }

            if (CalculateBlendShapes && _renderer is SkinnedMeshRenderer smr)
            {
                var tempObj = Instantiate(ModelPrefab);
                var tempSmr = tempObj.GetComponentInChildren<SkinnedMeshRenderer>();
                tempSmr.sharedMesh = _mesh;

                for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
                {
                    tempSmr.SetBlendShapeWeight(i, smr.GetBlendShapeWeight(i));
                }

                tempSmr.BakeMesh(_tempMesh);
                _tempMeshData.GetNormals(_normalsAsVector);
                _tempMeshData.GetTangents(_tangentsAsVector);
            }
            else
            {
                _mainMeshData.GetNormals(_normalsAsVector);
                _mainMeshData.GetTangents(_tangentsAsVector);
            }

            CachedLiteMethod.NormalizeDuplicateVertices(_nativeDuplicatesData, ref _normals, ref _tangents);

            SetNormalsAndTangents(_normals, _tangents);
        }

        private void SetNormalsAndTangents(NativeArray<float3> normals, NativeArray<float4> tangents)
        {
            if (NormalOutputTarget == NormalOutputEnum.WriteToMesh)
            {
                _mesh.SetNormals(normals);
                _mesh.SetTangents(tangents);
            }
            else if (NormalOutputTarget == NormalOutputEnum.WriteToMaterial)
            {
                _normalsOutBuffer.SetData(normals);
                _tangentsOutBuffer.SetData(tangents);
            }
        }
    }
}