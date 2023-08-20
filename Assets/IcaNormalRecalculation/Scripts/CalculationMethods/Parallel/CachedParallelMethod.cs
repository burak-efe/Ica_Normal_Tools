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
            RecalculateNormalsAndGetHandle(vertices, indices, ref outNormals, adjacencyList, adjacencyMapper, triNormals, out var handle);

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

        /// <summary>
        /// Scheduling the normal recalculating and returns to job handle. Do not forget to Complete job handle!!!
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        /// <param name="outNormals"></param>
        /// <param name="adjacencyList"></param>
        /// <param name="adjacencyMap"></param>
        /// <param name="triNormals"></param>
        /// <param name="handle"></param>
        [BurstCompile]
        public static void RecalculateNormalsAndGetHandle
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
            var pSchedule = new ProfilerMarker("pSchedule");
            
            var triangleCount = indices.Length / 3;

            
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

            var tJobHandle = triNormalJob.ScheduleParallel(triangleCount, NativeUtils.GetBatchCountThatMakesSense(triangleCount), default);

            handle = vertexNormalJob.ScheduleParallel(vertices.Length, NativeUtils.GetBatchCountThatMakesSense(vertices.Length), tJobHandle);

            pSchedule.End();
            
        }


        /// <summary>
        /// Scheduling the tangent recalculating and returns to job handle. Do not forget to Complete job handle!!!
        /// If not dependent on normal handle pass default.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="normals"></param>
        /// <param name="indices"></param>
        /// <param name="uv"></param>
        /// <param name="adjacencyList"></param>
        /// <param name="adjacencyMap"></param>
        /// <param name="tan1"></param>
        /// <param name="tan2"></param>
        /// <param name="outTangents"></param>
        /// <param name="normalHandle"></param>
        /// <param name="tangentHandle"></param>
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
                (indices.Length / 3, NativeUtils.GetBatchCountThatMakesSense(indices.Length / 3), normalHandle);

            tangentHandle = vertexTangentJob.ScheduleParallel
                (vertices.Length,  NativeUtils.GetBatchCountThatMakesSense(vertices.Length), triHandle);

            pCachedParallelTangent.End();
        }
    }
}