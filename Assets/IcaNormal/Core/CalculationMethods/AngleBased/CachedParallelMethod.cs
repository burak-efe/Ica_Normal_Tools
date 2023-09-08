using Ica.Normal.JobStructs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Assertions;
using Ica.Utils;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

namespace Ica.Normal
{
    [BurstCompile]
    public static class CachedParallelMethod
    {
        public static void RecalculateNormalsIca(this Mesh mesh, float angle = 180f)
        {
            var mda = Mesh.AcquireReadOnlyMeshData(mesh);
            
            CalculateNormalDataUncached(mda[0], out var outNormals, Allocator.TempJob, angle);
            
            mesh.SetNormals(outNormals.AsArray().Reinterpret<Vector3>());
            
            outNormals.Dispose();
            mda.Dispose();
        }

        /// <summary>
        /// For procedural mesh.
        /// </summary>
        /// <param name="meshData">mesh as data source</param>
        /// <param name="outNormals">output normals will be allocated with given allocator.
        /// Its your responsibility to how to apply normals.</param>
        /// <param name="allocator">TempJob or Persistent. Cannot be temp allocator since will be passed a job.</param>
       // [BurstCompile]
        public static void CalculateNormalDataUncached
        (
            in Mesh.MeshData meshData,
            out NativeList<float3> outNormals,
            Allocator allocator,
            float angle = 180f
        )
        {
            Assert.IsFalse(allocator == Allocator.Temp);

            var vertices = new NativeList<float3>(meshData.vertexCount, Allocator.Temp);
            var indices = new NativeList<int>(meshData.vertexCount, Allocator.Temp);
            meshData.GetVerticesDataAsList(ref vertices);
            meshData.GetAllIndicesDataAsList(ref indices);
            outNormals = new NativeList<float3>(meshData.vertexCount, allocator);
            outNormals.ResizeUninitialized(meshData.vertexCount);
            
            VertexPositionMapper.GetVertexPosHashMap(vertices.AsArray(), out var posMap, Allocator.Temp);
            AdjacencyMapper.CalculateAdjacencyData(vertices.AsArray(), indices.AsArray(), posMap, out var adjacencyList, out var adjacencyMapper, out var connectedCountMap, Allocator.Temp);
            RecalculateNormalsAndGetHandle(vertices, indices, ref outNormals, adjacencyList, adjacencyMapper, connectedCountMap, out var handle, angle);
            
            handle.Complete();
            
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
            in NativeList<int> connectedCountMap,
            out JobHandle handle,
            float angle = 180f
        )
        {
            angle = math.clamp(angle, 0, 180);
            Assert.IsTrue(vertices.Length == outNormals.Length);
            //Assert.IsTrue(triNormals.Length == indices.Length / 3);
            var pSchedule = new ProfilerMarker("pSchedule");

            var triangleCount = indices.Length / 3;

            pSchedule.Begin();

            var triNormals = new NativeArray<float3>(indices.Length / 3, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var triNormalJob = new TriNormalJob
            {
                Indices = indices.AsArray(),
                TriNormals = triNormals,
                Vertices = vertices.AsArray()
            };

            var triNormalJobHandle = triNormalJob.ScheduleParallel(triangleCount, JobUtils.GetBatchCountThatMakesSense(triangleCount), default);

            if (angle == 180f)
            {
                var vertexNormalJob = new SmoothVertexNormalJob()
                {
                    AdjacencyList = adjacencyList.AsArray(),
                    AdjacencyMapper = adjacencyStartIndicesMap.AsArray(),
                    TriNormals = triNormals,
                    Normals = outNormals.AsArray(),
                };
                handle = vertexNormalJob.ScheduleParallel(vertices.Length, JobUtils.GetBatchCountThatMakesSense(vertices.Length), triNormalJobHandle);
                handle = triNormals.Dispose(handle);
            }
            else
            {
                var vertexNormalJob = new AngleBasedVertexNormalJob
                {
                    AdjacencyList = adjacencyList.AsArray(),
                    AdjacencyMapper = adjacencyStartIndicesMap.AsArray(),
                    TriNormals = triNormals,
                    Normals = outNormals.AsArray(),
                    ConnectedMapper = connectedCountMap.AsArray(),
                    CosineThreshold = Mathf.Cos(angle * Mathf.Deg2Rad)
                };

                //Debug.Log(math.cos(angle * Mathf.Deg2Rad));
                handle = vertexNormalJob.ScheduleParallel(vertices.Length, JobUtils.GetBatchCountThatMakesSense(vertices.Length), triNormalJobHandle);
                handle = triNormals.Dispose(handle);
            }


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