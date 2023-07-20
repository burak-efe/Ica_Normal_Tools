using Unity.Burst;
using Unity.Collections;
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
        public static void GetAllIndicesCountOfGivenMeshes(  Mesh.MeshDataArray data, out int count)
        {
            count = 0;
            for (int i = 0; i < data.Length; i++)
            {
                GetAllIndicesCount(data[i], out var meshIndexCount);
                count += meshIndexCount;
            }

        }
        
    }
}