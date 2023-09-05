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
                double3 dotProdSum = 0;

                //Debug.Log( $" total count {subArrayCount}, connected count {connectedCount}, non connected count {subArrayCount - connectedCount}");
                
                //for every connected triangle
                for (int i = 0; i < connectedCount; ++i)
                {
                    int triID = AdjacencyList[subArrayStart + i];
                    dotProdSum += TriNormals[triID];
                }

                double3 normalsOfConnectedTriangles = math.normalize(dotProdSum);
                

                //for every non connected (but adjacent) triangle
                for (int i = 0; i < subArrayCount - connectedCount; i++)
                {
                    int triID = AdjacencyList[subArrayStart + connectedCount + i];
                    double dotProd = math.dot(TriNormals[triID], normalsOfConnectedTriangles);
                    
                    // include it to final vertex normal if angle smooth enough
                    if (dotProd >= CosineThreshold)
                        dotProdSum += TriNormals[triID];
                }
                
                Normals[vertexIndex] = (float3)math.normalize(dotProdSum);
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