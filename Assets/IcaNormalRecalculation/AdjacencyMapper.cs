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
        /// Calculate adjacency data of every vertex
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [BurstCompile]
        public static void CalculateAdjacencyData
        (
            in NativeArray<float3> vertices, in NativeList<int> indices, in UnsafeHashMap<float3, NativeList<int>> posGraph,
            out NativeList<int> outAdjacencyList, out NativeArray<int2> outAdjacencyMapper, Allocator allocator
        )
        {
            var pTempAllocate = new ProfilerMarker("TempAllocate");
            pTempAllocate.Begin();
            
            var tempAdjData = new UnsafeList<NativeList<int>>(vertices.Length, Allocator.Temp);
            
            pTempAllocate.End();
            
            
            var pTempSubAllocate = new ProfilerMarker("pTempSubAllocate");
            pTempSubAllocate.Begin();

             for (int i = 0; i < vertices.Length; i++)
                 tempAdjData.Add(new NativeList<int>(6, Allocator.Temp));
 
            pTempSubAllocate.End();


            var p1 = new ProfilerMarker("Map");
            p1.Begin();

            var unrolledListLength = 0;
            //for every triangle
            for (int indicesIndex = 0; indicesIndex < indices.Length; indicesIndex += 3)
            {
                var triIndex = indicesIndex / 3;
                //for three connected vertex of triangle
                for (int j = 0; j < 3; j++)
                {
                    var subVertexOfTriangle = indices[indicesIndex + j];

                    foreach (int vertexIndex in posGraph[vertices[subVertexOfTriangle]])
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
            outAdjacencyMapper = new NativeArray<int2>(vertices.Length, allocator);
            pOut.End();


            var p2 = new ProfilerMarker("Unroll");
            p2.Begin();

            int currentStartIndex = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                int size = tempAdjData[i].Length;
                outAdjacencyList.AddRange(tempAdjData[i].AsArray());
                outAdjacencyMapper[i] = new int2(currentStartIndex, size);
                currentStartIndex += size;
            }

            p2.End();

            // var pDispose = new ProfilerMarker("pDispose");
            // pDispose.Begin();
            // foreach (var data in tempAdjData)
            // {
            //     data.Dispose();
            // }
            //
            // tempAdjData.Dispose();
            // pDispose.End();
        }


        /// <summary>
        /// Calculate adjacency data of every vertex
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [BurstCompile]
        public static void CalculateAdjacencyDataMulti
        (
            ref NativeArray<float3> vertices, ref NativeList<int> indices, ref UnsafeHashMap<float3, NativeList<int>> posGraph,
            ref NativeList<int> outAdjacencyList, ref NativeArray<int2> outAdjacencyMapper, Allocator allocator
        )
        {
            var tempAdjData = new UnsafeList<NativeList<int>>(vertices.Length, Allocator.Temp);

            for (int i = 0; i < vertices.Length; i++)
            {
                tempAdjData.Add(new NativeList<int>(6, Allocator.Temp));
            }

            var unrolledListLength = 0;
            //for every triangle
            for (int indicesIndex = 0; indicesIndex < indices.Length; indicesIndex += 3)
            {
                var triIndex = indicesIndex / 3;
                //for three connected vertex of triangle
                for (int j = 0; j < 3; j++)
                {
                    var subVertexOfTriangle = indices[indicesIndex + j];

                    foreach (int vertexIndex in posGraph[vertices[subVertexOfTriangle]])
                    {
                        //if (!tempAdjData[vertexIndex].Contains(triIndex))
                        tempAdjData[vertexIndex].Add(triIndex);
                        unrolledListLength++;
                    }
                }
            }

            outAdjacencyList = new NativeList<int>(unrolledListLength, Allocator.Temp);
            outAdjacencyMapper = new NativeArray<int2>(vertices.Length, allocator);

            int currentStartIndex = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                int size = tempAdjData[i].Length;
                outAdjacencyList.AddRange(tempAdjData[i].AsArray());
                outAdjacencyMapper[i] = new int2(currentStartIndex, size);
                currentStartIndex += size;
            }
        }

        [BurstCompile]
        private static void AllocateNested(ref UnsafeList<NativeList<int>> target)
        {
            for (int i = 0; i < target.Capacity; i++)
            {
                target.Add(new NativeList<int>());
            }

            
            var job = new AllocateNestedJob
            {
                Target = target
            };

            var handle = job.ScheduleParallel(target.Length, target.Length / 32, default);
            handle.Complete();
        }

        [BurstCompile]
        private struct AllocateNestedJob : IJobFor
        {
            public UnsafeList<NativeList<int>> Target;

            public void Execute(int index)
            {
                Target[index] = new NativeList<int>(6, Allocator.Temp);
            }
        }

        private struct AdjacencyDataJob : IJobFor
        {
            public void Execute(int triangleIndex)
            {
            }
        }
    }
}