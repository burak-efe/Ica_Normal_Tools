using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace IcaNormal
{
    public static class MeshCacheUtils
    {
        public static void CalculateAdjacencyData(Mesh mesh, Dictionary<Vector3, List<int>> posGraph, out int[] adjacencyList, out int2[] adjacencyMapper)
        {
            var triangles = mesh.triangles;
            var indicesCount = triangles.Length;
            var vertices = mesh.vertices;
            var tempAdjData = new List<List<int>>(mesh.vertexCount);

            for (int i = 0; i < mesh.vertexCount; i++)
            {
                tempAdjData.Add(new List<int>());
            }

            for (int index = 0; index < indicesCount; index += 3)
            {
                //var i0 = triangles[i];
                //var i1 = triangles[i + 1];
                //var i2 = triangles[i + 2];
                var triIndex = index / 3;

                //List<int> vertListOnSamePos;

                for (int j = 0; j < 3; j++)
                {
                    var subVertexOfTriangle = triangles[index + j];
                    
                    foreach (int vertexIndex in posGraph[vertices[subVertexOfTriangle]])
                    {
                        if (!tempAdjData[vertexIndex].Contains(triIndex))
                        {
                            tempAdjData[vertexIndex].Add(triIndex);
                        }
                    }
                }
                // foreach (int vertexIndex in posGraph[vertices[i0]])
                // {
                //     if (!tempAdjData[vertexIndex].Contains(triIndex))
                //     {
                //         tempAdjData[vertexIndex].Add(triIndex);
                //     }
                // }
                //
                //
                // if (posGraph.TryGetValue(vertices[i1], out vertListOnSamePos))
                // {
                //     foreach (int samePosVerts in vertListOnSamePos)
                //     {
                //         if (!tempAdjData[samePosVerts].Contains(triIndex))
                //         {
                //             tempAdjData[samePosVerts].Add(triIndex);
                //         }
                //     }
                // }
                // else
                // {
                //     tempAdjData[i1].Add(triIndex);
                //     Debug.Log("false");
                // }
                //
                // if (posGraph.TryGetValue(vertices[i2], out vertListOnSamePos))
                // {
                //     foreach (int samePosVerts in vertListOnSamePos)
                //     {
                //         if (!tempAdjData[samePosVerts].Contains(triIndex))
                //         {
                //             tempAdjData[samePosVerts].Add(triIndex);
                //         }
                //     }
                // }
                // else
                // {
                //     tempAdjData[i2].Add(triIndex);
                //     Debug.Log("false");
                // }
            }


            var unrolledList = new List<int>();
            adjacencyMapper = new int2[mesh.vertexCount];

            int currentStartIndex = 0;

            for (int i = 0; i < mesh.vertexCount; i++)
            {
                int size = tempAdjData[i].Count;
                unrolledList.AddRange(tempAdjData[i]);
                adjacencyMapper[i] = new int2(currentStartIndex, size);
                currentStartIndex += size;
            }

            adjacencyList = unrolledList.ToArray();
        }


        public static List<MeshDataCache.DuplicateVerticesArray> GetDuplicateVerticesMap(Mesh mesh, Dictionary<Vector3, List<int>> posGraph)
        {
            var map = new List<MeshDataCache.DuplicateVerticesArray>();

            foreach (var kvp in posGraph)
            {
                if (kvp.Value.Count > 1)
                {
                    map.Add(new MeshDataCache.DuplicateVerticesArray { Value = kvp.Value.ToArray() });
                }
            }

            Debug.Log("Number of Duplicate Vertices Cached: " + map.Count);
            return map;
        }


        public static Dictionary<Vector3, List<int>> GetVertexPosGraph(Mesh mesh)
        {
            var vertices = mesh.vertices;
            var graph = new Dictionary<Vector3, List<int>>(mesh.vertexCount);

            for (int vertexIndex = 0; vertexIndex < mesh.vertexCount; vertexIndex++)
            {
                List<int> entryList;

                if (!graph.TryGetValue(vertices[vertexIndex], out entryList))
                {
                    entryList = new List<int>();
                    graph.Add(vertices[vertexIndex], entryList);
                }

                entryList.Add(vertexIndex);
            }

            return graph;
        }
    }
}