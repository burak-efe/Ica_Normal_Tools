using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Ica.Normal
{
    [BurstCompile]
    public static class NormalJobs
    {
        //[BurstCompile]
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


        //[BurstCompile]
        public struct VertexNormalJob : IJobFor
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