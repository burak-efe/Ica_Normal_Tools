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
            [NoAlias] out NativeList<int> outStartIndices,
            [NoAlias] Allocator allocator
        )
        {
            var pAdjTempContainerAllocate = new ProfilerMarker("pAdjTempContainerAllocate");
            var pTempSubAllocate = new ProfilerMarker("pAdjTempSubContainerAllocate");
            var pCalculateAdjacencyData = new ProfilerMarker("pAdjCalculateAdjacencyData");
            var pAllocateOutContainers = new ProfilerMarker("pAdjAllocateOutContainers");
            var pUnroll = new ProfilerMarker("pAdjUnroll");

            pAdjTempContainerAllocate.Begin();

            var tempAdjData = new UnsafeList<NativeList<int>>(vertices.Length, Allocator.Temp);
            pAdjTempContainerAllocate.End();

            pTempSubAllocate.Begin();
            for (int i = 0; i < vertices.Length; i++)
            {
                tempAdjData.Add(new NativeList<int>(4, Allocator.Temp));
            }

            pTempSubAllocate.End();

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
                    var pos = vertices[subVertexOfTriangle];
                    var listOfVerticesOnThatPosition = vertexPosHashMap[pos];
                    for (int i = 0; i < listOfVerticesOnThatPosition.Length; i++)
                    {
                        tempAdjData.ElementAt(listOfVerticesOnThatPosition.ElementAt(i)).Add(triIndex);
                        unrolledListLength++;
                    }
                }
            }

            pCalculateAdjacencyData.End();

            pAllocateOutContainers.Begin();
            outAdjacencyList = new NativeList<int>(unrolledListLength, allocator);
            outStartIndices = new NativeList<int>(vertices.Length+1, allocator);
            pAllocateOutContainers.End();

            pUnroll.Begin();
            
            NativeContainerUtils.UnrollListsToList(tempAdjData,ref outAdjacencyList,ref outStartIndices);
            // unsafe
            // {
            //     int currentStartIndex = 0;
            //     for (int i = 0; i < vertices.Length; i++)
            //     {
            //         int size = tempAdjData.ElementAt(i).Length;
            //         outAdjacencyList.AddRangeNoResize(tempAdjData[i].Ptr, tempAdjData[i].Length);
            //         outAdjacencyMapper.Add(new int2(currentStartIndex, size));
            //         currentStartIndex += size;
            //     }
            // }

            pUnroll.End();
        }
    }
}