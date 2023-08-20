using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace IcaNormal
{
    /// <summary>
    /// Some of methods to create one dimensional container from multi-Dimensional container.
    /// </summary>
    [BurstCompile]
    public static class NativeContainerUtils
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
            var tempList = new NativeList<T>(size,Allocator.Temp);
            var mapperIndex = 0;
            for (int i = 0; i < nestedData.Length; i++)
            {
                tempList.AddRange(nestedData[i]);
                outMapper[i] = mapperIndex;
                mapperIndex += nestedData[i].Length;
            }
            outMapper[^1] = mapperIndex;
            outUnrolledData.CopyFrom(tempList.AsArray());
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
        
        // do we really need unsafe?
        // [BurstCompile]
        // public static void AddRangeUnsafeList<T>([NoAlias]this ref NativeList<T> list,[NoAlias] in UnsafeList<T> unsafeList) where T : unmanaged
        // {
        //     list.AddRange(unsafeList.Ptr,unsafeList.Length);
        //
        // }
    }
}