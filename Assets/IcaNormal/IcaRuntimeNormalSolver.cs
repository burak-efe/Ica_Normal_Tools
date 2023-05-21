using System;
using System.Collections.Generic;
using UnityEngine;

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

        public enum NormalOutputEnum
        {
            WriteToMesh,
            WriteToMaterial
        }

        public NormalOutputEnum NormalOutputTarget = NormalOutputEnum.WriteToMesh;
        public bool RecalculateOnStart;
        public bool CalculateBlendShapes;
        public GameObject ModelPrefab;

        //public bool DuplicateMaterialsOnStart;
        [SerializeField, HideInInspector] private List<DuplicateMap> map;

        private Renderer _renderer;
        private Mesh _mesh;
        private List<Vector3> _normals;

        private ComputeBuffer _normalsOutBuffer;
        private ComputeBuffer _tangentsOutBuffer;


        private void Awake()
        {
            Init();
            _normals = new List<Vector3>(_mesh.vertexCount);

            if (NormalOutputTarget == NormalOutputEnum.WriteToMesh)
            {
                _mesh.MarkDynamic();
            }
            else if (NormalOutputTarget == NormalOutputEnum.WriteToMaterial)
            {
                _normalsOutBuffer = new ComputeBuffer(_mesh.vertexCount, sizeof(float) * 3);
                _tangentsOutBuffer = new ComputeBuffer(_mesh.vertexCount, sizeof(float) * 4);

                _mesh.GetNormals(_normals);
                _normalsOutBuffer.SetData(_normals.ToArray());


                for (int i = 0; i < _renderer.materials.Length; i++)
                {
                    _renderer.materials[i] = new Material(_renderer.materials[i]);

                    //var mat = new Material(_smr.materials[i]);
                    //var initId = Shader.PropertyToID("_Initialized");
                    //Debug.Log(initId);
                    //var bufferId = Shader.PropertyToID("normalsOutBuffer");
                    //Debug.Log(bufferId);


                    _renderer.materials[i].SetBuffer("normalsOutBuffer", _normalsOutBuffer);
                    _renderer.materials[i].SetFloat("_Initialized", 1);
                    //mat.SetBuffer("_tangentsOutBuffer",_tangentsOutBuffer);
                    //_smr.materials[i].SetColor("_Color",Color.blue);
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

            //_mesh = _renderer ;
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
            var mapCount = map.Count;

            if (mapCount == 0)
            {
                Debug.Log("Mesh Data of " + _mesh.name + " not found. Please do not forget the cache data on contex menu or on start method before recalculating normals");
                return;
            }

            Mesh tempMesh = new Mesh();

            if (CalculateBlendShapes && _renderer is SkinnedMeshRenderer smr)
            {
                //var tempObj = new GameObject("tempMesh");
                //var tempSmr = tempObj.AddComponent<SkinnedMeshRenderer>();
                var tempObj = Instantiate(ModelPrefab);
                var tempSmr = tempObj.GetComponentInChildren<SkinnedMeshRenderer>();
                
                tempSmr.sharedMesh = _mesh;
                for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
                {
                    tempSmr.SetBlendShapeWeight(i,smr.GetBlendShapeWeight(i));
                }
                tempSmr.BakeMesh(tempMesh);
                
                tempMesh.RecalculateNormals();
                tempMesh.GetNormals(_normals);
                Destroy(tempObj);
            }
            else
            {
                tempMesh = _mesh;
                
                tempMesh.RecalculateNormals();
                tempMesh.GetNormals(_normals);
            }


   


            for (int listIndex = 0; listIndex < mapCount; listIndex++)
            {
                Vector3 sum = Vector3.zero;
                var listCount = map[listIndex].DuplicateIndexes.Count;

                for (int i = 0; i < listCount; i++)
                {
                    sum += _normals[map[listIndex].DuplicateIndexes[i]];
                }

                sum = sum.normalized;

                for (int i = 0; i < listCount; i++)
                {
                    _normals[map[listIndex].DuplicateIndexes[i]] = sum;
                }
            }

            if (NormalOutputTarget == NormalOutputEnum.WriteToMesh)
            {
                _mesh.SetNormals(_normals);
            }
            else if (NormalOutputTarget == NormalOutputEnum.WriteToMaterial)
            {
                _normalsOutBuffer.SetData(_normals.ToArray());
            }
        }
    }
}