using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

namespace IcaNormal
{
    [BurstCompile]
    public static class CachedParallelMethod
    {
        [BurstCompile]
        public static void CalculateNormalDataUncached
        (
            in NativeArray<float3> vertices,
            in NativeList<int> indices,
            ref NativeArray<float3> outNormals
        )
        {
            VertexPositionMapper.GetVertexPosHashMap(vertices, out var posMap, Allocator.TempJob);
            AdjacencyMapper.CalculateAdjacencyData(vertices, indices, posMap, out var adjacencyList, out var adjacencyMapper, Allocator.TempJob);
            
            var triNormals = new NativeArray<float3>(indices.Length / 3, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            ScheduleAndGetNormalJobHandle(vertices, indices, ref outNormals, adjacencyList, adjacencyMapper,triNormals ,out var handle);

            handle.Complete();
            foreach (var kvPair in posMap)
            {
                kvPair.Value.Dispose();
            }

            triNormals.Dispose();
            posMap.Dispose();
            adjacencyList.Dispose();
            adjacencyMapper.Dispose();
        }

        [BurstCompile]
        public static void ScheduleAndGetNormalJobHandle
        (
            in NativeArray<float3> vertices,
            in NativeList<int> indices,
            ref NativeArray<float3> outNormals,
            in NativeList<int> adjacencyList,
            in NativeArray<int2> adjacencyMap,
            in NativeArray<float3> triNormals,
            out JobHandle handle
        )
        {
            var triangleCount = indices.Length / 3;

            var pAllocate = new ProfilerMarker("Allocate");
            pAllocate.Begin();
            pAllocate.End();


            var pSchedule = new ProfilerMarker("pSchedule");
            pSchedule.Begin();

            var triNormalJob = new NormalJobs.TriNormalJob
            {
                Indices = indices.AsArray(),
                TriNormals = triNormals,
                Vertices = vertices
            };

            var vertexNormalJob = new NormalJobs.VertexNormalJob
            {
                AdjacencyList = adjacencyList.AsArray(),
                AdjacencyMapper = adjacencyMap,
                TriNormals = triNormals,
                Normals = outNormals
            };

            var tJobHandle = triNormalJob.ScheduleParallel(
                triangleCount,
                (int)math.ceil(triangleCount / 128f),
                default);

            handle = vertexNormalJob.ScheduleParallel(
                vertices.Length,
                (int)math.ceil(vertices.Length / 128f),
                tJobHandle);

            pSchedule.End();
            
            var pDispose = new ProfilerMarker("Dispose");
            pDispose.Begin();

            pDispose.End();
        }


      


        [BurstCompile]
        public static void ScheduleAndGetTangentJobHandle
        (
            in NativeArray<float3> vertices,
            in NativeArray<float3> normals,
            in NativeList<int> indices,
            in NativeArray<float2> uv,
            in NativeList<int> adjacencyList,
            in NativeArray<int2> adjacencyMap,
            in NativeArray<float3> tan1,
            in NativeArray<float3> tan2,
            ref NativeArray<float4> outTangents,
            ref JobHandle normalHandle,
            out JobHandle tangentHandle
        )
        {
            var pCachedParallelTangent = new ProfilerMarker("pCachedParallelTangent");
            pCachedParallelTangent.Begin();
            
            var triTangentJob = new TangentJobs.TriTangentJob
            {
                Indices = indices.AsArray(),
                Vertices = vertices,
                UV = uv,
                Tan1 = tan1,
                Tan2 = tan2
            };

            var vertexTangentJob = new TangentJobs.VertexTangentJob
            {
                AdjacencyList = adjacencyList.AsArray(),
                AdjacencyMapper = adjacencyMap,
                Normals = normals,
                Tan1 = tan1,
                Tan2 = tan2,
                Tangents = outTangents
            };
            
            var triHandle = triTangentJob.ScheduleParallel
                (indices.Length / 3, indices.Length / 3 / 64, normalHandle);

            tangentHandle = vertexTangentJob.ScheduleParallel
                (vertices.Length, vertices.Length / 64, triHandle);

            pCachedParallelTangent.End();
        }


    }
}