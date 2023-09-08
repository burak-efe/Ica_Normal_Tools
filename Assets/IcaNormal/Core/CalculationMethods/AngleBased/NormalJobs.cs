using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

namespace Ica.Normal.JobStructs
{
    [BurstCompile]
    public struct TriNormalJob : IJobFor
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
            //var normalized = math.normalize(crossProduct);
            TriNormals[index] = crossProduct;
        }
    }




    [BurstCompile]
    public struct AngleBasedVertexNormalJob : IJobFor
    {
        [ReadOnly] public NativeArray<int> AdjacencyList;
        [ReadOnly] public NativeArray<int> AdjacencyMapper;
        [ReadOnly] public NativeArray<int> ConnectedMapper;
        [ReadOnly] public NativeArray<float3> TriNormals;
        [ReadOnly] public float CosineThreshold;
        [WriteOnly] public NativeArray<float3> Normals;

        public void Execute(int vertexIndex)
        {
            int subArrayStart = AdjacencyMapper[vertexIndex];
            int subArrayCount = AdjacencyMapper[vertexIndex + 1] - AdjacencyMapper[vertexIndex];
            int connectedCount = ConnectedMapper[vertexIndex];
            double3 sum = 0;


            // for (int i = 0; i < subArrayCount; i++)
            // {
            //     var firstIndex = AdjacencyList[subArrayStart + i];
            //     var firstNormal = TriNormals[firstIndex];
            //
            //     for (int j = 0; j < subArrayCount; j++)
            //     {
            //         var secondIndex = AdjacencyList[subArrayStart + j];
            //         
            //         if (firstIndex == secondIndex)
            //         {
            //             Debug.Log("true");
            //             sum += TriNormals[AdjacencyList[subArrayStart + j]];
            //             continue;
            //         }
            //
            //         // var secondNormal = TriNormals[secondIndex];
            //         // var dot = math.dot(math.normalize(firstNormal), math.normalize(secondNormal));
            //         //
            //         // if (dot >= CosineThreshold)
            //         // {
            //         //     Debug.Log(dot + " is bigger than " + CosineThreshold);
            //         //     sum += secondNormal;
            //         // }
            //         // else
            //         // {
            //         //     Debug.Log(dot + " is NOT bigger than " + CosineThreshold);
            //         // }
            //     }
            // }


            //for every connected triangle
            for (int i = 0; i < connectedCount; ++i)
            {
                int triID = AdjacencyList[subArrayStart + i];
                sum += TriNormals[triID];
            }

            double3 normalFromConnectedTriangles = math.normalize(sum);

            //for every non connected (but adjacent) triangle
            for (int i = 0; i < subArrayCount - connectedCount; i++)
            {
                int triID = AdjacencyList[subArrayStart + connectedCount + i];
                var normalizedCurrentTri = math.normalize(TriNormals[triID]);
                double dotProd = math.dot(normalFromConnectedTriangles, normalizedCurrentTri);

                // include it to final vertex normal if angle smooth enough
                if (dotProd >= CosineThreshold)
                {
                    sum += TriNormals[triID];
                }
            }

            Normals[vertexIndex] = (float3)math.normalize(sum);
        }
    }


    [BurstCompile]
    public struct SmoothVertexNormalJob : IJobFor
    {
        [ReadOnly] public NativeArray<int> AdjacencyList;
        [ReadOnly] public NativeArray<int> AdjacencyMapper;
        [ReadOnly] public NativeArray<float3> TriNormals;
        [WriteOnly] public NativeArray<float3> Normals;

        public void Execute(int vertexIndex)
        {
            int subArrayStart = AdjacencyMapper[vertexIndex];
            int subArrayCount = AdjacencyMapper[vertexIndex + 1] - AdjacencyMapper[vertexIndex];
            double3 dotProdSum = 0;

            //for every adjacent triangle
            for (int i = 0; i < subArrayCount; ++i)
            {
                int triID = AdjacencyList[subArrayStart + i];
                dotProdSum += TriNormals[triID];
            }

            var normalized = math.normalize(dotProdSum);

            Normals[vertexIndex] = (float3)normalized;
        }
    }
    
    
        [BurstCompile]
    public struct VertexNormalJob : IJob
    {
        [ReadOnly] public NativeArray<float3> TriNormals;
        [ReadOnly] public NativeArray<float3> Vertices;
        [ReadOnly] public NativeArray<int> Indices;
        [WriteOnly] public NativeArray<float3> OutNormals;
        [ReadOnly] public float CosineThreshold;

        public void Execute()
        {
            var pGetVertexPosHashMap = new ProfilerMarker("pGetVertexPosHashMap");
            var pAddToList = new ProfilerMarker("pAddToList");
            var pAddNewPair = new ProfilerMarker("pAddNewPair");
            var pCreateList = new ProfilerMarker("pCreateList");
            pGetVertexPosHashMap.Begin();

            var posMap = new UnsafeHashMap<float3, NativeList<int>>(Vertices.Length, Allocator.Temp);

            for (int vertexIndex = 0; vertexIndex < Vertices.Length; vertexIndex++)
            {
                if (posMap.TryGetValue(Vertices[vertexIndex], out var vertexIndexList))
                {
                    pAddToList.Begin();
                    vertexIndexList.Add(vertexIndex);
                    pAddToList.End();
                }
                else
                {
                    pCreateList.Begin();
                    vertexIndexList = new NativeList<int>(1, Allocator.Temp) { vertexIndex };
                    pCreateList.End();

                    //vertexIndexList.Add(vertexIndex);
                    pAddNewPair.Begin();
                    posMap.Add(Vertices[vertexIndex], vertexIndexList);
                    pAddNewPair.End();
                }
            }

            pGetVertexPosHashMap.End();


            foreach (var kvPair in posMap)
            {
                var list = kvPair.Value;
                for (int i = 0; i < list.Length; i++)
                {
                    double3 sum = 0;


                    for (int j = 0; j < list.Length; j++)
                    {
                        if (Indices[i] == Indices[j])
                        {
                            sum += TriNormals[Indices[j]];
                        }
                        else
                        {
                            var dot = math.dot(math.normalize(TriNormals[Indices[i]]), math.normalize(TriNormals[Indices[j]]));

                            if (dot >= CosineThreshold)
                            {
                                sum += TriNormals[Indices[j]];
                            }
                        }
                    }

                    OutNormals[i] = (float3)math.normalize(sum);
                }
            }
        }
    }
}