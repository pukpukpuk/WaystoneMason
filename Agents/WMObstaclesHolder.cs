using System;
using System.Collections.Generic;
using System.Numerics;
using Clipper2Lib;
using UnityEngine;
using WaystoneMason.Pathfinding.Core;
using WaystoneMason.Utils;
using Vector2 = UnityEngine.Vector2;

namespace WaystoneMason.Agents
{
    [ExecuteAlways]
    [DefaultExecutionOrder(-100)]
    public class WMObstaclesHolder : MonoBehaviour
    {
        public static WMObstaclesHolder Instance;

        public Rect PregeneratedEmptyChunksRegion;
        
        private readonly Dictionary<Vector2Int, HashSet<WMDynamicObstacle>> _chunks = new();
        private readonly Dictionary<WMDynamicObstacle, List<Vector2Int>> _obstacles = new();
        
        private void Awake()
        {
            if (!Application.isPlaying)
            {
                Instance = null;
                return;
            }
            
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }
        
        public void AddObstacle(WMDynamicObstacle obstacle)
        {
            if (_obstacles.ContainsKey(obstacle))
            {
                throw new ArgumentException("This obstacle is already included!");
            }
            
            var obstacleList = _obstacles[obstacle] = new List<Vector2Int>();
            
            foreach (var chunkPosition in GetChunksInRect(obstacle.Bounds.min, obstacle.Bounds.max))
            {
                if (!_chunks.TryGetValue(chunkPosition, out var set))
                {
                    _chunks[chunkPosition] = set = new HashSet<WMDynamicObstacle>();
                }

                set.Add(obstacle);
                obstacleList.Add(chunkPosition);
            }
        }
        
        public void RemoveObstacle(WMDynamicObstacle obstacle)
        {
            if (!_obstacles.Remove(obstacle, out var list))
            {
                throw new ArgumentException("This obstacle wasn't included anyway!");
            }

            foreach (var chunkPosition in list)
            {
                _chunks[chunkPosition].Remove(obstacle);
                if (_chunks[chunkPosition].Count == 0) _chunks.Remove(chunkPosition);
            }
        }
        
        public IEnumerable<WMDynamicObstacle> GetObstacles() => _obstacles.Keys;
        
        /// <summary>
        /// Returns all obstacles intersecting a circle with the given parameters
        /// </summary>
        /// <param name="center">Center of the circle</param>
        /// <param name="radius">Radius of the circle</param>
        /// <param name="matrix">Matrix for transforming from screen space to world space</param>
        public IEnumerable<WMDynamicObstacle> GetObstacles(Vector2 center, float radius, Matrix3x2 matrix)
        {
            var chunks = GetChunksInCircle(center, radius, matrix);
            foreach (var chunkPosition in chunks)
            {
                if (!_chunks.TryGetValue(chunkPosition, out var set)) continue;

                foreach (var obstacle in set)
                {
                    if (IsInRadius(obstacle.CurrentContour, obstacle.Bounds, center, radius, matrix))
                    {
                        yield return obstacle;
                    }
                }
            }
        }

        /// <summary>
        /// Checks for intersection between the contour and a circle with the given parameters
        /// </summary>
        /// <param name="contour">The contour</param>
        /// <param name="bounds">The bounds of the contour</param>
        /// <param name="center">Center of the circle</param>
        /// <param name="radius">Radius of the circle</param>
        /// <param name="matrix">Matrix for transforming from screen space to world space</param>
        public static bool IsInRadius(PathD contour, Rect bounds, Vector2 center, float radius, Matrix3x2 matrix)
        {
            if (!IsCircleIntersectsRect(center, radius, bounds.min, bounds.max, matrix)) return false;
            
            var circleCenter = new PointD(center.x, center.y);
            var isPointInPolygon = Clipper.PointInPolygon(circleCenter, contour);
            return isPointInPolygon is PointInPolygonResult.IsOn or PointInPolygonResult.IsInside || 
                   IsPathIntersectsCircumference(contour, center, radius, matrix);
        }
        
        #region Chunks Enumeration
        
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

        private static IEnumerable<Vector2Int> GetChunksInCircle(Vector2 center, float radius, Matrix3x2 matrix)
        {
            var multipliedVector = matrix.Multiply(Vector2.one * radius);
            
            var min = center - multipliedVector;
            var max = center + multipliedVector;

            foreach (var chunkPosition in GetChunksInRect(min, max))
            {
                var rectMin = chunkPosition * Chunk.Size;
                var rectMax = rectMin + Vector2.one * Chunk.Size;

                if (!IsCircleIntersectsRect(center, radius, rectMin, rectMax, matrix)) continue;
                yield return chunkPosition;
            }
        }

        #endregion

        #region Intersection

        /// Checks if circle (world) intersects rect (screen)
        private static bool IsCircleIntersectsRect(
            Vector2 circleCenter, float radius, 
            Vector2 rectMin, Vector2 rectMax, 
            Matrix3x2 matrix)
        {
            var dx = Mathf.Max(rectMin.x - circleCenter.x, 0, circleCenter.x - rectMax.x);
            var dy = Mathf.Max(rectMin.y - circleCenter.y, 0, circleCenter.y - rectMax.y);
            var vectorToNearestPoint = new Vector2(dx, dy);
            
            return matrix.GetSquaredMagnitude(vectorToNearestPoint) <= radius * radius;
        }

        /// Checks if any of segment of given path is intersecting with circumference
        private static bool IsPathIntersectsCircumference(PathD path, Vector2 center, float radius, Matrix3x2 matrix)
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

        private static bool IsSegmentIntersectsCircumference(Vector2 a, Vector2 b, Vector2 center, float radius, Matrix3x2 matrix)
        {
            var delta = b - a;
            var ac = center - a;

            var t = Mathf.Clamp01(Vector2.Dot(ac, delta) / delta.sqrMagnitude);
            var closest = a + t * delta;
            
            return matrix.GetDistanceSquared(closest, center) <= radius * radius;
        }

        #endregion
        
        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            GizmosUtils.DrawRect(PregeneratedEmptyChunksRegion, GizmosUtils.Green, 0.01f, .3f);
        }

        #endregion
    }
}