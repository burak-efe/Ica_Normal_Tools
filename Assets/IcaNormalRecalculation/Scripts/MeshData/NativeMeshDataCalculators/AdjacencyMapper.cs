using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Profiling;

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
           [NoAlias] in NativeArray<float3> vertices,
           [NoAlias] in NativeList<int> indices,
           [NoAlias] in UnsafeHashMap<float3, NativeList<int>> vertexPosHashMap,
           [NoAlias] out NativeList<int> outAdjacencyList,
           [NoAlias] out NativeArray<int2> outAdjacencyMapper,
           [NoAlias] Allocator allocator
        )
        {
            var pAdjTempContainerAllocate = new ProfilerMarker("pAdjTempContainerAllocate");
            var pTempSubAllocate = new ProfilerMarker("pAdjTempSubContainerAllocate");
            var pCalculateAdjacencyData = new ProfilerMarker("pAdjCalculateAdjacencyData");
            var pAllocateOutContainers = new ProfilerMarker("pAdjAllocateOutContainers");
            var pUnroll = new ProfilerMarker("pAdjUnroll");

            pAdjTempContainerAllocate.Begin();

            var tempAdjData = new UnsafeList<UnsafeList<int>>(vertices.Length, Allocator.Temp);
            pAdjTempContainerAllocate.End();

            pTempSubAllocate.Begin();
            for (int i = 0; i < vertices.Length; i++)
            {
                tempAdjData.Add(new UnsafeList<int>(4, Allocator.Temp));
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

                    var listOfVerticesOnThatPosition = vertexPosHashMap[vertices[subVertexOfTriangle]];
                    for (int i = 0; i < listOfVerticesOnThatPosition.Length; i++)
                    {
                        tempAdjData.ElementAt(listOfVerticesOnThatPosition.ElementAt(i)).Add(triIndex);
                        unrolledListLength++;
                    }
                }
            }
            pCalculateAdjacencyData.End();

            pAllocateOutContainers.Begin();
            outAdjacencyList = new NativeList<int>(unrolledListLength , allocator);
            outAdjacencyMapper = new NativeArray<int2>(vertices.Length, allocator, NativeArrayOptions.UninitializedMemory);
            pAllocateOutContainers.End();

            pUnroll.Begin();
            unsafe
            {
                int currentStartIndex = 0;
                for (int i = 0; i < vertices.Length; i++)
                {
                    int size = tempAdjData.ElementAt(i).Length;
                    outAdjacencyList.AddRangeNoResize(tempAdjData[i].Ptr, tempAdjData[i].Length);
                    outAdjacencyMapper[i] = new int2(currentStartIndex, size);
                    currentStartIndex += size;
                }
                
            }
            pUnroll.End();
        }

    }
}