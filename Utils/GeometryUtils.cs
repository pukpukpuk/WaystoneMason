using System.Collections.Generic;
using Clipper2Lib;
using UnityEngine;
using WaystoneMason.Pathfinding.Core;

namespace WaystoneMason.Utils
{
    internal static class GeometryUtils
    {
        #region Intersection
        
        /// Checks if any of segment of given path is intersecting with circumference
        public static bool IsPathIntersectsCircumference(Vector2[] path, Vector2 center, float radius, Vector2 matrix)
        {
            for (int i = 0; i < path.Length; i++)
            {
                var a = path[i];
                var b = path[(i + 1) % path.Length];
                
                if (IsSegmentIntersectsCircumference(a, b, center, radius, matrix)) return true;
            }

            return false;
        }
        
        /// Checks if any of segment of given path is intersecting with circumference
        public static bool IsPathIntersectsCircumference(PathD path, Vector2 center, float radius, Vector2 matrix)
        {
            for (int i = 0; i < path.Count; i++)
            {
                var a = path[i];
                var b = path[(i + 1) % path.Count];

                var aVector = new Vector2((float)a.x, (float)a.y);
                var bVector = new Vector2((float)b.x, (float)b.y);

                if (IsSegmentIntersectsCircumference(aVector, bVector, center, radius, matrix)) return true;
            }

            return false;
        }
        
        public static bool IsSegmentIntersectsCircumference(Vector2 a, Vector2 b, Vector2 center, float radius, Vector2 matrix)
        {
            var delta = b - a;
            var ac = center - a;

            var t = Mathf.Clamp01(Vector2.Dot(ac, delta) / delta.sqrMagnitude);
            var closest = a + t * delta;
            
            return MatrixUtils.GetDistanceSquared(matrix, closest, center) <= radius * radius;
        }
        
        #endregion
        
        /// <returns>Returns all chunks intersecting with the specified rectangle</returns>
        public static IEnumerable<Vector2Int> GetChunksInRect(Vector2 min, Vector2 max)
        {
            var intMin = Vector2Int.FloorToInt(min / Chunk.Size);
            var intMax = Vector2Int.FloorToInt(max / Chunk.Size);

            for (int x = intMin.x; x <= intMax.x; x++)
            {
                for (int y = intMin.y; y <= intMax.y; y++)
                {
                    yield return new Vector2Int(x, y);
                }
            }
        }
        
        #region Distance To Contour

        public static Vector2 GetClosestPointOnSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            var delta = b - a;
            var t = Vector2.Dot(point - a, delta) / delta.sqrMagnitude;
            t = Mathf.Clamp01(t);
            return a + t * delta;
        }

        public static float GetDistanceToTriangleContour(Vector2 point, Vector2[] contour, out Vector2 closestPoint)
        {
            closestPoint = Vector2.zero;
            
            var minSquared = float.MaxValue;
            for (int i = 0; i < contour.Length; i++)
            {
                var a = contour[i];
                var b = contour[(i + 1) % contour.Length];

                var segmentClosestPoint = GetClosestPointOnSegment(point, a, b);
                var distance = (point - segmentClosestPoint).sqrMagnitude;

                if (distance >= minSquared) continue;
                minSquared = distance;
                closestPoint = segmentClosestPoint;
            }
            
            return Mathf.Sqrt(minSquared);
        }
        

        
        #endregion
    }
}