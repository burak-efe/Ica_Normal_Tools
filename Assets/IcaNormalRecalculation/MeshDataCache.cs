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
    public class MeshDataCache : ScriptableObject
    {
        public Mesh TargetMesh;
        [SerializeField, HideInInspector] public List<DuplicateVerticesList> SerializedDuplicatesData;
        [SerializeField, HideInInspector] public int IndicesCount;
        [SerializeField, HideInInspector] public int[] SerializedAdjacencyList;
        [SerializeField, HideInInspector] public int2[] SerializedAdjacencyMapper;
#if UNITY_EDITOR
        public string LastCacheDate = "Never";
#endif


        [ContextMenu("CacheData")]
        public void CacheData()
        {
            var mda = Mesh.AcquireReadOnlyMeshData(TargetMesh);
            var data = mda[0];

            var vertices = new NativeArray<float3>(data.vertexCount, Allocator.Temp);
            data.GetVertices(vertices.Reinterpret<Vector3>());


            Profiler.BeginSample("GetIndices");
            var indices = new NativeList<int>(Allocator.Temp);
            for (int i = 0; i < data.subMeshCount; i++)
            {
                var temp = new NativeArray<int>(data.GetSubMesh(i).indexCount, Allocator.Temp);
                data.GetIndices(temp, i);
                indices.AddRange(temp);
            }

            IndicesCount = indices.Length;
            Profiler.EndSample();


            Profiler.BeginSample("GetGraphs");

            var posGraph = new UnsafeHashMap<float3, NativeList<int>>();
            VertexPositionMapper.GetVertexPosGraph(ref vertices, ref posGraph, Allocator.Temp);

            var nativeVertMap = new UnsafeList<NativeArray<int>>();
            DuplicateVerticesMapper.GetDuplicateVerticesMap(ref posGraph,ref nativeVertMap,Allocator.Temp);
            Profiler.EndSample();


            Profiler.BeginSample("DuplicatesToManaged");
            SerializedDuplicatesData = NativeToManagedUtils.GetManagedDuplicateVerticesMap(nativeVertMap);
            Profiler.EndSample();


            Profiler.BeginSample("Adjacency");
            Profiler.BeginSample("Calculate");
            var adjacencyList = new NativeList<int>();
            var adjacencyMapper = new NativeArray<int2>();
            AdjacencyMapper.CalculateAdjacencyData(ref vertices, ref indices, ref posGraph, ref adjacencyList, ref adjacencyMapper, Allocator.Temp);
            Profiler.EndSample();
            
            adjacencyList.AsArray().CopyTo(SerializedAdjacencyList);
            adjacencyMapper.CopyTo(SerializedAdjacencyMapper);
            Profiler.EndSample();

            mda.Dispose();

#if UNITY_EDITOR
            LastCacheDate = System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToShortTimeString();
#endif
        }
    }
}