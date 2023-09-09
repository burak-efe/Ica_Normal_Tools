using Ica.Normal.JobStructs;
using Ica.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Assertions;

namespace Ica.Normal
{
    public static class UncachedMethod
    {
        [BurstCompile]
        public static void UncachedNormalRecalculate
        (
            in Mesh.MeshData meshData,
            out NativeList<float3> outNormals,
            Allocator allocator,
            float angle = 180f)
        {
            Assert.IsFalse(allocator == Allocator.Temp);
            
            angle = math.clamp(angle, 0, 180);



            var vertices = new NativeList<float3>(meshData.vertexCount, Allocator.Temp);
            meshData.GetVerticesDataAsList(ref vertices);
            
            var indices = new NativeList<int>(meshData.vertexCount, Allocator.Temp);
            meshData.GetAllIndicesDataAsList(ref indices);
            
            outNormals = new NativeList<float3>(meshData.vertexCount, allocator);
            outNormals.Resize(meshData.vertexCount,NativeArrayOptions.ClearMemory);
            
            
            var triNormals = new NativeArray<float3>(indices.Length / 3, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var triNormalJob = new TriNormalJob
            {
                TriNormals = triNormals,
                Vertices = vertices.AsArray(),
                Indices = indices.AsArray(),
            };

            var h1 = triNormalJob.ScheduleParallel(indices.Length / 3, 100, default);
            
            if (angle == 180f)
            {
                var t = new UncachedSmoothVertexNormalJob()
                {
                    OutNormals = outNormals.AsArray(),
                    Vertices = vertices.AsArray(),
                    Indices = indices.AsArray(),
                    TriNormals = triNormals,
                    PGetVertexPosHashMap = new ProfilerMarker("pPosMap"),
                    PCalculate = new ProfilerMarker("pCalculate"),
                };
                var h2 = t.Schedule(h1);
                h2.Complete();
            }
            else
            {
                var t = new UncachedAngleVertexNormalJob()
                {
                    OutNormals = outNormals.AsArray(),
                    Vertices = vertices.AsArray(),
                    Indices = indices.AsArray(),
                    TriNormals = triNormals,
                    CosineThreshold = Mathf.Cos(angle * Mathf.Deg2Rad),
                    PGetVertexPosHashMap = new ProfilerMarker("pPosMap"),
                    PCalculate = new ProfilerMarker("pCalculate"),
                };
                var h2 = t.Schedule(h1);
                h2.Complete();
                
            }
            

            
            triNormals.Dispose();
        }

        /// <summary>
        /// For procedural mesh.
        /// </summary>
        /// <param name="meshData">mesh as data source</param>
        /// <param name="outNormals">output normals will be allocated with given allocator.
        /// Its your responsibility to how to apply normals.</param>
        /// <param name="allocator">TempJob or Persistent. Cannot be temp allocator since will be passed a job.</param>
        [BurstCompile]
        public static void CalculateNormalDataUncached_OBSOLETE
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
            CachedMethod.RecalculateNormalsAndGetHandle(vertices, indices, ref outNormals, adjacencyList, adjacencyMapper, connectedCountMap, out var handle, angle);

            handle.Complete();
        }
    }
}