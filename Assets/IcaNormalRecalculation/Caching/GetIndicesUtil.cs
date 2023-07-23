using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace IcaNormal
{
    [BurstCompile]
    public static class GetIndicesUtil
    {
        [BurstCompile]
        public static void GetAllIndicesWithNewNativeContainer( this in Mesh.MeshData data, out NativeList<int> outIndices, Allocator allocator)
        {
            var submeshCount = data.subMeshCount;
            var indexCount = 0;
            
            for (int i = 0; i < submeshCount; i++)
            {
                indexCount += data.GetSubMesh(i).indexCount;
            }
            
            outIndices = new NativeList<int>(indexCount,allocator);
            for (int subMeshIndex = 0; subMeshIndex < submeshCount; subMeshIndex++)
            {
                var tempSubmeshIndices = new NativeArray<int>(data.GetSubMesh(subMeshIndex).indexCount, Allocator.Temp);
                data.GetIndices(tempSubmeshIndices, subMeshIndex);

                outIndices.AddRange(tempSubmeshIndices);
            }

        }
        
        [BurstCompile]
        public static void GetAllIndicesCount( in Mesh.MeshData data, out int count)
        {
            var submeshCount = data.subMeshCount;
            count = 0;
            for (int i = 0; i < submeshCount; i++)
            {
                count += data.GetSubMesh(i).indexCount;
            }
        }
        
        [BurstCompile]
        public static void GetAllIndicesCountOfMDA(  Mesh.MeshDataArray data, out int count)
        {
            count = 0;
            for (int i = 0; i < data.Length; i++)
            {
                GetAllIndicesCount(data[i], out var meshIndexCount);
                count += meshIndexCount;
            }

        }
        
        
        
        
        [BurstCompile]
        public static void CreateAndGetMergedIndices(
            Mesh.MeshDataArray mda,
            out NativeList<int> outMergedIndices,
            out NativeArray<int> outMergedIndicesMap,
            Allocator allocator
        )
        {
            GetIndicesUtil.GetAllIndicesCountOfMDA(mda, out int totalIndexCount);
            outMergedIndices = new NativeList<int>(totalIndexCount, allocator);
            outMergedIndicesMap = new NativeList<int>(totalIndexCount + 1, allocator);
            
            GetMergedIndices(mda, ref outMergedIndices, ref outMergedIndicesMap);
        }


        [BurstCompile]
        public static void GetMergedIndices(
            Mesh.MeshDataArray mda,
            ref NativeList<int> mergedIndices,
            ref NativeArray<int> mergedIndicesMap
        )
        {
            var indexList = new UnsafeList<NativeList<int>>(1, Allocator.Temp);
            for (int i = 0; i < mda.Length; i++)
            {
                mda[i].GetAllIndicesWithNewNativeContainer(out var indices, Allocator.Temp);
                indexList.Add(indices);
            }

            NativeContainerUtils.GetUnrollNestedDataToNativeList(indexList, ref mergedIndicesMap, ref mergedIndices);
        }

        
    }
}