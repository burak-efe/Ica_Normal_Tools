using System;
using System.Collections.Generic;
using UnityEngine;

namespace IcaNormal
{
    [CreateAssetMenu(menuName = "Plugins/IcaNormalRecalculation/MeshDataCache",fileName = "IcaMeshDataCache")]
    [PreferBinarySerialization]
    public class IcaMeshDataCache :ScriptableObject
    {
        public Mesh TargetMesh;
        
        [SerializeField, HideInInspector] public List<DuplicateMap> DuplicatesData;
        [SerializeField, HideInInspector] public List<Vector3> NormalsList;
        [SerializeField, HideInInspector] public List<Vector4> TangentsList;

        [ContextMenu("CacheData")]
        public void CacheData()
        {
            DuplicatesData = GetDuplicateVerticesMap(TargetMesh);
            TargetMesh.GetNormals(NormalsList);
            TargetMesh.GetTangents(TangentsList);
        }
        
        [Serializable]
        public struct DuplicateMap
        {
            public List<int> DuplicateIndexes;
        }

        public static  List<DuplicateMap> GetDuplicateVerticesMap(Mesh mesh)
        {
            var vertices = mesh.vertices;
            var tempMap = new Dictionary<Vector3, List<int>>(mesh.vertexCount);
            var map = new List<DuplicateMap>();

            for (int vertexIndex = 0; vertexIndex < mesh.vertexCount; vertexIndex++)
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
            return map;
        }
    }
}