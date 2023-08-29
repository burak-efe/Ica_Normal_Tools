using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;


namespace Ica.Utils
{
    public static class UnrolledContainer
    {
        public struct UnrolledList<T> : IDisposable where T : unmanaged
        {
            public NativeList<T> _data;
            public int SubContainerCount { get; }
            public NativeList<int> _startIndices;


            public UnrolledList(in UnsafeList<NativeList<T>> nestedData, Allocator allocator)
            {
                Assert.IsTrue(nestedData.Length > 0, "nested list count should be more than zero");

                SubContainerCount = nestedData.Length;
                NativeContainerUtils.GetTotalSizeOfNestedContainer(nestedData, out var totalSize);
                _data = new NativeList<T>(totalSize, allocator);
                _startIndices = new NativeList<int>(SubContainerCount + 1 , allocator);
                NativeContainerUtils.UnrollListsToList(nestedData, ref _data, ref _startIndices);
            }

            public NativeArray<T> GetSubArray(int index)
            {
                return _data.AsArray().GetSubArray(_startIndices[index], GetSubArrayLength(index));
            }

            public int GetSubArrayLength(int subArrayIndex)
            {
                return _startIndices[subArrayIndex + 1] - _startIndices[subArrayIndex];
            }

            public void Dispose()
            {
                _data.Dispose();
                _startIndices.Dispose();
            }
        }

        public struct UnrolledArray<T> : IDisposable where T : unmanaged
        {
            public NativeArray<T> _data;
            public int SubContainerCount { get; }
            public NativeList<int> _startIndices;


            public UnrolledArray(in UnsafeList<NativeArray<T>> nestedData, Allocator allocator)
            {
                Assert.IsTrue(nestedData.Length > 0, "nested list count should be more than zero");

                SubContainerCount = nestedData.Length;
                NativeContainerUtils.GetTotalSizeOfNestedContainer(nestedData, out var totalSize);
                _data = new NativeArray<T>(totalSize, allocator, NativeArrayOptions.UninitializedMemory);
                _startIndices = new NativeList<int>(SubContainerCount, allocator);
                NativeContainerUtils.UnrollArraysToArray(nestedData, ref _data, ref _startIndices);
            }

            public NativeArray<T> GetSubArray(int index)
            {
                return _data.GetSubArray(_startIndices[index], GetSubArrayLength(index));
            }

            public int GetSubArrayLength(int subArrayIndex)
            {
                return _startIndices[subArrayIndex + 1] - _startIndices[subArrayIndex];
            }

            public void Dispose()
            {
                _data.Dispose();
                _startIndices.Dispose();
            }
        }
    }
}