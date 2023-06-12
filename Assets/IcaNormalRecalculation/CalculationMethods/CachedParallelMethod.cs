using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace IcaNormal
{ 
    public static class CachedParallelMethod
    {
        public static void CalculateNormalData(Mesh.MeshData meshData, int indicesCount, NativeArray<int> indices, NativeArray<float3> normals,
            NativeArray<float4> tangents, NativeArray<int> adjacencyList, NativeArray<int2> adjacencyMap)
        {
            Profiler.BeginSample("Allocate");
            var triNormals = new NativeArray<float3>(indicesCount / 3, Allocator.TempJob);
            var vertices = new NativeArray<float3>(meshData.vertexCount, Allocator.TempJob);
            var verticesAsVector = vertices.Reinterpret<Vector3>();
            Profiler.EndSample();

            Profiler.BeginSample("GetVertices");
            meshData.GetVertices(verticesAsVector);
            Profiler.EndSample();


            var triNormalJob = new TriNormalJob
            {
                Indices = indices,
                TriNormals = triNormals,
                Vertices = vertices
            };

            var vertexNormalJob = new VertexNormalJob
            {
                AdjacencyList = adjacencyList,
                AdjacencyMap = adjacencyMap,
                TriNormals = triNormals,
                Normals = normals
            };

            Profiler.BeginSample("Schedule");
            
            var tJobHandle = triNormalJob.ScheduleParallel
                (indices.Length / 3, indices.Length / 3 / 64, default);

            var vJobHandle = vertexNormalJob.ScheduleParallel
                (meshData.vertexCount, meshData.vertexCount / 64, tJobHandle);
            
            Profiler.EndSample();

            vJobHandle.Complete();
            
            triNormals.Dispose();
            vertices.Dispose();
        }


        [BurstCompile]
        private struct TriNormalJob : IJobFor
        {
            //public Mesh.MeshData Data;
            [ReadOnly] public NativeArray<int> Indices;
            [ReadOnly] public NativeArray<float3> Vertices;
            [WriteOnly] public NativeArray<float3> TriNormals;

            public void Execute(int index)
            {
                float3 vertexA = Vertices[Indices[index * 3]];
                float3 vertexB = Vertices[Indices[index * 3 + 1]];
                float3 vertexC = Vertices[Indices[index * 3 + 2]];

                // Calculate the normal of the triangle
                float3 crossProduct = math.cross(vertexB - vertexA, vertexC - vertexA);
                TriNormals[index] = crossProduct;
            }
        }

        
        [BurstCompile]
        private struct VertexNormalJob : IJobFor
        {
            [ReadOnly] public NativeArray<int> AdjacencyList;
            [ReadOnly] public NativeArray<int2> AdjacencyMap;
            [ReadOnly] public NativeArray<float3> TriNormals;
            [WriteOnly] public NativeArray<float3> Normals;

            public void Execute(int vertexIndex)
            {
                int2 adjacencyOffsetCount = AdjacencyMap[vertexIndex];
                float3 dotProdSum = 0;

                for (int i = 0; i < adjacencyOffsetCount.y; ++i)
                {
                    int triID = AdjacencyList[adjacencyOffsetCount.x + i];
                    dotProdSum += AdjacencyList[triID];
                }

                Normals[vertexIndex] = math.normalize(dotProdSum);
            }
        }
    }
}