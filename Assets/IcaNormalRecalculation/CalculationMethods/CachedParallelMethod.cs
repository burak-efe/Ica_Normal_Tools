using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace IcaNormal
{
    [BurstCompile]
    public static class CachedParallelMethod
    {
        [BurstCompile]
        public static void CalculateNormalData(in Mesh.MeshData meshData, int indicesCount, in NativeArray<int> indices, ref NativeArray<float3> outNormals,
            ref NativeArray<float4> outTangents, in NativeArray<int> adjacencyList, in NativeArray<int2> adjacencyMap)
        {

            var pAllocate = new ProfilerMarker("Allocate");
            pAllocate.Begin();
            var triNormals = new NativeArray<float3>(indicesCount / 3, Allocator.TempJob);
            var vertices = new NativeArray<float3>(meshData.vertexCount, Allocator.TempJob);
            var verticesAsVector = vertices.Reinterpret<Vector3>();
            pAllocate.End();


            var pGetVertices = new ProfilerMarker("pGetVertices");
            pGetVertices.Begin();
            meshData.GetVertices(verticesAsVector);
            pGetVertices.End();

            var pSchedule = new ProfilerMarker("pSchedule");
            pSchedule.Begin();

            var triNormalJob = new TriNormalJob
            {
                Indices = indices,
                TriNormals = triNormals,
                Vertices = vertices
            };

            var vertexNormalJob = new VertexNormalJob
            {
                AdjacencyList = adjacencyList,
                AdjacencyMapper = adjacencyMap,
                TriNormals = triNormals,
                Normals = outNormals
            };



            var tJobHandle = triNormalJob.ScheduleParallel
                (indices.Length / 3, indices.Length / 3 / 64, default);

            var vJobHandle = vertexNormalJob.ScheduleParallel
                (meshData.vertexCount, meshData.vertexCount / 64, tJobHandle);

            pSchedule.End();

            vJobHandle.Complete();

            var pDispose = new ProfilerMarker("Dispose");
            pDispose.Begin();
            triNormals.Dispose();
            vertices.Dispose();
            pDispose.End();
        }


        [BurstCompile]
        private struct TriNormalJob : IJobFor
        {
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
            [ReadOnly] public NativeArray<int2> AdjacencyMapper;
            [ReadOnly] public NativeArray<float3> TriNormals;
            [WriteOnly] public NativeArray<float3> Normals;

            public void Execute(int vertexIndex)
            {
                int2 adjacencyOffsetCount = AdjacencyMapper[vertexIndex];
                float3 dotProdSum = 0;

                for (int i = 0; i < adjacencyOffsetCount.y; ++i)
                {
                    int triID = AdjacencyList[adjacencyOffsetCount.x + i];
                    dotProdSum += TriNormals[triID];
                }

                Normals[vertexIndex] = math.normalize(dotProdSum);
            }
        }
    }
}