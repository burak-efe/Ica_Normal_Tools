using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;


namespace Ica.Utils
{
    public struct UnrolledList<T> : IDisposable where T : unmanaged
    {
        public NativeList<T> Data;
        public int SubContainerCount => StartIndices.Length - 1;
        public NativeList<int> StartIndices;

        public void Add(int subArrayIndex, T item)
        {
            Data.Insert(StartIndices[subArrayIndex] + GetSubArrayLength(subArrayIndex), item);
            for (int i = subArrayIndex + 1; i < StartIndices.Length; i++)
            {
                StartIndices[i]++;
            }
        }

        public UnrolledList(in UnsafeList<NativeList<T>> nestedData, Allocator allocator)
        {
            Assert.IsTrue(nestedData.Length > 0, "nested list count should be more than zero");
            
            NativeContainerUtils.GetTotalSizeOfNestedContainer(nestedData, out var totalSize);
            Data = new NativeList<T>(totalSize, allocator);
            StartIndices = new NativeList<int>(nestedData.Length + 1, allocator);
            NativeContainerUtils.UnrollListsToList(nestedData, ref Data, ref StartIndices);
        }

        public NativeArray<T> GetSubArray(int index)
        {
            return Data.AsArray().GetSubArray(StartIndices[index], GetSubArrayLength(index));
        }

        public int GetSubArrayLength(int subArrayIndex)
        {
            return StartIndices[subArrayIndex + 1] - StartIndices[subArrayIndex];
        }

        public void Dispose()
        {
            Data.Dispose();
            StartIndices.Dispose();
        }
    }
}