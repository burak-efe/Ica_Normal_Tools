using System;
using System.Collections;
using System.Collections.Generic;
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
    public List<List<int>> nestedList = new List<List<int>>();

    private void Start()
    {
        var mda = Mesh.AcquireReadOnlyMeshData(Meshes);

        var vList = new UnsafeList<NativeArray<float3>>(2, Allocator.Persistent);
        for (int i = 0; i < mda.Length; i++)
        {
            var v = new NativeArray<float3>(mda[i].vertexCount, Allocator.Persistent);
            mda[i].GetVertices(v.Reinterpret<Vector3>());
            vList.Add(v);
        }

        UnRollerF3(vList, out var vMap, out var vMerged, Allocator.Persistent);

        var nMerged = new NativeArray<float3>(vMerged.Length, Allocator.Persistent);

        var iList = new UnsafeList<NativeArray<int>>(2, Allocator.Persistent);
        for (int i = 0; i < mda.Length; i++)
        {
            mda[i].GetAllIndices(out var indices, Allocator.Persistent);
            iList.Add(indices);
        }

        UnRoller(iList, out var iMap, out var iMerged, Allocator.Persistent);

        CachedParallelMethod.CalculateNormalDataUncached(vMerged.AsArray(), iMerged.AsArray(), ref nMerged);

        for (int i = 0; i < mda.Length; i++)
        {
            
        }
        
        
        
        
    }

    private void ApplyToMeshes()
    {
    }


    private void UnRollerF3(UnsafeList<NativeArray<float3>> nestedData, out NativeArray<int> outMapper, out NativeList<float3> outUnrolledData, Allocator allocator)
    {
        var size = 0;
        for (int i = 0; i < nestedData.Length; i++)
        {
            size += nestedData[i].Length;
        }

        outUnrolledData = new NativeList<float3>(size, allocator);
        outMapper = new NativeArray<int>(nestedList.Count + 1, allocator);
        var mapperIndex = 0;

        for (int i = 0; i < nestedList.Count; i++)
        {
            outUnrolledData.AddRange(nestedData[i]);
            outMapper[i] = mapperIndex;
            mapperIndex += nestedList[i].Count;
        }

        outMapper[^1] = mapperIndex;
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
        outMapper = new NativeArray<int>(nestedList.Count + 1, allocator);
        var mapperIndex = 0;

        for (int i = 0; i < nestedList.Count; i++)
        {
            outUnrolledData.AddRange(nestedData[i]);
            outMapper[i] = mapperIndex;
            mapperIndex += nestedList[i].Count;
        }

        outMapper[^1] = mapperIndex;
    }
}