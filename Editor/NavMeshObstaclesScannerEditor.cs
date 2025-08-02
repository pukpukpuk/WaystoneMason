#region

using UnityEditor;
using UnityEngine;
using WaystoneMason.Agents;

#endregion

namespace WaystoneMason.Editor
{
    [CustomEditor(typeof(WMNavMeshObstaclesScanner), true)]
    public class NavMeshObstaclesScannerEditor : UnityEditor.Editor
    {
        private SerializedProperty _scanTypeProperty;
        private SerializedProperty _scanRadiusProperty;

        private void OnEnable()
        {
            _scanTypeProperty = serializedObject.FindProperty("ScanType");
            _scanRadiusProperty = serializedObject.FindProperty("ScanRadius");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "ScanType", "ScanRadius");

            EditorGUILayout.PropertyField(_scanTypeProperty, new GUIContent("Scan Type"));

            if ((ObstaclesScanType)_scanTypeProperty.enumValueIndex == ObstaclesScanType.FiniteRadius)
            {
                var unclamped = EditorGUILayout.FloatField("Scan Radius", _scanRadiusProperty.floatValue);
                _scanRadiusProperty.floatValue = Mathf.Max(unclamped, 0);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}