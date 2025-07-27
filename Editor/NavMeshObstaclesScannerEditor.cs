using System;
using UnityEditor;
using WaystoneMason.Agents;

namespace WaystoneMason.Editor
{
    [CustomEditor(typeof(WMNavMeshObstaclesScanner), true)]
    public class NavMeshObstaclesScannerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var scanner = (WMNavMeshObstaclesScanner)target;
            
            scanner.ScanType = (ObstaclesScanType)EditorGUILayout.EnumPopup("Scan Type", scanner.ScanType);

            if (scanner.ScanType == ObstaclesScanType.FiniteRadius)
            {
                var unclamped = EditorGUILayout.FloatField("Scan Radius", scanner.FiniteScanRadius);
                scanner.FiniteScanRadius = Math.Max(unclamped, 0);
            }
        }
    }
}