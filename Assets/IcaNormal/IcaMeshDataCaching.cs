using System;
using System.Collections.Generic;
using UnityEngine;

namespace IcaNormal
{

    public static class IcaMeshDataCaching
    {
        [Serializable]
        public struct DuplicateMap
        {
            public List<int> DuplicateIndexes;
        }

        public static  List<DuplicateMap> GetDuplicateVerticesMap(Mesh mesh)
        {
            //Init();
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