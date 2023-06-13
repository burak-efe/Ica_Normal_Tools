using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;


namespace IcaNormal
{
    [BurstCompile]
    public static class AdjacencyMapper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [BurstCompile]
        public static void CalculateAdjacencyData
        (
            ref NativeArray<float3> vertices, ref NativeList<int> indices, ref UnsafeHashMap<float3, NativeList<int>> posGraph,
            ref NativeList<int> outAdjacencyList, ref NativeArray<int2> outAdjacencyMapper, Allocator allocator
        )
        {
            var tempAdjData = new UnsafeList<NativeList<int>>(vertices.Length, Allocator.Temp);

            for (int i = 0; i < vertices.Length; i++)
            {
                tempAdjData.Add(new NativeList<int>(6,Allocator.Temp));
            }

            var unrolledListLength = 0;
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
                            unrolledListLength++;
                        }
                    }
                }
            }

            outAdjacencyList = new NativeList<int>(unrolledListLength,Allocator.Temp);
            outAdjacencyMapper = new NativeArray<int2>(vertices.Length, allocator);

            int currentStartIndex = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                int size = tempAdjData[i].Length;
                outAdjacencyList.AddRange(tempAdjData[i].AsArray());
                outAdjacencyMapper[i] = new int2(currentStartIndex, size);
                currentStartIndex += size;
            }

            //outAdjacencyList = new NativeArray<int>(unrolledList.AsArray(), allocator);
        }
    }
}