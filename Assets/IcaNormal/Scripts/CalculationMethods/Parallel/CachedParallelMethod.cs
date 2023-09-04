using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Assertions;
using Ica.Utils;

namespace Ica.Normal
{
    [BurstCompile]
    public static class CachedParallelMethod
    {
        /// <summary>
        /// For procedural mesh.
        /// </summary>
        /// <param name="mesh">mesh as data source</param>
        /// <param name="outNormals">output normals will be allocated with given allocator.
        /// Its your responsibility to how to apply normals.</param>
        /// <param name="allocator">TempJob or Persistent. Cannot be temp allocator since will be passed a job.</param>

        public static void CalculateNormalDataUncached
        (
            Mesh mesh,
            out NativeList<float3> outNormals,
            Allocator allocator
        )
        {
            Assert.IsFalse(allocator == Allocator.Temp);

            var mda = Mesh.AcquireReadOnlyMeshData(mesh);
            var data = mda[0];
            var vertices = new NativeList<float3>(data.vertexCount, Allocator.Temp);
            var indices = new NativeList<int>(data.vertexCount, Allocator.Temp);
            data.GetVerticesDataAsList(ref vertices);
            data.GetAllIndicesDataAsList(ref indices);

            VertexPositionMapper.GetVertexPosHashMap(vertices.AsArray(), out var posMap, Allocator.TempJob);
            AdjacencyMapper.CalculateAdjacencyData(vertices.AsArray(), indices.AsArray(), posMap, out var adjacencyList, out var adjacencyMapper, Allocator.TempJob);
            outNormals = new NativeList<float3>(data.vertexCount, allocator);

            var triNormals = new NativeList<float3>(indices.Length / 3, Allocator.TempJob);
            triNormals.Resize(indices.Length / 3, NativeArrayOptions.UninitializedMemory);
            RecalculateNormalsAndGetHandle(vertices, indices, ref outNormals, adjacencyList, adjacencyMapper, triNormals, out var handle);
            handle.Complete();

            foreach (var kvPair in posMap)
            {
                kvPair.Value.Dispose();
            }

            mda.Dispose();
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
        /// <param name="adjacencyStartIndicesMap"></param>
        /// <param name="triNormals"></param>
        /// <param name="handle"></param>
        [BurstCompile]
        public static void RecalculateNormalsAndGetHandle
        (
            in NativeList<float3> vertices,
            in NativeList<int> indices,
            ref NativeList<float3> outNormals,
            in NativeList<int> adjacencyList,
            in NativeList<int> adjacencyStartIndicesMap,
            in NativeList<float3> triNormals,
            out JobHandle handle
        )
        {
            Assert.IsTrue(vertices.Length == outNormals.Length);
            Assert.IsTrue(triNormals.Length == indices.Length / 3);
            var pSchedule = new ProfilerMarker("pSchedule");

            var triangleCount = indices.Length / 3;

            pSchedule.Begin();

            var triNormalJob = new NormalJobs.TriNormalJob
            {
                Indices = indices.AsArray(),
                TriNormals = triNormals.AsArray(),
                Vertices = vertices.AsArray()
            };

            var vertexNormalJob = new NormalJobs.VertexNormalJob
            {
                AdjacencyList = adjacencyList.AsArray(),
                AdjacencyMapper = adjacencyStartIndicesMap.AsArray(),
                TriNormals = triNormals.AsArray(),
                Normals = outNormals.AsArray()
            };

            var tJobHandle = triNormalJob.ScheduleParallel(triangleCount, JobUtils.GetBatchCountThatMakesSense(triangleCount), default);

            handle = vertexNormalJob.ScheduleParallel(vertices.Length, JobUtils.GetBatchCountThatMakesSense(vertices.Length), tJobHandle);

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
            in NativeList<float3> vertices,
            in NativeList<float3> normals,
            in NativeList<int> indices,
            in NativeList<float2> uv,
            in NativeList<int> adjacencyList,
            in NativeList<int> adjacencyMap,
            in NativeList<float3> tan1,
            in NativeList<float3> tan2,
            ref NativeList<float4> outTangents,
            ref JobHandle normalHandle,
            out JobHandle tangentHandle
        )
        {
            var pCachedParallelTangent = new ProfilerMarker("pCachedParallelTangent");
            pCachedParallelTangent.Begin();

            var triTangentJob = new TangentJobs.TriTangentJob
            {
                Indices = indices.AsArray(),
                Vertices = vertices.AsArray(),
                UV = uv.AsArray(),
                Tan1 = tan1.AsArray(),
                Tan2 = tan2.AsArray()
            };

            var vertexTangentJob = new TangentJobs.VertexTangentJob
            {
                AdjacencyList = adjacencyList.AsArray(),
                AdjacencyStartIndices = adjacencyMap.AsArray(),
                Normals = normals.AsArray(),
                Tan1 = tan1.AsArray(),
                Tan2 = tan2.AsArray(),
                Tangents = outTangents.AsArray()
            };

            var triHandle = triTangentJob.ScheduleParallel
                (indices.Length / 3, JobUtils.GetBatchCountThatMakesSense(indices.Length / 3), normalHandle);

            tangentHandle = vertexTangentJob.ScheduleParallel
                (vertices.Length, JobUtils.GetBatchCountThatMakesSense(vertices.Length), triHandle);

            pCachedParallelTangent.End();
        }
    }
}