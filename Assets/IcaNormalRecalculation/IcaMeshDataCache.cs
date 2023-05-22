using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace IcaNormal
{
    [CreateAssetMenu(menuName = "Plugins/IcaNormalRecalculation/MeshDataCache", fileName = "IcaMeshDataCache")]
    [PreferBinarySerialization]
    public class IcaMeshDataCache : ScriptableObject
    {
#if UNITY_EDITOR
        [ReadOnlyInspector] public string LastCacheDate = "Never";
#endif
        public Mesh TargetMesh;
        [SerializeField, HideInInspector] public List<DuplicateMap> DuplicatesData;

        [ContextMenu("CacheData")]
        public void CacheData()
        {
            DuplicatesData = GetDuplicateVerticesMap(TargetMesh);
#if UNITY_EDITOR
            LastCacheDate = System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToShortTimeString();
#endif
        }

        [Serializable]
        public struct DuplicateMap
        {
            public int[] DuplicateIndexes;
        }
        public static List<DuplicateMap> GetDuplicateVerticesMap(Mesh mesh)
        {
            var vertices = mesh.vertices;
            var tempMap = new Dictionary<Vector3, List<int>>(mesh.vertexCount);
            var map = new List<DuplicateMap>();

            for (int vertexIndex = 0; vertexIndex < mesh.vertexCount; vertexIndex++)
            {
                List<int> entryList;

                if (!tempMap.TryGetValue(vertices[vertexIndex], out entryList))
                {
                    entryList = new List<int>();
                    tempMap.Add(vertices[vertexIndex], entryList);
                }

                entryList.Add(vertexIndex);
            }

            foreach (var kvp in tempMap)
            {
                if (kvp.Value.Count > 1)
                {
                    map.Add(new DuplicateMap { DuplicateIndexes = kvp.Value.ToArray() });
                }
            }

            Debug.Log("Number of Duplicate Vertices Cached: " + map.Count);
            return map;
        }
    }

#if UNITY_EDITOR
    // taken from https://forum.unity.com/threads/read-only-fields.68976/#post-2729947
    [CustomPropertyDrawer(typeof(ReadOnlyInspectorAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
    public class ReadOnlyInspectorAttribute : PropertyAttribute { }
#endif
}