using Unity.Burst;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace IcaNormal
{
    [BurstCompile]
    public static class GetIndicesUtil
    {
        [BurstCompile]
        public static void GetAllIndices( this in Mesh.MeshData data, out NativeList<int> outIndices, Allocator allocator)
        {
            var submeshCount = data.subMeshCount;
            var indexCount = 0;
            
            for (int i = 0; i < submeshCount; i++)
            {
                indexCount += data.GetSubMesh(i).indexCount;
            }
            
            //var tempIndices= new NativeList<int>(indexCount,Allocator.Temp);
            outIndices = new NativeList<int>(indexCount,allocator);
            
            for (int subMeshIndex = 0; subMeshIndex < submeshCount; subMeshIndex++)
            {
                var tempSubmeshIndices = new NativeArray<int>(data.GetSubMesh(subMeshIndex).indexCount, Allocator.Temp);
                data.GetIndices(tempSubmeshIndices, subMeshIndex);
                //tempIndices.AddRange(tempSubmeshIndices);
                outIndices.AddRange(tempSubmeshIndices);
            }
            
            //outIndices = new NativeArray<int>(tempIndices.AsArray(),allocator);
        }
    }
}