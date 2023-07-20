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
        public static void CreateAndGetMergedVertices(Mesh.MeshDataArray mda, out NativeList<float3> outMergedVertices, out NativeList<int> map, Allocator allocator)
        {
            var size = GetTotalVertexCountFomMDA(mda);

            outMergedVertices = new NativeList<float3>(size, allocator);
            map = new NativeList<int>(size, allocator);
            GetMergedVertices(mda, ref outMergedVertices, ref map);
        }

        [BurstCompile]
        public static void GetMergedVertices(Mesh.MeshDataArray mda, ref NativeList<float3> outMergedVertices, ref NativeList<int> map)
        {
            var vertexList = new UnsafeList<NativeList<float3>>(1, Allocator.Temp);
            for (int i = 0; i < mda.Length; i++)
            {
                var v = new NativeList<float3>(mda[i].vertexCount, Allocator.Temp);
                mda[i].GetVertices(v.AsArray().Reinterpret<Vector3>());
                vertexList.Add(v);
            }

            NativeContainerUtils.GetUnrollNestedData(vertexList, ref map, ref outMergedVertices);
        }

        [BurstCompile]
        public static void CreateAndGetMergedIndices(
            Mesh.MeshDataArray mda,
            out NativeList<float3> outMergedIndices,
            out NativeList<int> outMergedIndicesMap,
            Allocator allocator
            )
        {
            GetIndicesUtil.GetAllIndicesCountOfGivenMeshes(mda, out int totalIndexCount);
            outMergedIndices = new NativeList<float3>(totalIndexCount, allocator);
            outMergedIndicesMap = new NativeList<int>(totalIndexCount + 1, allocator);
            
            GetMergedVertices(mda, ref outMergedIndices, ref outMergedIndicesMap);
        }


        [BurstCompile]
        public static void GetMergedIndices(
            Mesh.MeshDataArray mda,
            ref NativeList<int> mergedIndices,
            ref NativeList<int> mergedIndicesMap
            )
        {
            var indexList = new UnsafeList<NativeList<int>>(1, Allocator.Temp);
            for (int i = 0; i < mda.Length; i++)
            {
                mda[i].GetAllIndicesWithNewNativeContainer(out var indices, Allocator.Temp);
                indexList.Add(indices);
            }

            GetUnrollNestedData(indexList, ref mergedIndicesMap, ref mergedIndices);
        }


        [BurstCompile]
        public static void UnrollNestedDataAndCreate<T>
        (
            UnsafeList<NativeList<T>> nestedData,
            out NativeList<int> outMapper,
            out NativeList<T> outUnrolledData,
            Allocator allocator
        ) where T : unmanaged
        {
            GetUnrolledSizeOfNestedContainer(nestedData, out var unrolledSize);
            outUnrolledData = new NativeList<T>(unrolledSize, allocator);
            outMapper = new NativeList<int>(nestedData.Length + 1, allocator);

            GetUnrollNestedData(nestedData, ref outMapper, ref outUnrolledData);
        }


        [BurstCompile]
        public static void CreateAndGetUnrolledNestedData<T>
        (
            UnsafeList<NativeList<T>> nestedData,
            out NativeList<int> outMapper,
            out NativeList<T> outUnrolledData,
            Allocator allocator
        ) where T : unmanaged
        {
            GetUnrolledSizeOfNestedContainer(nestedData, out var unrolledSize);

            outUnrolledData = new NativeList<T>(unrolledSize, allocator);
            outMapper = new NativeList<int>(nestedData.Length + 1, allocator);
            
            GetUnrollNestedData(nestedData, ref outMapper, ref outUnrolledData);

        }

        [BurstCompile]
        public static void GetUnrollNestedData<T>(UnsafeList<NativeList<T>> nestedData, ref NativeList<int> outMapper, ref NativeList<T> outUnrolledData) where T : unmanaged
        {
            //GetUnrolledSizeOfNestedContainer(nestedData, out var unrolledSize);
            //outUnrolledData = new NativeList<T>(unrolledSize, allocator);
            //outMapper = new NativeArray<int>(nestedData.Length + 1, allocator);
            
            var mapperIndex = 0;

            for (int i = 0; i < nestedData.Length; i++)
            {
                outUnrolledData.AddRange(nestedData[i]);
                outMapper[i] = mapperIndex;
                mapperIndex += nestedData[i].Length;
            }

            outMapper[^1] = mapperIndex;
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

        public static int GetTotalVertexCountFomMDA(Mesh.MeshDataArray mda)
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