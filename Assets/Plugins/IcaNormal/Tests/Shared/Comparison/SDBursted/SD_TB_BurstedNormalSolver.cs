using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Ica.Normal
{
    public static class SD_TB_BurstedNormalSolver
    {
        public static void RecalculateNormals(this Mesh mesh, float angle, bool recalculateTangents = false)
        {
            var dataArray = Mesh.AcquireReadOnlyMeshData(mesh);
            var data = dataArray[0];
            var outNormals = new NativeArray<float3>(data.vertexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var outTangents = new NativeArray<float4>(data.vertexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var normalJob = new SDBurstedJob
            {
                Data = data,
                Angle = angle,
                Normals = outNormals,
                Tangents = outTangents,
                RecalculateTangents = recalculateTangents
            };
            var handle = normalJob.Schedule();
            handle.Complete();

            mesh.SetNormals(outNormals);

            if (recalculateTangents)
            {
                mesh.SetTangents(outTangents);
            }

            outNormals.Dispose();
            outTangents.Dispose();
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