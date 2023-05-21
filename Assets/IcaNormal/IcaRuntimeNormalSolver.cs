using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace IcaNormal
{
    [RequireComponent(typeof(Renderer))]
    public class IcaRuntimeNormalSolver : MonoBehaviour
    {
        [Serializable]
        public struct DuplicateMap
        {
            public List<int> DuplicateIndexes;
        }

        public enum NormalRecalculateMethodEnum
        {
            Cached,
            Bursted,
        }

        public enum NormalOutputEnum
        {
            WriteToMesh,
            WriteToMaterial
        }

        public NormalRecalculateMethodEnum Method = NormalRecalculateMethodEnum.Cached;
        public NormalOutputEnum NormalOutputTarget = NormalOutputEnum.WriteToMesh;
        [Range(0, 180)] public float SmoothingAngle = 120f;
        public bool RecalculateOnStart;
        public bool CalculateBlendShapes;
        public GameObject ModelPrefab;

        [SerializeField, HideInInspector] private List<DuplicateMap> map;

        private Renderer _renderer;
        private Mesh _mesh;
        private List<Vector3> _normalsList;
        private Vector3[] _normals;
        private Vector4[] _tangents;

        private ComputeBuffer _normalsOutBuffer;
        private ComputeBuffer _tangentsOutBuffer;
        private Mesh _tempMesh;


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                RecalculateNormals();
            }
        }

        private void Start()
        {
            Init();
            
            _normalsList = new List<Vector3>(_mesh.vertexCount);
            _normals = new Vector3[_mesh.vertexCount];
            _tangents = new Vector4[_mesh.vertexCount];
            _tempMesh = new Mesh();

            if (NormalOutputTarget == NormalOutputEnum.WriteToMesh)
            {
                _mesh.MarkDynamic();
            }
            else if (NormalOutputTarget == NormalOutputEnum.WriteToMaterial)
            {
                _normalsOutBuffer = new ComputeBuffer(_mesh.vertexCount, sizeof(float) * 3);
                _tangentsOutBuffer = new ComputeBuffer(_mesh.vertexCount, sizeof(float) * 4);

                _mesh.GetNormals(_normalsList);
                _normals = _normalsList.ToArray();
                _normalsOutBuffer.SetData(_normals);


                for (int i = 0; i < _renderer.materials.Length; i++)
                {
                    _renderer.materials[i] = new Material(_renderer.materials[i]);

                    _renderer.materials[i].SetBuffer("normalsOutBuffer", _normalsOutBuffer);
                    _renderer.materials[i].SetFloat("_Initialized", 1);
                }
            }

            if (RecalculateOnStart)
            {
                RecalculateNormals();
            }
        }

        private void Init()
        {
            _renderer = GetComponent<Renderer>();

            if (_renderer is SkinnedMeshRenderer smr)
            {
                _mesh = smr.sharedMesh;
            }
            else if (_renderer is MeshRenderer mr)
            {
                _mesh = GetComponent<MeshFilter>().sharedMesh;
            }
            
        }

        private void OnDestroy()
        {
            if (NormalOutputTarget == NormalOutputEnum.WriteToMaterial)
            {
                _normalsOutBuffer.Release();
                _tangentsOutBuffer.Release();
            }
        }
#if UNITY_EDITOR
        private void Reset()
        {
            CacheVertices();
        }
#endif


        [ContextMenu("CacheVertices")]
        public void CacheVertices()
        {
            Init();
            var vertices = _mesh.vertices;
            var tempMap = new Dictionary<Vector3, List<int>>(_mesh.vertexCount);
            map = new List<DuplicateMap>();

            for (int vertexIndex = 0; vertexIndex < _mesh.vertexCount; vertexIndex++)
            {
                List<int> entryList;

                if (!tempMap.TryGetValue(vertices[vertexIndex], out entryList))
                {
                    entryList = new List<int>();
                    tempMap.Add(vertices[vertexIndex], entryList);
                }

                entryList.Add(vertexIndex);
            }

            foreach (var kvp in tempMap)
            {
                if (kvp.Value.Count > 1)
                {
                    map.Add(new DuplicateMap { DuplicateIndexes = kvp.Value });
                }
            }

            Debug.Log("Number of Duplicate Vertices Cached: " + map.Count);
        }

        [ContextMenu("RecalculateNormals")]
        public void RecalculateNormals()
        {
            if (Method == NormalRecalculateMethodEnum.Bursted)
            {
                RecalculateBursted();
            }
            else if (Method == NormalRecalculateMethodEnum.Cached)
            {
                RecalculateCached();
            }
        }

        private void RecalculateBursted()
        {
            Profiler.BeginSample("RecalculateBursted");

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

                IcaNormalSolverUtils.RecalculateNormals(_tempMesh, SmoothingAngle, ref _normals, ref _tangents);
                Destroy(tempObj);
            }
            else
            {
                IcaNormalSolverUtils.RecalculateNormals(_mesh, SmoothingAngle, ref _normals, ref _tangents);
            }
            
            
            if (NormalOutputTarget == NormalOutputEnum.WriteToMesh)
            {
                _mesh.SetNormals(_normals);
                _mesh.SetTangents(_tangents);
            }
            else if (NormalOutputTarget == NormalOutputEnum.WriteToMaterial)
            {
                _normalsOutBuffer.SetData(_normals);
            }
            Profiler.EndSample();
            
        }

        private void RecalculateCached()
        {
            Profiler.BeginSample("RecalculateCached");
            var mapCount = map.Count;

            if (mapCount == 0)
            {
                Debug.Log("Mesh Data of " + _mesh.name + " not found. Please do not forget the cache data on contex menu or on start method before recalculating normals");
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

                _tempMesh.RecalculateNormals();
                _tempMesh.GetNormals(_normalsList);
                Destroy(tempObj);
            }
            else
            {
                _tempMesh = _mesh;

                _tempMesh.RecalculateNormals();
                _tempMesh.GetNormals(_normalsList);
            }


            for (int listIndex = 0; listIndex < mapCount; listIndex++)
            {
                Vector3 sum = Vector3.zero;
                var listCount = map[listIndex].DuplicateIndexes.Count;

                for (int i = 0; i < listCount; i++)
                {
                    sum += _normalsList[map[listIndex].DuplicateIndexes[i]];
                }

                sum = sum.normalized;

                for (int i = 0; i < listCount; i++)
                {
                    _normalsList[map[listIndex].DuplicateIndexes[i]] = sum;
                }
            }

            if (NormalOutputTarget == NormalOutputEnum.WriteToMesh)
            {
                _mesh.SetNormals(_normalsList);
            }
            else if (NormalOutputTarget == NormalOutputEnum.WriteToMaterial)
            {
                _normalsOutBuffer.SetData(_normalsList);
            }
            Profiler.EndSample();
        }
    }
}