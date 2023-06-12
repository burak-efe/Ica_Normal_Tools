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
            SDBursted,
            CachedParallel
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

        [SerializeField] [Tooltip("Data cache asset required when using cached method. You can create this on project tab context menu/plugins /Mesh data cache.")]
        private MeshDataCache _dataCache;

        [Tooltip("Asset of this model in zero pose. Only necessary when using Calculate Blend Shapes option")] public GameObject ModelPrefab;

        private Renderer _renderer;
        private Mesh _mesh;
        private Mesh _tempMesh;
        private GameObject _tempObj;
        private SkinnedMeshRenderer _tempSmr;

        private UnsafeList<NativeArray<int>> _nativeDuplicatesData;
        private NativeArray<int> _nativeAdjacencyList;
        private NativeArray<int2> _nativeAdjacencyMap;
        private NativeArray<int> _indices;

        private NativeArray<float3> _normals;
        private NativeArray<float4> _tangents;
        private NativeArray<Vector3> _normalsAsVector;
        private NativeArray<Vector4> _tangentsAsVector;
        private Mesh.MeshDataArray _meshDataArray;
        private Mesh.MeshData _mainMeshData;


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

            if (CalculateBlendShapes && _renderer is SkinnedMeshRenderer smr)
            {
                _tempObj = Instantiate(ModelPrefab, transform);
                _tempSmr = GetComponentInChildren<SkinnedMeshRenderer>();
                _tempObj.SetActive(false);
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
                    _nativeDuplicatesData.Add(new NativeArray<int>(data.Value, Allocator.Persistent));
                }
            }

            if (CalculateMethod == NormalRecalculateMethodEnum.CachedParallel)
            {
                _nativeAdjacencyList = new NativeArray<int>(_dataCache.AdjacencyList, Allocator.Persistent);
                _nativeAdjacencyMap = new NativeArray<int2>(_dataCache.AdjacencyMapper, Allocator.Persistent);
                _indices = new NativeArray<int>(_mesh.triangles, Allocator.Persistent);
            }

            _tempMesh = new Mesh();

            _normals = new NativeArray<float3>(_mesh.vertexCount, Allocator.Persistent);
            _tangents = new NativeArray<float4>(_mesh.vertexCount, Allocator.Persistent);

            _normalsAsVector = _normals.Reinterpret<Vector3>();
            _tangentsAsVector = _tangents.Reinterpret<Vector4>();

            _meshDataArray = Mesh.AcquireReadOnlyMeshData(_mesh);
            _mainMeshData = _meshDataArray[0];

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
                    nativeArray.Dispose();

                _nativeDuplicatesData.Dispose();
            }

            if (CalculateMethod == NormalRecalculateMethodEnum.CachedParallel)
            {
                _nativeAdjacencyList.Dispose();
                _nativeAdjacencyMap.Dispose();
                _indices.Dispose();
            }
        }

        [ContextMenu("RecalculateNormals")]
        public void RecalculateNormals()
        {
            if (CalculateMethod == NormalRecalculateMethodEnum.SDBursted)
                RecalculateSDBursted();
            else if (CalculateMethod == NormalRecalculateMethodEnum.CachedLite)
                RecalculateCachedLite();
            else if (CalculateMethod == NormalRecalculateMethodEnum.CachedParallel)
                RecalculateCachedParallel();
        }

        private void RecalculateSDBursted()
        {
            if (CalculateBlendShapes && _renderer is SkinnedMeshRenderer smr)
            {
                SmrUtils.CopyBlendShapes(smr,_tempSmr);
                _tempSmr.BakeMesh(_tempMesh);
                var mda = Mesh.AcquireReadOnlyMeshData(_tempMesh);
                SDBurstedMethod.CalculateNormalData(mda[0], SmoothingAngle, ref _normals, ref _tangents);
                mda.Dispose();
            }
            else
            {
                SDBurstedMethod.CalculateNormalData(_mainMeshData, SmoothingAngle, ref _normals, ref _tangents);
            }

            SetNormalsAndTangents(_normals, _tangents);
        }

        private void RecalculateCachedLite()
        {
            if (_dataCache.DuplicatesData == null)
            {
                Debug.LogWarning("Mesh Data of " + _mesh.name + " not found. Please do not forget the cache data on context menu or on start method before recalculating normals");
                return;
            }

            if (CalculateBlendShapes && _renderer is SkinnedMeshRenderer smr)
            {
                SmrUtils.CopyBlendShapes(smr,_tempSmr);
                _tempSmr.BakeMesh(_tempMesh);
                
                _tempMesh.RecalculateNormals();
                _tempMesh.RecalculateTangents();

                var mda = Mesh.AcquireReadOnlyMeshData(_tempMesh);
                mda[0].GetNormals(_normalsAsVector);
                mda[0].GetTangents(_tangentsAsVector);
                mda.Dispose();
            }
            else
            {
                _mainMeshData.GetNormals(_normalsAsVector);
                _mainMeshData.GetTangents(_tangentsAsVector);
            }
            CachedLiteMethod.NormalizeDuplicateVertices(_nativeDuplicatesData, ref _normals, ref _tangents);
            SetNormalsAndTangents(_normals, _tangents);
        }


        private void RecalculateCachedParallel()
        {
            if (CalculateBlendShapes && _renderer is SkinnedMeshRenderer smr)
            {
                SmrUtils.CopyBlendShapes(smr,_tempSmr);
                _tempSmr.BakeMesh(_tempMesh);
                var mda = Mesh.AcquireReadOnlyMeshData(_tempMesh);
                mda[0].GetNormals(_normalsAsVector);
                mda[0].GetTangents(_tangentsAsVector);
                CachedParallelMethod.CalculateNormalData(mda[0], _dataCache.IndicesCount, _indices, ref _normals, ref _tangents, _nativeAdjacencyList, _nativeAdjacencyMap);
                mda.Dispose();
            }
            else
            {
                CachedParallelMethod.CalculateNormalData(_mainMeshData, _dataCache.IndicesCount, _indices, ref _normals, ref _tangents, _nativeAdjacencyList, _nativeAdjacencyMap);
            }
            SetNormalsAndTangents(_normals,_tangents);
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