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
        /// <summary>
        /// Get a HashMap where keys are position and values a list of indices of vertices that locate that key. If Value List only have one member that means that vertex have not a duplicate. 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="posVertexIndicesPair"></param>
        /// <param name="allocator"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [BurstCompile]
        public static void GetPosVertexIndicesDict(in NativeArray<float3> vertices, ref UnsafeHashMap<float3, NativeList<int>> posVertexIndicesPair, Allocator allocator)
        {
            posVertexIndicesPair = new UnsafeHashMap<float3, NativeList<int>>(vertices.Length, allocator);

            for (int vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex++)
            {
                if (!posVertexIndicesPair.TryGetValue(vertices[vertexIndex], out var entryList))
                {
                    entryList = new NativeList<int>(allocator);
                    posVertexIndicesPair.Add(vertices[vertexIndex], entryList);
                }
                entryList.Add(vertexIndex);
            }
        }
    }
}