
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
            var outputData = new NativeList<float3>(data.vertexCount, Allocator.TempJob);
            var outputTangents = new NativeList<float4>(data.vertexCount, Allocator.TempJob);
            
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

            mesh.SetNormals(outputData.AsArray());

            if (recalculateTangents)
            {
                mesh.SetTangents(outputTangents.AsArray());
            }
            
            outputData.Dispose();
            outputTangents.Dispose();
            dataArray.Dispose();
        }

        public static void CalculateNormalData(Mesh.MeshData meshData, float angle, ref NativeList<float3> normalOut, ref NativeList<float4> tangentOut)
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