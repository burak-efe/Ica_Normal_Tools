using Ica.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

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
            [NoAlias] out NativeList<int> outRealConnectedCount,
            [NoAlias] Allocator allocator
        )
        {
            var pAdjacencyMapper = new ProfilerMarker("pAdjacencyMapper");
            var pUnroll = new ProfilerMarker("pUnroll");
            var pAllocateForPerVertex = new ProfilerMarker("pAllocateForPerVertex");
            var pCalculate = new ProfilerMarker("pCalculate");

            pAdjacencyMapper.Begin();

            var tempAdjData = new UnsafeList<UnsafeList<int>>(vertices.Length, Allocator.Temp);
            
            pAllocateForPerVertex.Begin();
            for (int i = 0; i < vertices.Length; i++)
                tempAdjData.Add(new UnsafeList<int>(8, Allocator.Temp));
            pAllocateForPerVertex.End();
            

            outRealConnectedCount = new NativeList<int>(vertices.Length, allocator);
            outRealConnectedCount.Resize(vertices.Length, NativeArrayOptions.ClearMemory);

            pCalculate.Begin();
            //for every index
            for (int i = 0; i < indices.Length; i++)
            {
                int triIndex = i / 3;
                int vertexIndex = indices[i];
                float3 pos = vertices[vertexIndex];
                NativeList<int> listOfVerticesOnThatPosition = vertexPosHashMap[pos];

                // to every vertices on that position, add current triangle index
                for (int j = 0; j < listOfVerticesOnThatPosition.Length; j++)
                {
                    int vertexOnThatPos = listOfVerticesOnThatPosition.ElementAt(j);

                    //physically connected
                    if (vertexIndex == vertexOnThatPos)
                    {
                        tempAdjData.ElementAt(vertexOnThatPos).InsertAtBeginning(triIndex);
                        outRealConnectedCount[vertexIndex]++;
                    }
                    //not physically connected
                    else
                        tempAdjData.ElementAt(vertexOnThatPos).Add(triIndex);
                }
            }
            pCalculate.End();

            outAdjacencyList = new NativeList<int>(allocator);
            outStartIndicesMap = new NativeList<int>(allocator);

            pUnroll.Begin();
            NativeContainerUtils.UnrollUnsafeListsToList(tempAdjData, ref outAdjacencyList, ref outStartIndicesMap);
            pUnroll.End();
            pAdjacencyMapper.End();
        }
        
        
        
                /// <summary>
        /// Calculate adjacency data to triangle of every vertex
        /// </summary>
        [BurstCompile]
        public static void CalculateAdjacencyData2
        (
            [NoAlias] in NativeArray<float3> vertices,
            [NoAlias] in NativeArray<int> indices,
            [NoAlias] in UnsafeHashMap<float3, NativeList<int>> vertexPosHashMap,
            [NoAlias] out NativeList<int> outAdjacencyList,
            [NoAlias] out NativeList<int> outStartIndicesMap,
            [NoAlias] out NativeList<int> outRealConnectedCount,
            [NoAlias] Allocator allocator
        )
        {
            var pAdjacencyMapper = new ProfilerMarker("pAdjacencyMapper");
            var pUnroll = new ProfilerMarker("pUnroll");

            pAdjacencyMapper.Begin();

            var tempAdjData = new UnsafeList<NativeList<int>>(vertices.Length, Allocator.Temp);

            for (int i = 0; i < vertices.Length; i++)
            {
                tempAdjData.Add(new NativeList<int>(8, Allocator.Temp));
            }

            outRealConnectedCount = new NativeList<int>(vertices.Length, allocator);
            outRealConnectedCount.Resize(vertices.Length, NativeArrayOptions.ClearMemory);

            //for every index
            for (int i = 0; i < indices.Length; i++)
            {
                int triIndex = i / 3;
                int vertexIndex = indices[i];
                float3 pos = vertices[vertexIndex];
                NativeList<int> listOfVerticesOnThatPosition = vertexPosHashMap[pos];

                // for every vertices on that position, add current triangle index
                for (int j = 0; j < listOfVerticesOnThatPosition.Length; j++)
                {
                    int vertexOnThatPos = listOfVerticesOnThatPosition.ElementAt(j);

                    //physically connected
                    if (vertexIndex == vertexOnThatPos)
                    {
                        tempAdjData.ElementAt(vertexOnThatPos).InsertAtBeginning(triIndex);
                        outRealConnectedCount[vertexIndex]++;
                    }
                    //not physically connected
                    else
                        tempAdjData.ElementAt(vertexOnThatPos).Add(triIndex);
                }
            }

            outAdjacencyList = new NativeList<int>(allocator);
            outStartIndicesMap = new NativeList<int>(allocator);

            pUnroll.Begin();
            NativeContainerUtils.UnrollListsToList(tempAdjData, ref outAdjacencyList, ref outStartIndicesMap);
            pUnroll.End();
            pAdjacencyMapper.End();
        }
        
        
        
        
        
        
        
    }
    
    
}