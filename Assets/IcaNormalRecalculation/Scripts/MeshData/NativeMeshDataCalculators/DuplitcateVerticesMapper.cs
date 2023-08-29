using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Ica.Normal
{
    [BurstCompile]
    public static class DuplicateVerticesMapper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [BurstCompile]
        public static void GetDuplicateVerticesMap
        (in UnsafeHashMap<float3, NativeList<int>> vertexPosHashMap,
            out UnsafeList<NativeArray<int>> outMap, Allocator allocator)
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


        /// <summary>
        /// Convert native duplicate vertex map to managed one,which can be serialize.
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public static List<DuplicateVerticesList> GetManagedDuplicateVerticesMap(UnsafeList<NativeArray<int>> from)
        {
            var list = new List<DuplicateVerticesList>(from.Length);
            foreach (var fromArray in from)
            {
                var managed = new DuplicateVerticesList { Value = fromArray.ToArray() };
                list.Add(managed);
            }

            return list;
        }
    }
}