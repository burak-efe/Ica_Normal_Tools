using System;
using System.Collections.Generic;
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
            Cached,
            Bursted
        }

        public enum NormalOutputEnum
        {
            WriteToMesh,
            WriteToMaterial
        }

        public NormalRecalculateMethodEnum CalculateMethod = NormalRecalculateMethodEnum.Bursted;
        public NormalOutputEnum NormalOutputTarget = NormalOutputEnum.WriteToMesh;

        [Tooltip("Smoothing angle only usable with bursted method")] 
        [Range(0, 180)] 
        public float SmoothingAngle = 120f;

        public bool RecalculateOnStart;

        public bool CalculateBlendShapes;

        [Tooltip("Data cache asset required when using cached method. You can create this on project tab context menu/plugins /Mesh data cache.")] 
        [SerializeField]
        private MeshDataCache _dataCache;

        [Tooltip("Asset of this model in zero pose. Only necessary when using Calculate Blend Shapes option")] 
        public GameObject ModelPrefab;

        private Renderer _renderer;
        private Mesh _mesh;
        private List<Vector3> _normalsList;
        private List<Vector4> _tangentsList;
        private Vector3[] _normals;
        private Vector4[] _tangents;

        //compute buffer for passing data into shaders
        private ComputeBuffer _normalsOutBuffer;
        private ComputeBuffer _tangentsOutBuffer;
        private bool _isComputeBuffersCreated;

        private Mesh _tempMesh;

        private void Awake()
        {
            CacheComponents();
        }

        private void Start()
        {
            _normalsList = new List<Vector3>(_mesh.vertexCount);
            _tangentsList = new List<Vector4>(_mesh.vertexCount);
            _mesh.GetNormals(_normalsList);
            _mesh.GetTangents(_tangentsList);
            _normals = _normalsList.ToArray();
            _tangents = _tangentsList.ToArray();
            _tempMesh = new Mesh();

            if (CalculateMethod == NormalRecalculateMethodEnum.Cached && _dataCache == null) 
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

        private void OnDestroy()
        {
            //Compute buffers need to be destroyed
            if (_isComputeBuffersCreated)
            {
                _normalsOutBuffer.Release();
                _tangentsOutBuffer.Release();
            }
        }

        [ContextMenu("RecalculateNormals")]
        public void RecalculateNormals()
        {
            if (CalculateMethod == NormalRecalculateMethodEnum.Bursted)
            {
                RecalculateBursted();
            }
            else if (CalculateMethod == NormalRecalculateMethodEnum.Cached)
            {
                RecalculateCached();
            }
        }

        private void RecalculateBursted()
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

                FullMethod.CalculateNormalData(_tempMesh, SmoothingAngle, ref _normals, ref _tangents);
                Destroy(tempObj);
            }
            else
            {
                FullMethod.CalculateNormalData(_mesh, SmoothingAngle, ref _normals, ref _tangents);
            }

            if (NormalOutputTarget == NormalOutputEnum.WriteToMesh)
            {
                _mesh.SetNormals(_normals);
                _mesh.SetTangents(_tangents);
            }
            else if (NormalOutputTarget == NormalOutputEnum.WriteToMaterial)
            {
                _normalsOutBuffer.SetData(_normals);
                _tangentsOutBuffer.SetData(_tangents);
            }
        }

        private void RecalculateCached()
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
            }
            else
            {
                _tempMesh = _mesh;
            }
            
            CachedMethod.CalculateNormalData(_tempMesh,SmoothingAngle,_dataCache.DuplicatesData, ref _normals,ref _tangents);
            
            if (NormalOutputTarget == NormalOutputEnum.WriteToMesh)
            {
                _mesh.SetNormals(_normals);
                _mesh.SetTangents(_tangents);
            }
            else if (NormalOutputTarget == NormalOutputEnum.WriteToMaterial)
            {
                _normalsOutBuffer.SetData(_normals);
                _tangentsOutBuffer.SetData(_tangents);
            }
        }
        
    }
}