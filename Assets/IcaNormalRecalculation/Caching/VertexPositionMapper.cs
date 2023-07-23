using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;

namespace IcaNormal
{
    [BurstCompile]
    public static class VertexPositionMapper
    {
        /// <summary>
        /// Get a HashMap where keys are position and values a list of indices of vertices that locate on that position. If Value List only have one member that means that vertex have not a duplicate. 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="posVertexIndicesPair"></param>
        /// <param name="allocator"></param>
        [BurstCompile]
        public static void GetVertexPosHashMap(in NativeArray<float3> vertices, out UnsafeHashMap<float3, NativeList<int>> posVertexIndicesPair, Allocator allocator)
        {
            var pAllocateOut = new ProfilerMarker("pAllocateOut");
            pAllocateOut.Begin();
            
            posVertexIndicesPair = new UnsafeHashMap<float3, NativeList<int>>(vertices.Length, allocator);
            
            pAllocateOut.End();

            for (int vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex++)
            {
                var pTryGetValue = new ProfilerMarker("pTryGetValue");
                pTryGetValue.Begin();
                
                if (!posVertexIndicesPair.TryGetValue(vertices[vertexIndex], out var vertexIndexList))
                {
                    
                    var pAddNewPair = new ProfilerMarker("pAddNewPair");
                    pAddNewPair.Begin();
                    vertexIndexList = new NativeList<int>(allocator);
                    posVertexIndicesPair.Add(vertices[vertexIndex], vertexIndexList);
                    pAddNewPair.End();
                }
                
                pTryGetValue.End();
                
                var pAddToList = new ProfilerMarker("pAddToList");
                pAddToList.Begin();
                
                vertexIndexList.Add(vertexIndex);
                
                pAddToList.End();
            }
        }


    }
}