using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Ica.Utils.Tests
{
    public class CollectionUtilsTests
    {
        [Test]
        public void UnrollListToList_SubArrayLength()
        {
            var nested = new UnsafeList<NativeList<int>>(1, Allocator.Temp);

            nested.Add(new NativeList<int>(Allocator.Temp) { 0 });
            nested.Add(new NativeList<int>(Allocator.Temp) { 1, 1 });
            nested.Add(new NativeList<int>(Allocator.Temp) { 2, 2, 2 });

            var unrolled = new UnrolledContainer.UnrolledList<int>(nested, Allocator.Temp);

            Assert.IsTrue(unrolled._data.Length == 6, "Unrolled Data size not correct");
            Assert.IsTrue(unrolled.GetSubArrayLength(0) == 1, "a");
            Assert.IsTrue(unrolled.GetSubArrayLength(1) == 2, "b");
            Assert.IsTrue(unrolled.GetSubArrayLength(2) == 3, "c");
        }
        
        [Test]
        public void UnrollListToList_MapperCount()
        {
            var nested = new UnsafeList<NativeList<int>>(1, Allocator.Temp);

            nested.Add(new NativeList<int>(Allocator.Temp) { 0 });
            nested.Add(new NativeList<int>(Allocator.Temp) { 1, 1 });
            nested.Add(new NativeList<int>(Allocator.Temp) { 2, 2, 2 });

            var unrolled = new UnrolledContainer.UnrolledList<int>(nested, Allocator.Temp);

            Assert.IsTrue(unrolled._startIndices.Length == 4, "mapper length is not correct");

        }


        [Test]
        public void UnrollArrayToArray_SubArrayLength()
        {
            var nested = new UnsafeList<NativeArray<int>>(1, Allocator.Temp);

            nested.Add(new NativeArray<int>(1, Allocator.Temp));
            nested.Add(new NativeArray<int>(2, Allocator.Temp));
            nested.Add(new NativeArray<int>(3, Allocator.Temp));

            var unrolled = new UnrolledContainer.UnrolledArray<int>(nested, Allocator.Temp);

            Assert.IsTrue(unrolled._data.Length == 6, "Unrolled Data size not correct");
            Assert.IsTrue(unrolled.GetSubArrayLength(0) == 1, "a");
            Assert.IsTrue(unrolled.GetSubArrayLength(1) == 2, "b");
            Assert.IsTrue(unrolled.GetSubArrayLength(2) == 3, "c");
        }


        [Test]
        public void Get_NestedTotalSize_List()
        {
            var nested = new UnsafeList<NativeList<int>>(1, Allocator.Temp);
            nested.Add(new NativeList<int>(Allocator.Temp) { 0, 0, 0 });
            nested.Add(new NativeList<int>(Allocator.Temp) { 1, 1 });
            nested.Add(new NativeList<int>(Allocator.Temp) { 2 });

            Ica.Utils.NativeContainerUtils.GetTotalSizeOfNestedContainer(nested, out var size);
            Assert.IsTrue(size == 6);
        }

        [Test]
        public void Get_NestedTotalSize_Array()
        {
            var nested = new UnsafeList<NativeArray<int>>(1, Allocator.Temp);
            nested.Add(new NativeArray<int>(3, Allocator.Temp));
            nested.Add(new NativeArray<int>(2, Allocator.Temp));
            nested.Add(new NativeArray<int>(1, Allocator.Temp));

            Ica.Utils.NativeContainerUtils.GetTotalSizeOfNestedContainer(nested, out var size);
            Assert.IsTrue(size == 6);
        }
    }
}