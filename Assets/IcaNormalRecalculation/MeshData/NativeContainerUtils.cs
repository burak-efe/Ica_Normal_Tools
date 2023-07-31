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
    public unsafe static class NativeContainerUtils
    {
        [BurstCompile]
        public static void UnrollListOfListToArray<T>(UnsafeList<NativeList<T>> nestedData, ref NativeArray<T> outUnrolledData, ref NativeArray<int> outMapper) where T : unmanaged
        {
            var templist = new NativeList<T>(Allocator.Temp);
            UnrollListOfListToList(nestedData,  ref templist,ref outMapper);
            outUnrolledData.CopyFrom(templist.AsArray());
        }

        [BurstCompile]
        public static void UnrollListOfListToList<T>(UnsafeList<NativeList<T>> nestedData, ref NativeList<T> outUnrolledData, ref NativeArray<int> outMapper) where T : unmanaged
        {
            var mapperIndex = 0;
            for (int i = 0; i < nestedData.Length; i++)
            {
                outUnrolledData.AddRange(nestedData[i].AsArray());
                outMapper[i] = mapperIndex;
                mapperIndex += nestedData[i].Length;
            }
            outMapper[^1] = mapperIndex;
        }
        
        [BurstCompile]
        public static void UnrollListOfArrayToArray<T>(UnsafeList<NativeArray<T>> nestedData, ref NativeArray<int> outMapper, ref NativeArray<T> outUnrolledData) where T : unmanaged
        {
            GetUnrolledSizeOfNestedContainer(nestedData,out var size);
            var templist = new NativeList<T>(size,Allocator.Temp);
            var mapperIndex = 0;
            for (int i = 0; i < nestedData.Length; i++)
            {
                templist.AddRange(nestedData[i]);
                outMapper[i] = mapperIndex;
                mapperIndex += nestedData[i].Length;
            }
            outMapper[^1] = mapperIndex;
            outUnrolledData.CopyFrom(templist.AsArray());
        }


        [BurstCompile]
        public static void GetUnrolledSizeOfNestedContainer<T>(UnsafeList<NativeList<T>> nestedContainer, out int size) where T : unmanaged
        {
            size = 0;
            for (int i = 0; i < nestedContainer.Length; i++)
            {
                size += nestedContainer[i].Length;
            }
        }
        
        [BurstCompile]
        public static void GetUnrolledSizeOfNestedContainer<T>(UnsafeList<NativeArray<T>> nestedContainer, out int size) where T : unmanaged
        {
            size = 0;
            for (int i = 0; i < nestedContainer.Length; i++)
            {
                size += nestedContainer[i].Length;
            }
        }
        
        [BurstCompile]
        public static void AddRangeUnsafeList<T>(this ref NativeList<T> list, in UnsafeList<T> unsafeList) where T : unmanaged
        {
            list.AddRange(unsafeList.Ptr,unsafeList.Length);
   
        }

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