using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class GBufferTeset : MonoBehaviour
{
    public Mesh TargetMesh;
    public Vector3[] Vertices;
    void Start()
    {
        var mesh = new Mesh();
        mesh.SetVertices(Vertices);
        
        var buffer = TargetMesh.GetVertexBuffer(0);
        Debug.Log("stride is " + buffer.stride + " count is " + buffer.count);
        var data = new float[buffer.stride * buffer.count / 4];
        buffer.GetData(data);
        var n = new NativeArray<float>(data, Allocator.Temp);

        NativeSlice<float> slice = n.Slice(3, 3);
        //var s = new NativeSlice<float>(1);
        foreach (var value in n.Reinterpret<float3>(4))
        {
            Debug.Log(value);
        }
        buffer.Dispose();
    }
}