using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;

namespace IcaNormal
{
    [PreferBinarySerialization]
    [CreateAssetMenu(menuName = "Plugins/IcaNormalRecalculation/MeshDataCache", fileName = "IcaMeshDataCache")]
    public class MeshDataCacheAsset : ScriptableObject
    {
        public Mesh TargetMesh;
        //[SerializeField, HideInInspector] public List<DuplicateVerticesList> SerializedDuplicatesData;
        [FormerlySerializedAs("IndicesCount")] [SerializeField, HideInInspector] public int[] SerializedIndices;
        [SerializeField, HideInInspector] public int[] SerializedAdjacencyList;
        [SerializeField, HideInInspector] public int2[] SerializedAdjacencyMapper;
#if UNITY_EDITOR
        public string LastCacheDate = "Never";
#endif


        [ContextMenu("CacheData")]
        public void CacheData()
        {
            Profiler.BeginSample("GetMDA");
            var mda = Mesh.AcquireReadOnlyMeshData(TargetMesh);
            var data = mda[0];
            Profiler.EndSample();
            
            Profiler.BeginSample("GetVertices");
            var vertices = new NativeList<float3>(data.vertexCount, Allocator.Temp);
            data.GetVertices(vertices.AsArray().Reinterpret<Vector3>());
            Profiler.EndSample();

            Profiler.BeginSample("GetIndices");
            data.GetAllIndicesWithNewNativeContainer(out var indices, Allocator.Temp);
            //Indices = indices.Length;
            Profiler.EndSample();


            Profiler.BeginSample("GetPosGraph");
            VertexPositionMapper.GetVertexPosHashMap( vertices.AsArray(), out var posMap, Allocator.Temp);
            Profiler.EndSample();

            Profiler.BeginSample("GetDuplicatesGraph");
            //DuplicateVerticesMapper.GetDuplicateVerticesMap(in posGraph, out var nativeVertMap, Allocator.Temp);
            Profiler.EndSample();


            Profiler.BeginSample("DuplicatesToManaged");
            //SerializedDuplicatesData = NativeToManagedUtils.GetManagedDuplicateVerticesMap(nativeVertMap);
            Profiler.EndSample();


            Profiler.BeginSample("Adjacency");
            Profiler.BeginSample("Calculate");
            AdjacencyMapper.CalculateAdjacencyData( vertices.AsArray(),  indices,  posMap, out var  adjacencyList, out var adjacencyMapper, Allocator.Temp);
            Profiler.EndSample();

            SerializedAdjacencyList = new int[adjacencyList.Length];
            SerializedAdjacencyMapper = new int2[adjacencyMapper.Length];
            SerializedIndices = new int[indices.Length];
            adjacencyList.AsArray().CopyTo(SerializedAdjacencyList);
            adjacencyMapper.CopyTo(SerializedAdjacencyMapper);
            indices.AsArray().CopyTo(SerializedIndices);
            Profiler.EndSample();

            mda.Dispose();

#if UNITY_EDITOR
            LastCacheDate = System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToShortTimeString();
#endif
        }
    }
}