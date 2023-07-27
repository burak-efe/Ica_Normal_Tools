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
    public static class NativeContainerUtils
    {
        // [BurstCompile]
        // public static void CreateAndGetMergedVertices(Mesh.MeshDataArray mda, out NativeList<float3> outMergedVertices, out NativeList<int> map, Allocator allocator)
        // {
        //     var size = GetTotalVertexCountFomMDA(mda);
        //
        //     outMergedVertices = new NativeList<float3>(size, allocator);
        //     map = new NativeList<int>(size, allocator);
        //     GetMergedVertices(mda, ref outMergedVertices, ref map);
        // }

        [BurstCompile]
        public static void GetMergedVertices(in Mesh.MeshDataArray mda, ref NativeArray<float3> outMergedVertices, ref NativeArray<int> map)
        {
            var vertexList = new UnsafeList<NativeArray<float3>>(mda[0].vertexCount, Allocator.Temp);
            for (int i = 0; i < mda.Length; i++)
            {
                var v = new NativeArray<float3>(mda[i].vertexCount, Allocator.Temp);
                mda[i].GetVertices(v.Reinterpret<Vector3>());
                vertexList.Add(v);
            }

            NativeContainerUtils.UnrollArrayToArray(vertexList, ref map, ref outMergedVertices);
        }


        //
        // [BurstCompile]
        // public static void UnrollNestedDataAndCreate<T>
        // (
        //     UnsafeList<NativeList<T>> nestedData,
        //     out NativeList<int> outMapper,
        //     out NativeList<T> outUnrolledData,
        //     Allocator allocator
        // ) where T : unmanaged
        // {
        //     GetUnrolledSizeOfNestedContainer(nestedData, out var unrolledSize);
        //     outUnrolledData = new NativeList<T>(unrolledSize, allocator);
        //     outMapper = new NativeList<int>(nestedData.Length + 1, allocator);
        //
        //     GetUnrollNestedDataToNativeList(nestedData, ref outMapper, ref outUnrolledData);
        // }
        //
        //
        // [BurstCompile]
        // public static void CreateAndGetUnrolledNestedData<T>
        // (
        //     UnsafeList<NativeList<T>> nestedData,
        //     out NativeArray<int> outMapper,
        //     out NativeList<T> outUnrolledData,
        //     Allocator allocator
        // ) where T : unmanaged
        // {
        //     GetUnrolledSizeOfNestedContainer(nestedData, out var unrolledSize);
        //
        //     outUnrolledData = new NativeList<T>(unrolledSize, allocator);
        //     outMapper = new NativeList<int>(nestedData.Length + 1, allocator);
        //     
        //     GetUnrollNestedDataToNativeList(nestedData, ref outMapper, ref outUnrolledData);
        //
        // }

        [BurstCompile]
        public static void GetUnrollNestedDataToNativeArray<T>(UnsafeList<NativeList<T>> nestedData, ref NativeArray<int> outMapper, ref NativeArray<T> outUnrolledData) where T : unmanaged
        {
            var templist = new NativeList<T>(Allocator.Temp);
            GetUnrollNestedDataToNativeList(nestedData, ref outMapper, ref templist);
            outUnrolledData.CopyFrom(templist.AsArray());
        }

        [BurstCompile]
        public static void GetUnrollNestedDataToNativeList<T>(UnsafeList<NativeList<T>> nestedData, ref NativeArray<int> outMapper, ref NativeList<T> outUnrolledData) where T : unmanaged
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
        public static void UnrollArrayToArray<T>(UnsafeList<NativeArray<T>> nestedData, ref NativeArray<int> outMapper, ref NativeArray<T> outUnrolledData) where T : unmanaged
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

        public static int GetTotalVertexCountFomMDA(in Mesh.MeshDataArray mda)
        {
            var count = 0;
            for (int i = 0; i < mda.Length; i++)
            {
                count += mda[i].vertexCount;
            }

            return count;
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