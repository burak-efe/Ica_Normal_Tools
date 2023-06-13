using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;


namespace IcaNormal
{
    public static class MeshAdjacency
    {
        public static void CalculateAdjacencyData 
        (NativeArray<float3> vertices, NativeList<int> indices, UnsafeHashMap<float3, NativeList<int>> posGraph,
            out NativeArray<int> adjacencyList, out NativeArray<int2> adjacencyMapper, Allocator allocator)
        {

            var tempAdjData = new UnsafeList<NativeList<int>>(vertices.Length, Allocator.Temp);

            for (int i = 0; i < vertices.Length; i++)
            {
                tempAdjData.Add(new NativeList<int>(Allocator.Temp));
            }

            //for every triangle
            for (int index = 0; index < indices.Length; index += 3)
            {
                var triIndex = index / 3;
                //for three vertex of triangle
                for (int j = 0; j < 3; j++)
                {
                    var subVertexOfTriangle = indices[index + j];

                    foreach (int vertexIndex in posGraph[vertices[subVertexOfTriangle]])
                    {
                        if (!tempAdjData[vertexIndex].Contains(triIndex))
                        {
                            tempAdjData[vertexIndex].Add(triIndex);
                        }
                    }
                }
            }

            var unrolledList = new NativeList<int>(Allocator.Temp);
            adjacencyMapper = new NativeArray<int2>(vertices.Length, allocator);

            int currentStartIndex = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                int size = tempAdjData[i].Length;
                unrolledList.AddRange(tempAdjData[i].AsArray());
                adjacencyMapper[i] = new int2(currentStartIndex, size);
                currentStartIndex += size;
            }

            adjacencyList = new NativeArray<int>(unrolledList.AsArray(), allocator);
        }



        public static List<DuplicateVerticesList> GetManagedDuplicateVerticesMap(UnsafeList<NativeArray<int>> from)
        {
            var list = new List<DuplicateVerticesList>(from.Length);
            foreach (var fromArray in from)
            {
                var managed = new DuplicateVerticesList
                {
                    Value = fromArray.ToArray()
                };

                list.Add(managed);
            }

            return list;
        }

        public static UnsafeList<NativeArray<int>> GetDuplicateVerticesMap(UnsafeHashMap<float3, NativeList<int>> posGraph, Allocator allocator)
        {
            var map = new UnsafeList<NativeArray<int>>(10, allocator);

            foreach (var kvp in posGraph)
            {
                if (kvp.Value.Length > 1)
                {
                    map.Add(new NativeArray<int>(kvp.Value.AsArray(), allocator));
                }
            }

            Debug.Log("Number of Duplicate Vertices Cached: " + map.Length);
            return map;
        }

        
        public static UnsafeHashMap<float3, NativeList<int>> GetVertexPosGraph(NativeArray<float3> vertices, Allocator allocator)
        {
            //var vertices = mesh.vertices;
            var graph = new UnsafeHashMap<float3, NativeList<int>>(vertices.Length, allocator);

            for (int vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex++)
            {
                NativeList<int> entryList;

                if (!graph.TryGetValue(vertices[vertexIndex], out entryList))
                {
                    entryList = new NativeList<int>(allocator);
                    graph.Add(vertices[vertexIndex], entryList);
                }

                entryList.Add(vertexIndex);
            }

            return graph;
        }
    }
}