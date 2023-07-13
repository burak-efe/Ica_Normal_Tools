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
        var mda = Mesh.AcquireReadOnlyMeshData(Meshes);

        var vList = new UnsafeList<NativeArray<float3>>(2, Allocator.Temp);
        for (int i = 0; i < mda.Length; i++)
        {
            var v = new NativeArray<float3>(mda[i].vertexCount, Allocator.Temp);
            mda[i].GetVertices(v.Reinterpret<Vector3>());
            vList.Add(v);
        }

        UnRoller(vList, out var vMap, out var vMerged, Allocator.TempJob);
        
        var nMerged = new NativeArray<float3>(vMerged.Length, Allocator.TempJob);
        
        var iList = new UnsafeList<NativeArray<int>>(2, Allocator.Temp);
        for (int meshIndex = 0; meshIndex < mda.Length; meshIndex++)
        {
            mda[meshIndex].GetAllIndices(out var indices, Allocator.Temp);
            for (int index = 0; index < indices.Length; index++)
            {
                Debug.Log("added to every index " + vMap[meshIndex]);
                indices[index] += vMap[meshIndex];
            }
            iList.Add(indices);
        }

        UnRoller(iList, out var iMap, out var iMerged, Allocator.TempJob);

        CachedParallelMethod.CalculateNormalDataUncached(vMerged.AsArray(), iMerged.AsArray(), ref nMerged);

        
        // Apply
        for (int i = 0; i < mda.Length; i++)
        {
            var sub = nMerged.GetSubArray(vMap[i], vMap[i + 1] - vMap[i]);

            Meshes[i].SetNormals(sub);
        }


        vMap.Dispose();
        iMap.Dispose();
        vMerged.Dispose();
        nMerged.Dispose();
        iMerged.Dispose();
        mda.Dispose();
    }


    [BurstCompile]
    private void UnRoller<T>(UnsafeList<NativeArray<T>> nestedData, out NativeArray<int> outMapper, out NativeList<T> outUnrolledData, Allocator allocator) where T : unmanaged
    {
        var size = 0;
        for (int i = 0; i < nestedData.Length; i++)
        {
            size += nestedData[i].Length;
        }

        outUnrolledData = new NativeList<T>(size, allocator);
        outMapper = new NativeArray<int>(nestedData.Length + 1, allocator);
        var mapperIndex = 0;

        for (int i = 0; i < nestedData.Length; i++)
        {
            outUnrolledData.AddRange(nestedData[i]);
            outMapper[i] = mapperIndex;
            mapperIndex += nestedData[i].Length;
        }

        outMapper[^1] = mapperIndex;
    }
}