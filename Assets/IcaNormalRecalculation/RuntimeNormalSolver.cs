using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;

namespace IcaNormal
{
    [RequireComponent(typeof(Renderer))]
    public class RuntimeNormalSolver : MonoBehaviour
    {
        public enum NormalRecalculateMethodEnum
        {
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

        [Range(0, 180)]
        [Tooltip("Smoothing angle only usable with bursted method")]
        public float SmoothingAngle = 120f;

        public bool RecalculateOnStart;
        public bool RecalculateTangents;
        public bool CalculateBlendShapes;

        [FormerlySerializedAs("_dataCache")]
        [SerializeField]
        [Tooltip("Data cache asset required when using cached method. You can create this on project tab context menu/plugins /Mesh data cache.")]
        private MeshDataCacheAsset _dataCacheAsset;

        [Tooltip("Asset of this model in zero pose. Only necessary when using Calculate Blend Shapes option")]
        public GameObject ModelPrefab;

        public List<SkinnedMeshRenderer> SMRs;

        public List<GameObject> Prefabs;
        //public List<GameObject> Prefabs;
        
        //
        private Renderer _smr;
        private Mesh _mesh;
        private Mesh _tempMesh;
        private GameObject _tempObj;
        private SkinnedMeshRenderer _tempSmr;

        private MeshDataCache _meshData;
        // private NativeArray<int> _nativeAdjacencyList;
        // private NativeArray<int2> _nativeAdjacencyMap;
        // private NativeArray<int> _indices;
        // private NativeArray<float3> _vertices;
        // private NativeArray<float3> _normals;
        // private NativeArray<float4> _tangents;
        // private NativeArray<float2> _uvs;
        //
        // private Mesh.MeshDataArray _meshDataArray;
        // private Mesh.MeshData _mainMeshData;

        //compute buffer for passing data into shaders
        private ComputeBuffer _normalsOutBuffer;
        private ComputeBuffer _tangentsOutBuffer;
        private bool _isComputeBuffersCreated;

        private void Start()
        {
            //_meshData = new MeshDataCache();
            CacheComponents();
            InitNativeContainers();

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
                for (int i = 0; i < _smr.materials.Length; i++)
                {
                    _smr.materials[i] = new Material(_smr.materials[i]);
                    _smr.materials[i].SetBuffer("normalsOutBuffer", _normalsOutBuffer);
                    _smr.materials[i].SetBuffer("tangentsOutBuffer", _tangentsOutBuffer);
                    _smr.materials[i].SetFloat("_Initialized", 1);
                }

                _normalsOutBuffer.SetData(_meshData._normals.AsArray());
                _tangentsOutBuffer.SetData(_meshData._tangents.AsArray());
            }

            if (CalculateBlendShapes)
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
            _smr = GetComponent<Renderer>();
            if (_smr is SkinnedMeshRenderer smr)
                _mesh = smr.sharedMesh;
            else if (_smr is MeshRenderer)
                _mesh = GetComponent<MeshFilter>().sharedMesh;
        }

        private void InitNativeContainers()
        {
            // _tempMesh = new Mesh();
            // _meshDataArray = Mesh.AcquireReadOnlyMeshData(_mesh);
            // _mainMeshData = _meshDataArray[0];
            // _vertices = new NativeArray<float3>(_mesh.vertexCount, Allocator.Persistent);
            // _normals = new NativeArray<float3>(_mesh.vertexCount, Allocator.Persistent);
            // _tangents = new NativeArray<float4>(_mesh.vertexCount, Allocator.Persistent);
            // _uvs = new NativeArray<float2>(_mesh.vertexCount, Allocator.Persistent);
            // _mainMeshData.GetNormals(_normals.Reinterpret<Vector3>());
            // _mainMeshData.GetTangents(_tangents.Reinterpret<Vector4>());
            // _mainMeshData.GetUVs(0, _uvs.Reinterpret<Vector2>());
            //
            // if (CalculateMethod == NormalRecalculateMethodEnum.CachedParallel)
            // {
            //     if (_dataCacheAsset != null)
            //     {
            //         Profiler.BeginSample("GetCacheDataFromAsset");
            //         _nativeAdjacencyList = new NativeArray<int>(_dataCacheAsset.SerializedAdjacencyList, Allocator.Persistent);
            //         _nativeAdjacencyMap = new NativeArray<int2>(_dataCacheAsset.SerializedAdjacencyMapper, Allocator.Persistent);
            //         _indices = new NativeArray<int>(_dataCacheAsset.SerializedIndices, Allocator.Persistent);
            //         Profiler.EndSample();
            //     }
            //     else
            //     {
            //         CalculateCache();
            //     }
            // }
        }

        private void OnDestroy()
        {
            //Compute buffers need to be destroyed
            if (_isComputeBuffersCreated)
            {
                _normalsOutBuffer.Release();
                _tangentsOutBuffer.Release();
            }
            _meshData.Dispose();

            // _vertices.Dispose();
            // _normals.Dispose();
            // _tangents.Dispose();
            // _meshDataArray.Dispose();
            // _uvs.Dispose();
            //
            // if (CalculateMethod == NormalRecalculateMethodEnum.CachedParallel)
            // {
            //     _nativeAdjacencyList.Dispose();
            //     _nativeAdjacencyMap.Dispose();
            //     _indices.Dispose();
            // }
        }

        [ContextMenu("RecalculateNormals")]
        public void RecalculateNormals()
        {
            if (CalculateMethod == NormalRecalculateMethodEnum.SDBursted)
                RecalculateSDBursted();
            else if (CalculateMethod == NormalRecalculateMethodEnum.CachedParallel)
                RecalculateCachedParallel();
        }

        // public void CalculateCache()
        // {
        //     Profiler.BeginSample("CalculateCacheData");
        //
        //     Profiler.BeginSample("GetIndices");
        //     _mainMeshData.GetAllIndices(out _indices, Allocator.Persistent);
        //     Profiler.EndSample();
        //
        //     Profiler.BeginSample("GetVertices");
        //     var tempVertices = new NativeArray<float3>(_mainMeshData.vertexCount, Allocator.Temp);
        //     _mainMeshData.GetVertices(tempVertices.Reinterpret<Vector3>());
        //     Profiler.EndSample();
        //
        //     Profiler.BeginSample("GetVertexPosHashMap");
        //     VertexPositionMapper.GetVertexPosHashMap(in tempVertices, out var tempPosGraph, Allocator.Temp);
        //     Profiler.EndSample();
        //
        //     Profiler.BeginSample("GetAdjacency");
        //     AdjacencyMapper.CalculateAdjacencyData(in tempVertices, in _indices, in tempPosGraph, out _nativeAdjacencyList, out _nativeAdjacencyMap, Allocator.Persistent);
        //     Profiler.EndSample();
        //
        //     Profiler.EndSample();
        // }

        private void RecalculateSDBursted()
        {
            if (CalculateBlendShapes)
            {
                
                SmrUtils.CopyBlendShapes((SkinnedMeshRenderer)_smr, _tempSmr);
                _tempSmr.BakeMesh(_tempMesh);
                var mda = Mesh.AcquireReadOnlyMeshData(_tempMesh);
                SDBurstedMethod.CalculateNormalData(mda[0], SmoothingAngle, ref _meshData._normals, ref _meshData._tangents);
                mda.Dispose();
            }
            else
            {
                SDBurstedMethod.CalculateNormalData(_meshData._mainMeshData, SmoothingAngle, ref _meshData._normals, ref _meshData._tangents);
            }

            SetNormals(_meshData._normals);
            SetTangents(_meshData._tangents);
        }

        private void RecalculateCachedParallel()
        {
           // UpdateNativeVertices();
            
            CachedParallelMethod.CalculateNormalData(_meshData._vertices, _meshData._indices, ref _meshData._normals, _meshData._nativeAdjacencyList, _meshData._nativeAdjacencyMap);
            SetNormals(_meshData._normals);
            
            if (RecalculateTangents)
            {
                CachedParallelMethod.CalculateTangentData(_meshData._vertices, _meshData._normals, _meshData._indices, _meshData._uvs, _meshData._nativeAdjacencyList, _meshData._nativeAdjacencyMap, ref _meshData._tangents);
                SetTangents(_meshData._tangents);
            }
        }

        private void SetNormals(NativeList<float3> normals)
        {
            if (NormalOutputTarget == NormalOutputEnum.WriteToMesh)
            {
                Profiler.BeginSample("WriteToMesh");
                _mesh.SetNormals(normals.AsArray());
                Profiler.EndSample();
            }
            else if (NormalOutputTarget == NormalOutputEnum.WriteToMaterial)
            {
                Profiler.BeginSample("WriteToMaterial");
                _normalsOutBuffer.SetData(normals.AsArray());
                Profiler.EndSample();
            }
        }

        private void SetTangents(NativeList<float4> tangents)
        {
            if (NormalOutputTarget == NormalOutputEnum.WriteToMesh)
            {
                Profiler.BeginSample("WriteToMesh");
                _mesh.SetTangents(tangents.AsArray());
                Profiler.EndSample();
            }
            else if (NormalOutputTarget == NormalOutputEnum.WriteToMaterial)
            {
                Profiler.BeginSample("WriteToMaterial");
                _tangentsOutBuffer.SetData(tangents.AsArray());
                Profiler.EndSample();
            }
        }

        public void TangentsOnlyTest()
        {
           // UpdateNativeVerticesFromMeshData(_mainMeshData);
            CachedParallelMethod.CalculateTangentData(_meshData._vertices, _meshData._normals, _meshData._indices, _meshData._uvs, _meshData._nativeAdjacencyList, _meshData._nativeAdjacencyMap, ref _meshData._tangents);
            SetTangents(_meshData._tangents);
        }

        // private void UpdateNativeVertices()
        // {
        //     if (CalculateBlendShapes )
        //     {
        //         Profiler.BeginSample("GetSMRData");
        //         SmrUtils.CopyBlendShapes((SkinnedMeshRenderer)_smr, _tempSmr);
        //         _tempSmr.BakeMesh(_tempMesh);
        //         var mda = Mesh.AcquireReadOnlyMeshData(_tempMesh);
        //         UpdateNativeVerticesFromMeshData(mda[0]);
        //         mda.Dispose();
        //         Profiler.EndSample();
        //     }
        //     else
        //     {
        //         UpdateNativeVerticesFromMeshData(_mainMeshData);
        //     }
        // }

        // private void UpdateNativeVerticesFromMeshData(Mesh.MeshData data)
        // {
        //     data.GetVertices(_vertices.Reinterpret<Vector3>());
        // }
    }
}