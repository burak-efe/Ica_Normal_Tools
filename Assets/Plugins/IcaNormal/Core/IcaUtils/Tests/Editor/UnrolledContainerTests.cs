using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;


namespace Ica.Utils.Tests
{
    public class UnrolledContainerTests
    {
        [Test]
        public void UnrollListToList_SubArrayLength()
        {
            var nested = new UnsafeList<NativeList<int>>(1, Allocator.Temp);

            nested.Add(new NativeList<int>(Allocator.Temp) { 0 });
            nested.Add(new NativeList<int>(Allocator.Temp) { 1, 1 });
            nested.Add(new NativeList<int>(Allocator.Temp) { 2, 2, 2 });

            var unrolled = new UnrolledList<int>(nested, Allocator.Temp);

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

            var unrolled = new UnrolledList<int>(nested, Allocator.Temp);

            Assert.IsTrue(unrolled._startIndices.Length == 4, "mapper length is not correct");
        }


        [Test]
        public void UnrolledContainerTestsSimplePasses()
        {
            var nested = new UnsafeList<NativeList<int>>(1, Allocator.Temp);

            nested.Add(new NativeList<int>(Allocator.Temp) { 3, 3, 3 });
            nested.Add(new NativeList<int>(Allocator.Temp) { 2, 2 });
            nested.Add(new NativeList<int>(Allocator.Temp) { 4, 4, 4, 4 });

            var unrolled = new UnrolledList<int>(nested, Allocator.Temp);

            unrolled.Add(1, 0, 34);
            UnityEngine.Assertions.Assert.AreEqual(unrolled._data.Length, 10);

            Assert.AreEqual(unrolled.GetSubArray(1)[0], 34);
        }
    }
}