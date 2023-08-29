using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

namespace Ica.Normal
{
    [BurstCompile]
    public static class VertexPositionMapper
    {
        /// <summary>
        /// Get a HashMap where keys are position and values are a list of index of vertices that locate on that position.
        /// If Value List only have one member that means that vertex have not a duplicate. 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="posVertexIndicesPair"></param>
        /// <param name="allocator"></param>
        [BurstCompile]
        public static void GetVertexPosHashMap(
            [NoAlias] in NativeArray<float3> vertices,
            [NoAlias]out UnsafeHashMap<float3, NativeList<int>> posVertexIndicesPair,
            [NoAlias]Allocator allocator)
        {
            var pAllocateOut = new ProfilerMarker("pPosMapAllocateOut");
            var pTryGetValueAndAddNewPair = new ProfilerMarker("pPosMapTryGetValueAndAddNewPair");
            var pAddNewPair = new ProfilerMarker("pPosMapAddNewPair");
            var pAddToList = new ProfilerMarker("pPosMapAddToList");

            pAllocateOut.Begin();
            posVertexIndicesPair = new UnsafeHashMap<float3, NativeList<int>>(vertices.Length, allocator);
            pAllocateOut.End();

            for (int vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex++)
            {
                pTryGetValueAndAddNewPair.Begin();

                if (!posVertexIndicesPair.TryGetValue(vertices[vertexIndex], out var vertexIndexList))
                {
                    pAddNewPair.Begin();
                    vertexIndexList = new NativeList<int>(1, allocator);
                    vertexIndexList.Add(vertexIndex);
                    
                    posVertexIndicesPair.Add(vertices[vertexIndex], vertexIndexList);
                    pAddNewPair.End();
                }
                else
                {
                    vertexIndexList.Add(vertexIndex);
                }
                pTryGetValueAndAddNewPair.End();
                
                pAddToList.Begin();

                pAddToList.End();
            }

        }
    }
}