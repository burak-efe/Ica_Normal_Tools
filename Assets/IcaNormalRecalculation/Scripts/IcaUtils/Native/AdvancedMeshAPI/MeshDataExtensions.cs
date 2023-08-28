using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Ica.Utils
{
    public static class MeshDataExtensions
    {
        public static  NativeArray<float3> GetVerticesData( this ref Mesh.MeshData data,  Allocator allocator)
        {
            var outVertices = new NativeArray<float3>(data.vertexCount, allocator,NativeArrayOptions.UninitializedMemory);
            data.GetVertices(outVertices.Reinterpret<Vector3>());
            return outVertices;
        }
        public static NativeArray<float3> GetNormalsData( this ref Mesh.MeshData data, Allocator allocator)
        {
            var outNormals = new NativeArray<float3>(data.vertexCount, allocator,NativeArrayOptions.UninitializedMemory);
            data.GetNormals(outNormals.Reinterpret<Vector3>());
            return outNormals;
        }
    }
}