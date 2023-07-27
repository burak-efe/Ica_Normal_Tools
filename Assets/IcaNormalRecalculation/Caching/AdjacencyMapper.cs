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
    //  [BurstCompile]
    public static class AdjacencyMapper
    {
        /// <summary>
        /// Calculate adjacency data of every vertex
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
            var pTempAllocate = new ProfilerMarker("TempAllocate");
            pTempAllocate.Begin();

            var tempAdjData = new UnsafeList<NativeList<int>>(vertices.Length, Allocator.Temp);

            var pTempSubAllocate = new ProfilerMarker("pTempSubAllocate");

            for (int i = 0; i < vertices.Length; i++)
            {
                pTempSubAllocate.Begin();
                tempAdjData.Add(new NativeList<int>(3, Allocator.Temp));
                pTempSubAllocate.End();
            }

            pTempAllocate.End();


            var p1 = new ProfilerMarker("Map");
            p1.Begin();

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

            p1.End();

            var pOut = new ProfilerMarker("AllocateOut");
            pOut.Begin();

            outAdjacencyList = new NativeList<int>(unrolledListLength, allocator);
            outAdjacencyMapper = new NativeArray<int2>(vertices.Length, allocator, NativeArrayOptions.UninitializedMemory);
            pOut.End();

            var p2 = new ProfilerMarker("Unroll");
            p2.Begin();

            //var tempList = new NativeList<int>(unrolledListLength, Allocator.Temp);
            int currentStartIndex = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                int size = tempAdjData[i].Length;
                //tempList.AddRange(tempAdjData[i].AsArray());
                outAdjacencyList.AddRange(tempAdjData[i].AsArray());
                outAdjacencyMapper[i] = new int2(currentStartIndex, size);
                currentStartIndex += size;
            }

            //outAdjacencyList.CopyFrom(tempList.AsArray());
            p2.End();
        }
    }
}