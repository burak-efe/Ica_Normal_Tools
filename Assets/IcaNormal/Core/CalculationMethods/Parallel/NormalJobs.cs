using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Ica.Normal
{
    [BurstCompile]
    public static class NormalJobs
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


        //[BurstCompile]
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
    }
}