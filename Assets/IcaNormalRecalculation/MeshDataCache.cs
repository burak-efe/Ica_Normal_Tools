using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace IcaNormal
{
    [PreferBinarySerialization]
    [CreateAssetMenu(menuName = "Plugins/IcaNormalRecalculation/MeshDataCache", fileName = "IcaMeshDataCache")]
    public class MeshDataCache : ScriptableObject
    {
        [Serializable]
        public struct DuplicateVerticesArray
        {
            public int[] Value;
        }

        public Mesh TargetMesh;
        [SerializeField, HideInInspector] public List<DuplicateVerticesArray> DuplicatesData;
        [SerializeField, HideInInspector] public int IndicesCount;
        [SerializeField, HideInInspector] public int[] AdjacencyList;
        [FormerlySerializedAs("AdjacencyMap")] [SerializeField, HideInInspector] public int2[] AdjacencyMapper;


#if UNITY_EDITOR
        public string LastCacheDate = "Never";
#endif
        
        [ContextMenu("CacheData")]
        public void CacheData()
        {
            var posGraph = MeshCacheUtils.GetVertexPosGraph(TargetMesh);
            DuplicatesData = MeshCacheUtils.GetDuplicateVerticesMap(TargetMesh, posGraph);
            MeshCacheUtils.CalculateAdjacencyData(TargetMesh, posGraph, out AdjacencyList, out AdjacencyMapper);
            IndicesCount = TargetMesh.triangles.Length;

#if UNITY_EDITOR
            LastCacheDate = System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToShortTimeString();
#endif
        }
    }
}