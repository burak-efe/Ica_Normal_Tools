using UnityEditor;
using UnityEngine;

namespace IcaNormal

{
    public class Attributes
    {
        
    }
    
#if UNITY_EDITOR
    // taken from https://forum.unity.com/threads/read-only-fields.68976/#post-2729947
    [CustomPropertyDrawer(typeof(ReadOnlyInspectorAttribute))]
    public class ReadOnlyInspectorDrawer : PropertyDrawer
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