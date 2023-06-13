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
        public static void GetDuplicateVerticesMap(ref UnsafeHashMap<float3, NativeList<int>> posGraph, ref UnsafeList<NativeArray<int>> outMap,  Allocator allocator)
        {
            outMap = new UnsafeList<NativeArray<int>>(10, allocator);

            foreach (var kvp in posGraph)
            {
                if (kvp.Value.Length > 1)
                {
                    outMap.Add(new NativeArray<int>(kvp.Value.AsArray(), allocator));
                }
            }

            //Debug.Log("Number of Duplicate Vertices Cached: " + outMap.Length);
        }
    }
}