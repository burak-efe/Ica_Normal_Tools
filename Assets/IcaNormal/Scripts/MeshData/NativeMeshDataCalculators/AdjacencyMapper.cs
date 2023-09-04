using Ica.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Profiling;

namespace Ica.Normal
{
    [BurstCompile]
    public static class AdjacencyMapper
    {
        /// <summary>
        /// Calculate adjacency data to triangle of every vertex
        /// </summary>
        [BurstCompile]
        public static void CalculateAdjacencyData
        (
            [NoAlias] in NativeArray<float3> vertices,
            [NoAlias] in NativeArray<int> indices,
            [NoAlias] in UnsafeHashMap<float3, NativeList<int>> vertexPosHashMap,
            [NoAlias] out NativeList<int> outAdjacencyList,
            [NoAlias] out NativeList<int> outStartIndicesMap,
            //[NoAlias] out NativeList<int> outRealConnectedCount,
            [NoAlias] Allocator allocator
        )
        {
            var tempAdjData = new UnsafeList<NativeList<int>>(vertices.Length, Allocator.Temp);

            for (int i = 0; i < vertices.Length; i++)
            {
                tempAdjData.Add(new NativeList<int>(8, Allocator.Temp));
            }

            //outRealConnectedCount = new NativeList<int>(vertices.Length, allocator);
            //outRealConnectedCount.Resize(vertices.Length,NativeArrayOptions.ClearMemory);

            var unrolledListLength = 0;
            //for every triangle
            for (int indicesIndex = 0; indicesIndex < indices.Length; indicesIndex += 3)
            {
                int triIndex = indicesIndex / 3;
                //for three connected vertex of triangle
                for (int v = 0; v < 3; v++)
                {
                    int indexofSubVertexOfTriangle = indices[indicesIndex + v];
                    float3 pos = vertices[indexofSubVertexOfTriangle];
                    NativeList<int> listOfVerticesOnThatPosition = vertexPosHashMap[pos];

                    // for every vertices on that position, add current triangle index
                    for (int i = 0; i < listOfVerticesOnThatPosition.Length; i++)
                    {
                        var vertexIndex = listOfVerticesOnThatPosition.ElementAt(i);
                        tempAdjData.ElementAt(vertexIndex).Add(triIndex);
                        unrolledListLength++;
                    }
                }
            }

            outAdjacencyList = new NativeList<int>(unrolledListLength, allocator);
            outStartIndicesMap = new NativeList<int>(vertices.Length + 1, allocator);

            NativeContainerUtils.UnrollListsToList(tempAdjData, ref outAdjacencyList, ref outStartIndicesMap);
        }
    }
}