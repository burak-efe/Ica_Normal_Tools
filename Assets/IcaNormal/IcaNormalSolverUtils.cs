/*====================================================
*
* Francesco Cucchiara - 3POINT SOFT
* http://threepointsoft.altervista.org
*
=====================================================*/
/* 
 * The following code was taken from: https://schemingdeveloper.com
 *
 * Visit our game studio website: http://stopthegnomes.com
 *
 * License: You may use this code however you see fit, as long as you include this notice
 *          without any modifications.
 *
 *          You may not publish a paid asset on Unity store if its main function is based on
 *          the following code, but you may publish a paid asset that uses this code.
 *
 *          If you intend to use this in a Unity store asset or a commercial project, it would
 *          be appreciated, but not required, if you let me know with a link to the asset. If I
 *          don't get back to you just go ahead and use it anyway!
 */

using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace IcaNormal
{
    public static class IcaNormalSolverUtils
    {
        public static void RecalculateNormalsSmooth(this Mesh mesh, float angle, bool recalculateTangents = true)
        {
            var dataArray = Mesh.AcquireReadOnlyMeshData(mesh);
            var data = dataArray[0];
            var outputData = new NativeArray<float3>(data.vertexCount, Allocator.TempJob);
            var outputTangents = new NativeArray<float4>(data.vertexCount, Allocator.TempJob);


            var normalJob = new NormalJob
            {
                Data = data,
                Angle = angle,
                Normals = outputData,
                Tangents = outputTangents,
                RecalculateTangents = recalculateTangents
            };
            var handle = normalJob.Schedule();
            handle.Complete();

            mesh.SetNormals(outputData);

            if (recalculateTangents)
            {
                mesh.SetTangents(outputTangents);
            }

            outputData.Dispose();
            outputTangents.Dispose();
            dataArray.Dispose();
        }

        public static void RecalculateNormals(Mesh mesh, float angle, ref Vector3[] normalOut, ref Vector4[] tangentOut)
        {
            var dataArray = Mesh.AcquireReadOnlyMeshData(mesh);
            var data = dataArray[0];
            var outputNormals = new NativeArray<float3>(data.vertexCount, Allocator.TempJob);
            var outputTangents = new NativeArray<float4>(data.vertexCount, Allocator.TempJob);


            var normalJob = new NormalJob
            {
                Data = data,
                Angle = angle,
                Normals = outputNormals,
                Tangents = outputTangents,
                RecalculateTangents = true
            };
            var handle = normalJob.Schedule();
            handle.Complete();

            //tangentOut = new Vector4[outputTangents.Length];
            outputTangents.Reinterpret<Vector4>().CopyTo(tangentOut);
            
            //normalOut = new Vector3[outputNormals.Length];
            outputNormals.Reinterpret<Vector3>().CopyTo(normalOut);
            
            outputNormals.Dispose();
            outputTangents.Dispose();
            dataArray.Dispose();
        }

        [BurstCompile]
        public struct NormalJob : IJob
        {
            [ReadOnly] public Mesh.MeshData Data;
            [ReadOnly] public float Angle;
            [ReadOnly] public bool RecalculateTangents;
            public NativeArray<float3> Normals;
            public NativeArray<float4> Tangents;


            public void Execute()
            {
                var vertexCount = Data.vertexCount;
                //int triangleCount = 0;
                float cosineThreshold = math.cos(Angle * 0.017453292f);

                var vertices = new NativeArray<float3>(vertexCount, Allocator.Temp);
                //var verticesAsVector = vertices.Reinterpret<Vector3>();
                Data.GetVertices(vertices.Reinterpret<Vector3>());

                // Holds the normal of each triangle in each sub mesh.
                var triangles = new NativeArray<NativeArray<int>>(Data.subMeshCount, Allocator.Temp);
                var triNormals = new NativeArray<NativeArray<float3>>(Data.subMeshCount, Allocator.Temp);
                var dictionary = new UnsafeHashMap<VertexKey, NativeList<VertexEntry>>(Data.vertexCount, Allocator.Temp);


                for (int subMeshIndex = 0; subMeshIndex < Data.subMeshCount; ++subMeshIndex)
                {
                    var subMeshIndexCount = Data.GetSubMesh(subMeshIndex).indexCount;
                    triangles[subMeshIndex] = new NativeArray<int>(subMeshIndexCount, Allocator.Temp);
                    Data.GetIndices(triangles[subMeshIndex], subMeshIndex);
                    //var subTriangles = new NativeArray<int>((int)data.GetSubMesh(subMeshIndex).indexCount, Allocator.Temp);
                    //triangleCount += subMeshIndexCount;

                    triNormals[subMeshIndex] = new NativeArray<float3>(subMeshIndexCount / 3, Allocator.Temp);

                    for (int i = 0; i < subMeshIndexCount; i += 3)
                    {
                        int i1 = triangles[subMeshIndex][i];
                        int i2 = triangles[subMeshIndex][i + 1];
                        int i3 = triangles[subMeshIndex][i + 2];

                        int triIndex = i / 3;

                        // Calculate the normal of the triangle
                        float3 p1 = vertices[i2] - vertices[i1];
                        float3 p2 = vertices[i3] - vertices[i1];
                        float3 normal = math.cross(p1, p2);
                        float magnitude = math.length(normal);
                        if (magnitude > 0)
                        {
                            normal /= magnitude;
                        }

                        var array = triNormals[subMeshIndex];
                        array[triIndex] = normal;

                        VertexKey key;
                        NativeList<VertexEntry> entry;

                        if (!dictionary.TryGetValue(key = new VertexKey(vertices[i1]), out entry))
                        {
                            entry = new NativeList<VertexEntry>(3, Allocator.Temp);
                            dictionary.Add(key, entry);
                            //dictionary[key].Add((new VertexEntry(subMeshIndex, triIndex, i1)));
                        }

                        entry.Add(new VertexEntry(subMeshIndex, triIndex, i1));

                        if (!dictionary.TryGetValue(key = new VertexKey(vertices[i2]), out entry))
                        {
                            entry = new NativeList<VertexEntry>(3, Allocator.Temp);
                            dictionary.Add(key, entry);
                        }

                        entry.Add((new VertexEntry(subMeshIndex, triIndex, i2)));

                        if (!dictionary.TryGetValue(key = new VertexKey(vertices[i3]), out entry))
                        {
                            entry = new NativeList<VertexEntry>(3, Allocator.Temp);
                            dictionary.Add(key, entry);
                        }

                        entry.Add((new VertexEntry(subMeshIndex, triIndex, i3)));
                    }
                }

                // Each entry in the dictionary represents a unique vertex position.
                foreach (var kvp in dictionary)
                {
                    var vertList = kvp.Value;
                    var listCount = vertList.Length;

                    for (int i = 0; i < listCount; ++i)
                    {
                        var sum = new float3();
                        VertexEntry lhsEntry = vertList[i];

                        for (int j = 0; j < listCount; ++j)
                        {
                            VertexEntry rhsEntry = vertList[j];

                            if (lhsEntry.VertexIndex == rhsEntry.VertexIndex)
                            {
                                sum += triNormals[rhsEntry.MeshIndex][rhsEntry.TriangleIndex];
                            }
                            else
                            {
                                // The dot product is the cosine of the angle between the two triangles.
                                // A larger cosine means a smaller angle.
                                float dot = math.dot(
                                    triNormals[lhsEntry.MeshIndex][lhsEntry.TriangleIndex],
                                    triNormals[rhsEntry.MeshIndex][rhsEntry.TriangleIndex]);

                                if (dot >= cosineThreshold)
                                {
                                    sum += triNormals[rhsEntry.MeshIndex][rhsEntry.TriangleIndex];
                                }
                            }
                        }

                        Normals[lhsEntry.VertexIndex] = math.normalize(sum);
                    }
                }

                if (RecalculateTangents)
                {
                    // Recalculates mesh tangents
                    // For some reason the built-in RecalculateTangents function produces artifacts on dense geometries.
                    // This implementation id derived from:
                    // Lengyel, Eric. Computing Tangent Space Basis Vectors for an Arbitrary Mesh.
                    // Terathon Software 3D Graphics Library, 2001.
                    // http://www.terathon.com/code/tangent.html
                    var uv = new NativeArray<float2>(vertexCount, Allocator.Temp);
                    Data.GetUVs(0, uv.Reinterpret<Vector2>());

                    var tan1 = new NativeArray<float3>(vertexCount, Allocator.Temp);
                    var tan2 = new NativeArray<float3>(vertexCount, Allocator.Temp);

                    for (int subMeshIndex = 0; subMeshIndex < Data.subMeshCount; subMeshIndex++)
                    {
                        var count = triangles[subMeshIndex].Length;

                        for (int triIndex = 0; triIndex < count; triIndex += 3)
                        {
                            int i1 = triangles[subMeshIndex][triIndex + 0];
                            int i2 = triangles[subMeshIndex][triIndex + 1];
                            int i3 = triangles[subMeshIndex][triIndex + 2];

                            float3 v1 = vertices[i1];
                            float3 v2 = vertices[i2];
                            float3 v3 = vertices[i3];

                            float2 w1 = uv[i1];
                            float2 w2 = uv[i2];
                            float2 w3 = uv[i3];

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

                            tan1[i1] += sDir;
                            tan1[i2] += sDir;
                            tan1[i3] += sDir;

                            tan2[i1] += tDir;
                            tan2[i2] += tDir;
                            tan2[i3] += tDir;
                        }
                    }

                    for (int a = 0; a < vertexCount; ++a)
                    {
                        Vector3 n = Normals[a];
                        Vector3 t = tan1[a];

                        //TODO: Use math library and float3 here 
                        Vector3.OrthoNormalize(ref n, ref t);

                        var w = (math.dot(math.cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
                        Tangents[a] = new float4(t.x, t.y, t.z, w);
                    }

                    //tan1.Dispose();
                    //tan2.Dispose();
                    //uv.Dispose();
                }


                //Dispose  disposing temp allocations no-op anyway
                // vertices.Dispose();
                // foreach (var kvp in dictionary)
                // {
                //     kvp.Value.Dispose();
                // }

                // dictionary.Dispose();

                // foreach (var na in triNormals)
                // {
                //     na.Dispose();
                // }

                // foreach (var subTris in triangles)
                // {
                //     subTris.Dispose();
                // }

                // triangles.Dispose();

                // foreach (var subTriNormals in triNormals)
                // {
                //     subTriNormals.Dispose();
                // }

                // triNormals.Dispose();
            }
        }

        #region vertexMapStructs

        public struct VertexKey : IEquatable<VertexKey>
        {
            private readonly long _x;
            private readonly long _y;
            private readonly long _z;

            // Change this if you require a different precision.
            private const int Tolerance = 100000;

            // Magic FNV values. Do not change these.
            private const long FNV32Init = 0x811c9dc5;
            private const long FNV32Prime = 0x01000193;

            public VertexKey(float3 position)
            {
                _x = (long)(Mathf.Round(position.x * Tolerance));
                _y = (long)(Mathf.Round(position.y * Tolerance));
                _z = (long)(Mathf.Round(position.z * Tolerance));
            }


            public override int GetHashCode()
            {
                //return HashCode.Combine(_x, _y, _z);
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + _x.GetHashCode();
                    hash = hash * 31 + _y.GetHashCode();
                    hash = hash * 31 + _z.GetHashCode();
                    return hash;
                }
            }

            public bool Equals(VertexKey other)
            {
                return _x == other._x && _y == other._y && _z == other._z;
            }
        }

        public struct VertexEntry
        {
            public int MeshIndex;
            public int TriangleIndex;
            public int VertexIndex;

            public VertexEntry(int meshIndex, int triIndex, int vertIndex)
            {
                MeshIndex = meshIndex;
                TriangleIndex = triIndex;
                VertexIndex = vertIndex;
            }
        }

        #endregion
    }
}