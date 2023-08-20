using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace IcaNormal
{
    public static class MeshDataUtils
    {
        public static  void GetVerticesData( this ref Mesh.MeshData data, out NativeArray<float3> outVertices, Allocator allocator)
        {
            outVertices = new NativeArray<float3>(data.vertexCount, allocator,NativeArrayOptions.UninitializedMemory);
            data.GetVertices(outVertices.Reinterpret<Vector3>());
        }
        public static  void GetNormalsData( this ref Mesh.MeshData data, out NativeArray<float3> outNormals, Allocator allocator)
        {
            outNormals = new NativeArray<float3>(data.vertexCount, allocator,NativeArrayOptions.UninitializedMemory);
            data.GetNormals(outNormals.Reinterpret<Vector3>());
        }
    }
}