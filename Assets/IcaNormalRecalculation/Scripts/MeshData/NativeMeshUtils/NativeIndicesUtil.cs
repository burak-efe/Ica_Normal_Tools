using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace IcaNormal
{
    [BurstCompile]
    public static class NativeIndicesUtil
    {
        /// <summary>
        /// Creates a new native list with specified allocator then fill it with indices of given mesh's all submesh-es. similar to Mesh.triangles method.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="outIndices"></param>
        /// <param name="allocator"></param>
        [BurstCompile]
        public static void GetAllIndicesData(this in Mesh.MeshData data, out NativeList<int> outIndices, Allocator allocator)
        {
            GetCountOfAllIndices(data,out var indexCount);
            
            outIndices = new NativeList<int>(indexCount, allocator);
            
            for (int subMeshIndex = 0; subMeshIndex < data.subMeshCount; subMeshIndex++)
            {
                var tempSubmeshIndices = new NativeArray<int>(data.GetSubMesh(subMeshIndex).indexCount, Allocator.Temp,NativeArrayOptions.UninitializedMemory);
                data.GetIndices(tempSubmeshIndices, subMeshIndex);

                outIndices.AddRange(tempSubmeshIndices);
            }
        }
        
        /// <summary>
        /// counts and return given mesh's all indices.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="indexCount"></param>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetCountOfAllIndices(in Mesh.MeshData data, out int indexCount)
        {
            var submeshCount = data.subMeshCount;
            indexCount = 0;
            for (int i = 0; i < submeshCount; i++)
            {
                indexCount += data.GetSubMesh(i).indexCount;
            }
        }
    }
}