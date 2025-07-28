using UnityEditor;
using UnityEngine;

namespace WaystoneMason.Utils
{
    internal static class GizmosUtils
    {
        public static readonly Color Yellow = new(.8f, .8f, .2f);
        public static readonly Color Green = new(.3f, .7f, .3f);
        public static readonly Color Cyan = new(.2f, .8f, .8f);
        public static readonly Color Magenta = new(.7f, .3f, .7f);

        public static void DrawMesh(Mesh mesh, Color color, float fillAlpha, float borderAlpha)
        {
            Gizmos.color = color.WithAlpha(fillAlpha);
            Gizmos.DrawMesh(mesh);
            
            Gizmos.color = color.WithAlpha(borderAlpha);
            Gizmos.DrawWireMesh(mesh);
        }

        public static void DrawCircle(Vector2 center, float radius, Color color, float fillAlpha, float borderAlpha)
        {
            var normal = Vector3.back;
            
            Handles.color = color.WithAlpha(fillAlpha);
            Handles.DrawSolidDisc(center, normal, radius);
            
            Handles.color = color.WithAlpha(borderAlpha);
            Handles.DrawWireDisc(center, normal, radius);
        }

        public static void DrawRect(Rect rect, Color color, float fillAlpha, float borderAlpha)
        {
            var fillColor = color.WithAlpha(fillAlpha);
            var borderColor = color.WithAlpha(borderAlpha);
            
            Handles.DrawSolidRectangleWithOutline(rect, fillColor, borderColor);
        }
        
        public static Color WithAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}