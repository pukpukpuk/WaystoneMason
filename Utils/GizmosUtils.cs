using System.Numerics;
using UnityEditor;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace WaystoneMason.Utils
{
    internal static class GizmosUtils
    {
        public static readonly Color Yellow = new(.8f, .8f, .2f);
        public static readonly Color Green = new(.3f, .7f, .3f);
        public static readonly Color Cyan = new(.2f, .8f, .8f);
        public static readonly Color Magenta = new(.7f, .3f, .7f);
        
        public static void DrawCircle(
            Vector2 center, float radius, 
            Color color, float fillAlpha, float borderAlpha, 
            Matrix3x2 matrix, int segments = 32)
        {
            var vertices = new Vector3[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float angle = i / (float)segments * Mathf.PI * 2f;
                var local = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                var world = center + matrix.Multiply(local);
                vertices[i] = world;
            }

            DrawVertices(vertices, color, fillAlpha, borderAlpha);
        }
        
        public static void DrawPolygon(Vector2[] vertices, Color color, float fillAlpha, float borderAlpha)
        {
            var convertedVertices = new Vector3[vertices.Length + 1];
            for (int i = 0; i < vertices.Length; i++) convertedVertices[i] = vertices[i];
            convertedVertices[^1] = convertedVertices[0];

            DrawVertices(convertedVertices, color, fillAlpha, borderAlpha);
        }

        public static void DrawVertices(Vector3[] vertices, Color color, float fillAlpha, float borderAlpha)
        {
#if UNITY_EDITOR
            Handles.color = color.WithAlpha(fillAlpha);
            Handles.DrawAAConvexPolygon(vertices);
            
            Handles.color = color.WithAlpha(borderAlpha);
            Handles.DrawAAPolyLine(vertices);
#endif
        }

        public static void DrawRect(Rect rect, Color color, float fillAlpha, float borderAlpha)
        {
#if UNITY_EDITOR
            var fillColor = color.WithAlpha(fillAlpha);
            var borderColor = color.WithAlpha(borderAlpha);
            
            Handles.DrawSolidRectangleWithOutline(rect, fillColor, borderColor);
#endif
        }
        
        public static Color WithAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}