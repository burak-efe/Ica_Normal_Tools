using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;


namespace IcaNormal
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
            in NativeArray<float3> vertices,
            in NativeList<int> indices,
            in UnsafeHashMap<float3, NativeList<int>> vertexPosHashMap,
            out NativeList<int> outAdjacencyList,
            out NativeArray<int2> outAdjacencyMapper,
            Allocator allocator
        )
        {
            var pMeshAdjacency = new ProfilerMarker("pMeshAdjacency");
            var pTempAllocate = new ProfilerMarker("pTempContainerAllocate");
            var pTempSubAllocate = new ProfilerMarker("pTempSubContainerAllocate");
            var pCalculateAdjacencyData = new ProfilerMarker("pCalculateAdjacencyData");
            var pAllocateOutContainers = new ProfilerMarker("pAllocateOutContainers");
            var pUnroll = new ProfilerMarker("pUnroll");
            
            pMeshAdjacency.Begin();

            pTempAllocate.Begin();

            var tempAdjData = new UnsafeList<NativeList<int>>(vertices.Length, Allocator.Temp);


            pTempSubAllocate.Begin();
            for (int i = 0; i < vertices.Length; i++)
            {
                tempAdjData.Add(new NativeList<int>(3, Allocator.Temp));
            }

            pTempSubAllocate.End();

            pTempAllocate.End();


            pCalculateAdjacencyData.Begin();

            var unrolledListLength = 0;
            //for every triangle
            for (int indicesIndex = 0; indicesIndex < indices.Length; indicesIndex += 3)
            {
                var triIndex = indicesIndex / 3;
                //for three connected vertex of triangle
                for (int v = 0; v < 3; v++)
                {
                    var subVertexOfTriangle = indices[indicesIndex + v];

                    foreach (int vertexIndex in vertexPosHashMap[vertices[subVertexOfTriangle]])
                    {
                        //if (!tempAdjData[vertexIndex].Contains(triIndex))
                        tempAdjData[vertexIndex].Add(triIndex);
                        unrolledListLength++;
                    }
                }
            }

            pCalculateAdjacencyData.End();

            pAllocateOutContainers.Begin();

            outAdjacencyList = new NativeList<int>(unrolledListLength, allocator);
            outAdjacencyMapper = new NativeArray<int2>(vertices.Length, allocator, NativeArrayOptions.UninitializedMemory);
            pAllocateOutContainers.End();



            pUnroll.Begin();

            int currentStartIndex = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                int size = tempAdjData[i].Length;
                outAdjacencyList.AddRange(tempAdjData[i].AsArray());
                outAdjacencyMapper[i] = new int2(currentStartIndex, size);
                currentStartIndex += size;
            }

            pUnroll.End();

            pMeshAdjacency.End();
        }
    }
}