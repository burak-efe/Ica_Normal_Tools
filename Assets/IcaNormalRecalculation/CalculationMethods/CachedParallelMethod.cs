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
        //[BurstCompile]
        public static void CalculateNormalDataUncached
        (
            in NativeArray<float3> vertices,
            in NativeList<int> indices,
            ref NativeArray<float3> outNormals
        )
        {
            VertexPositionMapper.GetVertexPosHashMap(vertices, out var posMap, Allocator.TempJob);
            //DuplicateVerticesMapper.GetDuplicateVerticesMap(posMap, out var duplicateMap, Allocator.TempJob);
            AdjacencyMapper.CalculateAdjacencyData(vertices,indices,posMap,out var adjacencyList,out var adjacencyMapper,Allocator.TempJob);
            CalculateNormalData(vertices,indices,ref outNormals,adjacencyList,adjacencyMapper);

            foreach (var kvPair in posMap)
            {
                kvPair.Value.Dispose();
            }
            posMap.Dispose();
            adjacencyList.Dispose();
            adjacencyMapper.Dispose();
        }

        [BurstCompile]
        public static void CalculateNormalData
        (
            in NativeArray<float3> vertices,
            in NativeList<int> indices,
            ref NativeArray<float3> outNormals,
            in NativeList<int> adjacencyList,
            in NativeArray<int2> adjacencyMap
        )
        {
            var pAllocate = new ProfilerMarker("Allocate");
            pAllocate.Begin();
            var triNormals = new NativeArray<float3>(indices.Length / 3, Allocator.TempJob);
            //var vertices = new NativeArray<float3>(meshData.vertexCount, Allocator.TempJob);
            //var verticesAsVector = vertices.Reinterpret<Vector3>();
            pAllocate.End();

            // var pGetVertices = new ProfilerMarker("pGetVertices");
            // pGetVertices.Begin();
            // meshData.GetVertices(verticesAsVector);
            // pGetVertices.End();

            var pSchedule = new ProfilerMarker("pSchedule");
            pSchedule.Begin();

            var triNormalJob = new TriNormalJob
            {
                Indices = indices.AsArray(),
                TriNormals = triNormals,
                Vertices = vertices
            };

            var vertexNormalJob = new VertexNormalJob
            {
                AdjacencyList = adjacencyList.AsArray(),
                AdjacencyMapper = adjacencyMap,
                TriNormals = triNormals,
                Normals = outNormals
            };

            var tJobHandle = triNormalJob.ScheduleParallel
                (indices.Length / 3, indices.Length / 3 / 64, default);

            var vJobHandle = vertexNormalJob.ScheduleParallel
                (vertices.Length, vertices.Length / 64, tJobHandle);

            pSchedule.End();

            vJobHandle.Complete();

            var pDispose = new ProfilerMarker("Dispose");
            pDispose.Begin();
            triNormals.Dispose();
            //vertices.Dispose();
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


        [BurstCompile]
        public static void CalculateTangentData
        (

            in NativeArray<float3> vertices,
            in NativeArray<float3> normals,
            in NativeList<int> indices,
            in NativeArray<float2> uv,
            in NativeList<int> adjacencyList,
            in NativeArray<int2> adjacencyMap,
            ref NativeArray<float4> outTangents
        )
        {
            var p = new ProfilerMarker("pCachedParallelTangent");
            p.Begin();
            //var vertices = new NativeArray<float3>(meshData.vertexCount, Allocator.TempJob);
            //meshData.GetVertices(vertices.Reinterpret<Vector3>());
            var tan1 = new NativeArray<float3>(indices.Length / 3, Allocator.TempJob);
            var tan2 = new NativeArray<float3>(indices.Length / 3, Allocator.TempJob);

            var triTangentJob = new TriTangentJob
            {
                Indices = indices.AsArray(),
                Vertices = vertices,
                UV = uv,
                Tan1 = tan1,
                Tan2 = tan2
            };

            var vertexTangentJob = new VertexTangentJob
            {
                AdjacencyList = adjacencyList.AsArray(),
                AdjacencyMapper = adjacencyMap,
                Normals = normals,
                Tan1 = tan1,
                Tan2 = tan2,
                Tangents = outTangents
            };


            var triHandle = triTangentJob.ScheduleParallel
                (indices.Length / 3, indices.Length / 3 / 64, default);

            var vertHandle = vertexTangentJob.ScheduleParallel
                (vertices.Length, vertices.Length / 64, triHandle);

            vertHandle.Complete();
            //vertices.Dispose();
            tan1.Dispose();
            tan2.Dispose();
            p.End();
        }


        [BurstCompile]
        private struct TriTangentJob : IJobFor
        {
            [ReadOnly] public NativeArray<int> Indices;
            [ReadOnly] public NativeArray<float3> Vertices;
            [ReadOnly] public NativeArray<float2> UV;
            [WriteOnly] public NativeArray<float3> Tan1;
            [WriteOnly] public NativeArray<float3> Tan2;

            public void Execute(int triIndex)
            {
                int i1 = Indices[triIndex * 3];
                int i2 = Indices[triIndex * 3 + 1];
                int i3 = Indices[triIndex * 3 + 2];

                float3 v1 = Vertices[i1];
                float3 v2 = Vertices[i2];
                float3 v3 = Vertices[i3];

                float2 w1 = UV[i1];
                float2 w2 = UV[i2];
                float2 w3 = UV[i3];

                float x1 = v2.x - v1.x;
                float x2 = v3.x - v1.x;
                float y1 = v2.y - v1.y;
                float y2 = v3.y - v1.y;
                float z1 = v2.z - v1.z;
                float z2 = v3.z - v1.z;

                float s1 = w2.x - w1.x;
                float s2 = w3.x - w1.x;
                float t1 = w2.y - w1.y;
                float t2 = w3.y - w1.y;

                float div = s1 * t2 - s2 * t1;
                float r = div == 0.0f ? 0.0f : 1.0f / div;

                var sDir = new float3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                var tDir = new float3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                Tan1[triIndex] = sDir;
                Tan2[triIndex] = tDir;
            }
        }

        [BurstCompile]
        private struct VertexTangentJob : IJobFor
        {
            [ReadOnly] public NativeArray<int> AdjacencyList;
            [ReadOnly] public NativeArray<int2> AdjacencyMapper;
            [ReadOnly] public NativeArray<float3> Normals;
            [ReadOnly] public NativeArray<float3> Tan1;
            [ReadOnly] public NativeArray<float3> Tan2;
            [WriteOnly] public NativeArray<float4> Tangents;

            public void Execute(int vertexIndex)
            {
                int2 adjacencyOffsetCount = AdjacencyMapper[vertexIndex];
                float3 t1Sum = 0;
                float3 t2Sum = 0;

                for (int i = 0; i < adjacencyOffsetCount.y; ++i)
                {
                    int triID = AdjacencyList[adjacencyOffsetCount.x + i];
                    t1Sum += Tan1[triID];
                    t2Sum += Tan2[triID];
                }

                Vector3 nTemp = Normals[vertexIndex];
                Vector3 tTemp = t1Sum;

                //TODO: Use math library and float3 here, and remove temp values
                Vector3.OrthoNormalize(ref nTemp, ref tTemp);

                float3 n = nTemp;
                float3 t = tTemp;
                var w = (math.dot(math.cross(n, t), t2Sum) < 0.0f) ? -1.0f : 1.0f;
                Tangents[vertexIndex] = new float4(t.x, t.y, t.z, w);
            }
        }
    }
}