using System;
using UnityEditor;
using WaystoneMason.Agents;

namespace WaystoneMason.Editor
{
    [CustomEditor(typeof(WMNavMeshHolder), true)]
    public class WMNavMeshHolderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var holder = (WMNavMeshHolder)target;

            holder.IsIsometric = EditorGUILayout.Toggle("Is Isometric", holder.IsIsometric);
            if (holder.IsIsometric)
            {
                var unclamped = EditorGUILayout.FloatField("Isometry Y Scale", holder.IsometryYScale);
                holder.IsometryYScale = Math.Clamp(unclamped, 0, 90);
            }
        }
    }
}