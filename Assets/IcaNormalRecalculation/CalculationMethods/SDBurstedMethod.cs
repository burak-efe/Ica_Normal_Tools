
using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace IcaNormal
{
    public static class SDBurstedMethod
    {
        public static void RecalculateNormals(this Mesh mesh, float angle, bool recalculateTangents = true)
        {
            var dataArray = Mesh.AcquireReadOnlyMeshData(mesh);
            var data = dataArray[0];
            var outputData = new NativeArray<float3>(data.vertexCount, Allocator.TempJob);
            var outputTangents = new NativeArray<float4>(data.vertexCount, Allocator.TempJob);
            
            var normalJob = new SDBurstedJob
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

        public static void CalculateNormalData(Mesh.MeshData meshData, float angle, ref NativeArray<float3> normalOut, ref NativeArray<float4> tangentOut)
        {
            
            var normalJob = new SDBurstedJob
            {
                Data = meshData,
                Angle = angle,
                Normals = normalOut,
                Tangents = tangentOut,
                RecalculateTangents = true
            };
            var handle = normalJob.Schedule();
            handle.Complete();

        }


    }
}