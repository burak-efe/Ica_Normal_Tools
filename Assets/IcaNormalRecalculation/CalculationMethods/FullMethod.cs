
using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace IcaNormal
{
    public static class FullMethod
    {
        public static void RecalculateNormals(this Mesh mesh, float angle, bool recalculateTangents = true)
        {
            var dataArray = Mesh.AcquireReadOnlyMeshData(mesh);
            var data = dataArray[0];
            var outputData = new NativeArray<float3>(data.vertexCount, Allocator.TempJob);
            var outputTangents = new NativeArray<float4>(data.vertexCount, Allocator.TempJob);
            
            var normalJob = new FullNormalJob
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

        public static void CalculateNormalData(Mesh mesh, float angle, ref Vector3[] normalOut, ref Vector4[] tangentOut)
        {
            var dataArray = Mesh.AcquireReadOnlyMeshData(mesh);
            var data = dataArray[0];
            var outputNormals = new NativeArray<float3>(data.vertexCount, Allocator.TempJob);
            var outputTangents = new NativeArray<float4>(data.vertexCount, Allocator.TempJob);


            var normalJob = new FullNormalJob
            {
                Data = data,
                Angle = angle,
                Normals = outputNormals,
                Tangents = outputTangents,
                RecalculateTangents = true
            };
            var handle = normalJob.Schedule();
            handle.Complete();
            
            outputTangents.Reinterpret<Vector4>().CopyTo(tangentOut);
            outputNormals.Reinterpret<Vector3>().CopyTo(normalOut);
            
            outputNormals.Dispose();
            outputTangents.Dispose();
            dataArray.Dispose();
        }


    }
}