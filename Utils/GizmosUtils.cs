#region

using System.Linq;
using UnityEditor;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

#endregion

namespace WaystoneMason.Utils
{
    public static class GizmosUtils
    {
        public static readonly Color Yellow = new(.8f, .8f, .2f);
        public static readonly Color Green = new(.3f, .7f, .3f);
        public static readonly Color Cyan = new(.2f, .8f, .8f);
        public static readonly Color Magenta = new(.7f, .3f, .7f);

        public static Vector2[] GetCircleContour(Vector2 center, float radius, Vector2 matrix, int segments = 32)
        {
            var vertices = new Vector2[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float angle = i / (float)segments * Mathf.PI * 2f;
                var local = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                var world = center + matrix * local;
                vertices[i] = world;
            }

            return vertices;
        }
        
        public static void DrawPolygon(Vector2[] vertices, Color color, float fillAlpha, float borderAlpha)
        {
#if UNITY_EDITOR
            Handles.color = color.WithAlpha(fillAlpha);
            Handles.DrawAAConvexPolygon(vertices.Select(v => (Vector3)v).ToArray());

            DrawPolyLine(vertices, color, borderAlpha);
#endif
        }
        
        public static void DrawPolyLine(Vector2[] vertices, Color color, float borderAlpha)
        {
#if UNITY_EDITOR
            var convertedVertices = new Vector3[vertices.Length + 1];
            for (int i = 0; i < vertices.Length; i++) convertedVertices[i] = vertices[i];
            convertedVertices[^1] = convertedVertices[0];
            
            Handles.color = color.WithAlpha(borderAlpha);
            Handles.DrawAAPolyLine(convertedVertices);
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