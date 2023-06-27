using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;

namespace IcaNormal
{
    [BurstCompile]
    public static class GetIndicesUtil
    {
        [BurstCompile]
        public static void GetIndices(in Mesh.MeshData data, out NativeList<int> outIndices, Allocator allocator)
        {
            var submeshCount = data.subMeshCount;
            var indexCount = 0;
            
            for (int i = 0; i < submeshCount; i++)
            {
                indexCount += data.GetSubMesh(i).indexCount;
            }
            
            outIndices = new NativeList<int>(indexCount,allocator);
            
            for (int i = 0; i < submeshCount; i++)
            {
                var temp = new NativeArray<int>(data.GetSubMesh(i).indexCount, Allocator.Temp);
                data.GetIndices(temp, i);
                outIndices.AddRange(temp);
            }

        }
    }
}