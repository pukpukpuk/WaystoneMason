#region

using UnityEditor;
using UnityEngine;
using WaystoneMason.Agents;

#endregion

namespace WaystoneMason.Editor
{
    [CustomEditor(typeof(WMNavMeshHolder), true)]
    public class WMNavMeshHolderEditor : UnityEditor.Editor
    {
        private SerializedProperty _isIsometricProperty;
        private SerializedProperty _isometryYScaleProperty;

        private void OnEnable()
        {
            _isIsometricProperty = serializedObject.FindProperty("IsIsometric");
            _isometryYScaleProperty = serializedObject.FindProperty("IsometryYScale");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawPropertiesExcluding(serializedObject, "IsIsometric", "IsometryYScale");
            
            EditorGUILayout.PropertyField(_isIsometricProperty, new GUIContent("Is Isometric"));
            
            if (_isIsometricProperty.boolValue)
            {
                var unclamped = EditorGUILayout.FloatField("Isometry Y Scale", _isometryYScaleProperty.floatValue);
                _isometryYScaleProperty.floatValue = Mathf.Clamp(unclamped, 0, 2);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}