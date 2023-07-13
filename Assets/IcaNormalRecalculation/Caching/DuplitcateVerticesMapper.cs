using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace IcaNormal
{
    [BurstCompile]
    public static class DuplicateVerticesMapper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [BurstCompile]
        
        public static void GetDuplicateVerticesMap
        (in UnsafeHashMap<float3, NativeList<int>> vertexPosHashMap, 
            out UnsafeList<NativeArray<int>> outMap,  Allocator allocator)
        {
            outMap = new UnsafeList<NativeArray<int>>(16, allocator);
            
            foreach (var kvp in vertexPosHashMap)
            {
                if (kvp.Value.Length > 1)
                {
                    outMap.Add(new NativeArray<int>(kvp.Value.AsArray(), allocator));
                }
            }

        }
    }
}