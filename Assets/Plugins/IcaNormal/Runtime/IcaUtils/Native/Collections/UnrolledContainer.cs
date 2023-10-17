using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;


namespace Ica.Utils
{
    /// <summary>
    /// Unrolled 2D native list. Add operations involve a MemMove, similar to List Insert. So adding to last lists cheaper than adding to first lists.
    /// </summary>
    /// <typeparam name="T">Type of data that list holds</typeparam>
    public struct UnrolledList<T> : IDisposable where T : unmanaged
    {
        public NativeList<T> _data;
        public int SubContainerCount => StartIndices.Length - 1;
        
        /// <summary>
        /// Start indices of sub array on _data. Last element is total count of data;
        /// </summary>
        public NativeList<int> StartIndices;

        public void Add(int subArrayIndex, T item)
        {
            _data.Insert(StartIndices[subArrayIndex] + GetSubArrayLength(subArrayIndex), item);
            for (int i = subArrayIndex + 1; i < StartIndices.Length; i++)
            {
                StartIndices[i]++;
            }
        }

        public UnrolledList(int subArrayCount, Allocator allocator)
        {
            _data = new NativeList<T>(0, allocator);
            StartIndices = new NativeList<int>(subArrayCount + 1, allocator);
            StartIndices.Resize(subArrayCount + 1,NativeArrayOptions.ClearMemory);
        }

        public UnrolledList(in UnsafeList<NativeList<T>> nestedData, Allocator allocator)
        {
            Assert.IsTrue(nestedData.Length > 0, "nested list count should be more than zero");
            NativeContainerUtils.GetTotalSizeOfNestedContainer(nestedData, out var totalSize);
            _data = new NativeList<T>(totalSize, allocator);
            StartIndices = new NativeList<int>(nestedData.Length + 1, allocator);
            NativeContainerUtils.UnrollListsToList(nestedData, ref _data, ref StartIndices);
        }

        public NativeArray<T> GetSubArray(int index)
        {
            return _data.AsArray().GetSubArray(StartIndices[index], GetSubArrayLength(index));
        }

        public int GetSubArrayLength(int subArrayIndex)
        {
            return StartIndices[subArrayIndex + 1] - StartIndices[subArrayIndex];
        }

        public void Dispose()
        {
            _data.Dispose();
            StartIndices.Dispose();
        }
    }
}