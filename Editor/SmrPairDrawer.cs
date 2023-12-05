using UnityEditor;
using UnityEngine;

namespace Ica.Normal.Editor
{
    [CustomPropertyDrawer(typeof(SmrPair))]
    public class SmrPairDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Calculate the rect for the first GameObject field
            Rect firstRect = new Rect(position.x, position.y, position.width / 2 - 5, position.height);

            // Calculate the rect for the second GameObject field
            Rect secondRect = new Rect(position.x + position.width / 2 + 5, position.y, position.width / 2 - 5, position.height);

            // Get the serialized properties for the first and second GameObjects
            SerializedProperty first = property.FindPropertyRelative("SMR");
            SerializedProperty second = property.FindPropertyRelative("Prefab");

            // Draw the GameObject fields
            EditorGUI.PropertyField(firstRect, first, GUIContent.none);
            EditorGUI.PropertyField(secondRect, second, GUIContent.none);

            EditorGUI.EndProperty();
        }
    

    }
}