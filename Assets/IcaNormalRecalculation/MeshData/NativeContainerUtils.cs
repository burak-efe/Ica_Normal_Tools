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
    public static unsafe class NativeContainerUtils
    {
        [BurstCompile]
        public static void UnrollListOfListToArray<T>([NoAlias]UnsafeList<NativeList<T>> nestedData,[NoAlias] ref NativeArray<T> outUnrolledData,[NoAlias] ref NativeArray<int> outMapper) where T : unmanaged
        {
            var tempList = new NativeList<T>(Allocator.Temp);
            UnrollListOfListToList(nestedData,  ref tempList,ref outMapper);
            outUnrolledData.CopyFrom(tempList.AsArray());
        }

        [BurstCompile]
        public static void UnrollListOfListToList<T>([NoAlias]UnsafeList<NativeList<T>> nestedData,[NoAlias] ref NativeList<T> outUnrolledData,[NoAlias] ref NativeArray<int> outMapper) where T : unmanaged
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
        public static void UnrollListOfArrayToArray<T>([NoAlias]UnsafeList<NativeArray<T>> nestedData,[NoAlias] ref NativeArray<int> outMapper,[NoAlias] ref NativeArray<T> outUnrolledData) where T : unmanaged
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
        public static void GetUnrolledSizeOfNestedContainer<T>([NoAlias]UnsafeList<NativeList<T>> nestedContainer, [NoAlias]out int size) where T : unmanaged
        {
            size = 0;
            for (int i = 0; i < nestedContainer.Length; i++)
            {
                size += nestedContainer[i].Length;
            }
        }
        
        [BurstCompile]
        public static void GetUnrolledSizeOfNestedContainer<T>([NoAlias]UnsafeList<NativeArray<T>> nestedContainer,[NoAlias]out int size) where T : unmanaged
        {
            size = 0;
            for (int i = 0; i < nestedContainer.Length; i++)
            {
                size += nestedContainer[i].Length;
            }
        }
        
        [BurstCompile]
        public static void AddRangeUnsafeList<T>([NoAlias]this ref NativeList<T> list,[NoAlias] in UnsafeList<T> unsafeList) where T : unmanaged
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