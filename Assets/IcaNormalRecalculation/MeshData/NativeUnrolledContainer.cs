using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace IcaNormal
{
    public class NativeUnrolledContainer
    {
    }

    public struct UnrolledArray<T> where T : unmanaged
    {
        public NativeArray<T> Data;
        public int SubContainerCount;
        private NativeArray<int> _mapper;

        public UnrolledArray(UnsafeList<NativeList<T>> nestedData, Allocator allocator)
        {
            SubContainerCount = nestedData.Length;
            NativeContainerUtils.GetUnrolledSizeOfNestedContainer(nestedData, out var totalCount);
            Data = new NativeArray<T>(totalCount, allocator, NativeArrayOptions.UninitializedMemory);
            _mapper = new NativeArray<int>(SubContainerCount + 1, allocator);
            NativeContainerUtils.UnrollListOfListToArray(nestedData, ref Data, ref _mapper);
        }

        public NativeArray<T> GetSubArray(int index)
        {
            return Data.GetSubArray(_mapper[index], _mapper[index + 1] - _mapper[index]);
        }
    }
}