using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Ica.Utils
{
    public static class MeshDataExtensions
    {
        [BurstCompile]
        public static void GetVerticesData( this in Mesh.MeshData data, out NativeArray<float3> outVertices,  Allocator allocator)
        {
            outVertices = new NativeArray<float3>(data.vertexCount, allocator,NativeArrayOptions.UninitializedMemory);
            data.GetVertices(outVertices.Reinterpret<Vector3>());

        }
        [BurstCompile]
        public static void GetNormalsData( this ref Mesh.MeshData data, out NativeArray<float3> outNormals, Allocator allocator)
        {
            outNormals = new NativeArray<float3>(data.vertexCount, allocator,NativeArrayOptions.UninitializedMemory);
            data.GetNormals(outNormals.Reinterpret<Vector3>());
        }
        
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
        public static void GetCountOfAllIndices(this in Mesh.MeshData data, out int indexCount)
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