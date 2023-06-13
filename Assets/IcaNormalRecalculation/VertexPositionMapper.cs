using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace IcaNormal
{
    [BurstCompile]
    public static class VertexPositionMapper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [BurstCompile]
        public static void GetVertexPosGraph(ref NativeArray<float3> vertices, ref UnsafeHashMap<float3, NativeList<int>> graph, Allocator allocator)
        {
             graph = new UnsafeHashMap<float3, NativeList<int>>(vertices.Length, allocator);

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

            //outGraph
            //return graph;
        }
    }
}