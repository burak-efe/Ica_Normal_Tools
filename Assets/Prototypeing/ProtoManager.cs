using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IcaNormal;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

[BurstCompile]
public class ProtoManager : MonoBehaviour
{
    public List<Mesh> Meshes;
    public SkinnedMeshRenderer smr;


    private void Start()
    {
        //NewMethod();
    }


    private void NewMethod()
    {

        // var vertexList = new UnsafeList<NativeArray<float3>>(2, Allocator.Temp);
        // for (int i = 0; i < mda.Length; i++)
        // {
        //     var v = new NativeArray<float3>(mda[i].vertexCount, Allocator.Temp);
        //     mda[i].GetVertices(v.Reinterpret<Vector3>());
        //     vertexList.Add(v);
        // }
        // NativeContainerUtils.UnRoller(vertexList, out var vMap, out var vMerged, Allocator.TempJob);
        //
        //
        // var indexList = new UnsafeList<NativeArray<int>>(2, Allocator.Temp);
        // for (int meshIndex = 0; meshIndex < mda.Length; meshIndex++)
        // {
        //     mda[meshIndex].GetAllIndices(out var indices, Allocator.Temp);
        //     for (int index = 0; index < indices.Length; index++)
        //     {
        //         Debug.Log("added to every index " + vMap[meshIndex]);
        //         indices[index] += vMap[meshIndex];
        //     }
        //     indexList.Add(indices);
        // }
        //
        // NativeContainerUtils.UnRoller(indexList, ref _indicesMap, ref  _indicesMerged, Allocator.TempJob);
        //
        //
        
        var mda = Mesh.AcquireReadOnlyMeshData(Meshes);

        NativeContainerUtils.CreateMergedVertices(mda, out var mergedVertices, out var vMap, Allocator.TempJob);
        NativeContainerUtils.CreateMergedIndices(mda, out var mergedIndices, out var iMap, Allocator.TempJob);
        var mergedNormals = new NativeList<float3>(mergedVertices.Length, Allocator.TempJob);
         CachedParallelMethod.CalculateNormalDataUncached(mergedVertices, mergedIndices, ref mergedNormals);

        // Apply
        for (int i = 0; i < mda.Length; i++)
        {
            var sub = mergedNormals.AsArray().GetSubArray(vMap[i], vMap[i + 1] - vMap[i]);

            Meshes[i].SetNormals(sub);
        }


        mergedNormals.Dispose();
        mergedVertices.Dispose();
        mergedIndices.Dispose();
        vMap.Dispose();
        iMap.Dispose();
        mda.Dispose();
    }
}