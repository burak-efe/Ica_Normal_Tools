using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;


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

            Assert.IsTrue(unrolled.Data.Length == 6, "Unrolled Data size not correct");
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

            Assert.IsTrue(unrolled.StartIndices.Length == 4, "mapper length is not correct");
        }


        [Test]
        public void UnrolledContainer_Add()
        {
            var nested = new UnsafeList<NativeList<int>>(1, Allocator.Temp);

            nested.Add(new NativeList<int>(Allocator.Temp) { 3, 3, 3 });
            nested.Add(new NativeList<int>(Allocator.Temp) { 2, 2 });
            nested.Add(new NativeList<int>(Allocator.Temp) { 4, 4, 4, 4 });

            var unrolled = new UnrolledList<int>(nested, Allocator.Temp);
            unrolled.Add(1, 34);
            Assert.AreEqual(unrolled.Data.Length, 10);
            Assert.AreEqual(unrolled.GetSubArray(1).ToArray()[^1], 34);
        }

        [Test]
        public void UnrolledContainer_AddToEmptyContainer_()
        {
            var nested = new UnsafeList<NativeList<int>>(1, Allocator.Temp);

            nested.Add(new NativeList<int>(Allocator.Temp) { });
            nested.Add(new NativeList<int>(Allocator.Temp) { });
            nested.Add(new NativeList<int>(Allocator.Temp) { });

            var unrolled = new UnrolledList<int>(nested, Allocator.Temp);
            unrolled.Add(1, 34);
            Assert.AreEqual(unrolled.Data.Length, 1);
            Assert.AreEqual(unrolled.GetSubArray(1)[0], 34);
        }

        [Test]
        public void UnrolledContainer_Add_()
        {
            var nested = new UnsafeList<NativeList<int>>(1, Allocator.Temp);
            nested.Add(new NativeList<int>(Allocator.Temp) { 7, 8, 9 });
            nested.Add(new NativeList<int>(Allocator.Temp) { 4, 5, 6 });
            nested.Add(new NativeList<int>(Allocator.Temp) { });
            var unrolled = new UnrolledList<int>(nested, Allocator.Temp);

            unrolled.Add(0, 10);
            unrolled.Add(0, 11);
            unrolled.Add(0, 12);

            unrolled.Add(2, 1);
            unrolled.Add(2, 2);
            unrolled.Add(2, 3);

            unrolled.Add(1, 7);
            unrolled.Add(1, 8);
            unrolled.Add(1, 9);


            Assert.AreEqual(unrolled.Data.Length, 15);
            Assert.AreEqual(unrolled.GetSubArray(0).ToArray(), new int[] { 7, 8, 9, 10, 11, 12 });
            Assert.AreEqual(unrolled.GetSubArray(1).ToArray(), new int[] { 4, 5, 6, 7, 8, 9 });
            Assert.AreEqual(unrolled.GetSubArray(2).ToArray(), new int[] { 1, 2, 3 });
        }

        [Test]
        public void UnrolledContainer_EmptyConstructor_()
        {
            var unrolled = new UnrolledList<int>(3, Allocator.Temp);

            unrolled.Add(0, 10);
            unrolled.Add(0, 11);
            unrolled.Add(0, 12);

            unrolled.Add(2, 1);
            unrolled.Add(2, 2);
            unrolled.Add(2, 3);

            unrolled.Add(1, 7);
            unrolled.Add(1, 8);
            unrolled.Add(1, 9);


            Assert.AreEqual(unrolled.Data.Length, 9);
            Assert.AreEqual(unrolled.GetSubArray(0).ToArray(), new int[] { 10, 11, 12 });
            Assert.AreEqual(unrolled.GetSubArray(1).ToArray(), new int[] { 7, 8, 9 });
            Assert.AreEqual(unrolled.GetSubArray(2).ToArray(), new int[] { 1, 2, 3 });
        }
    }
}