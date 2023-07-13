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
        [BurstCompile]
        public static void CreateMergedVertices(Mesh.MeshDataArray mda, out NativeList<float3> outMergedVertices,out NativeArray<int> map, Allocator allocator)
        {
            var vertexList = new UnsafeList<NativeArray<float3>>(1, Allocator.Temp);
            for (int i = 0; i < mda.Length; i++)
            {
                var v = new NativeArray<float3>(mda[i].vertexCount, Allocator.Temp);
                mda[i].GetVertices(v.Reinterpret<Vector3>());
                vertexList.Add(v);
            }

            NativeContainerUtils.UnRoller(vertexList, out  map, out outMergedVertices, allocator);
        }

        [BurstCompile]
        public static void CreateMergedIndices(Mesh.MeshDataArray mda, out NativeList<int> outMergedIndices,out NativeArray<int> map,Allocator allocator)
        {
            var indexList = new UnsafeList<NativeArray<int>>(1, Allocator.Temp);
            for (int i = 0; i < mda.Length; i++)
            {
                //var indices = new NativeArray<int>();
                mda[i].GetAllIndices(out var indices, Allocator.Temp);
                indexList.Add(indices);
            }

            NativeContainerUtils.UnRoller(indexList, out map, out outMergedIndices, allocator);
        }

        [BurstCompile]
        public static void UnRoller<T>(UnsafeList<NativeArray<T>> nestedData, out NativeArray<int> outMapper, out NativeList<T> outUnrolledData, Allocator allocator) where T : unmanaged
        {
            var unrolledSize = 0;
            for (int i = 0; i < nestedData.Length; i++)
            {
                unrolledSize += nestedData[i].Length;
            }

            outUnrolledData = new NativeList<T>(unrolledSize, allocator);
            outMapper = new NativeArray<int>(nestedData.Length + 1, allocator);


            var mapperIndex = 0;

            for (int i = 0; i < nestedData.Length; i++)
            {
                outUnrolledData.AddRange(nestedData[i]);
                outMapper[i] = mapperIndex;
                mapperIndex += nestedData[i].Length;
            }

            outMapper[^1] = mapperIndex;
        }

        // private static void GetUnrolledSizeOfVertices<T>(UnsafeList<NativeArray<T>> nestedContainer, out int size) where T : unmanaged
        // {
        // }


        [BurstCompile]
        public static void GetUnrolledSizeOfNestedContainer<T>(UnsafeList<NativeArray<T>> nestedContainer ,out int size) where T :  unmanaged
        {
            size = 0;
            for (int i = 0; i < nestedContainer.Length; i++)
            {
                size += nestedContainer[i].Length;
            }
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