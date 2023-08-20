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
        public static void GetAllIndicesOfMeshWithNewNativeList(this in Mesh.MeshData data, out NativeList<int> outIndices, Allocator allocator)
        {
            var submeshCount = data.subMeshCount;
            var indexCount = 0;

            for (int i = 0; i < submeshCount; i++)
            {
                indexCount += data.GetSubMesh(i).indexCount;
            }

            outIndices = new NativeList<int>(indexCount, allocator);
            for (int subMeshIndex = 0; subMeshIndex < submeshCount; subMeshIndex++)
            {
                var tempSubmeshIndices = new NativeArray<int>(data.GetSubMesh(subMeshIndex).indexCount, Allocator.Temp);
                data.GetIndices(tempSubmeshIndices, subMeshIndex);

                outIndices.AddRange(tempSubmeshIndices);
            }
        }
        
        /// <summary>
        /// counts and return given mesh's all indices.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="count"></param>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetCountOfAllIndicesOfMesh(in Mesh.MeshData data, out int count)
        {
            var submeshCount = data.subMeshCount;
            count = 0;
            for (int i = 0; i < submeshCount; i++)
            {
                count += data.GetSubMesh(i).indexCount;
            }
        }
    }
}