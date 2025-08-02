#region

using System;
using System.Collections.Generic;
using System.Linq;
using Clipper2Lib;
using UnityEngine;
using WaystoneMason.Utils;
using Vector2 = UnityEngine.Vector2;

#endregion

namespace WaystoneMason.Pathfinding.Core
{
    public class NavMesh
    {
        private readonly Dictionary<Vector2Int, Chunk> _chunks = new();

        private readonly Dictionary<PathD, PathsD> _cachedInflatedObstacles = new();
        private readonly HashSet<Chunk> _dirtyChunks = new();

        public float AgentRadius { get; }
        public Vector2 FromScreenScaleMatrix { get; }

        public NavMesh(float agentRadius) : this(agentRadius, Vector2.one)
        {
        }
        
        public NavMesh(float agentRadius, Vector2 fromScreenScaleMatrix)
        {
            AgentRadius = agentRadius;
            FromScreenScaleMatrix = fromScreenScaleMatrix;
        }
        
        public bool TryComputePath(Vector2 start, Vector2 goal, out List<Vector2> path)
        {
            path = new List<Vector2>();

            var startPolygon = FindPolygonContainingPoint(start);
            if (startPolygon == null)
            {
                startPolygon = FindNearestPolygon(start, AgentRadius / 2f, 
                    FromScreenScaleMatrix, out var closestPoint);
                if (startPolygon == null) return false;
                
                start = closestPoint;
            }
            
            var goalPolygon = FindPolygonContainingPoint(goal);
            if (goalPolygon == null) return false;

            var polygonPath = AStar.ComputePathBetweenPolygons(startPolygon, goalPolygon, start, goal, FromScreenScaleMatrix);
            if (polygonPath == null) return false;

            var portals = AStar.GetPortals(polygonPath, start, goal);

            path.AddRange(AStar.StringPull(portals));
            return true;
        }
        
        public void AddObstacle(PathD path, bool permanent = false)
        {
            if (_cachedInflatedObstacles.ContainsKey(path))
            {
                throw new ArgumentException("This obstacle is already included!");
            }

            var matrix = FromScreenScaleMatrix;
            var doScaling = !matrix.Approximately(Vector2.one);

            var normalized = !Clipper.IsPositive(path) ? Clipper.ReversePath(path) : path;
            var inflated = new PathsD { normalized };
            
            if (doScaling) inflated = ClipperUtils.Multiply(inflated, matrix);
            inflated = Clipper.InflatePaths(inflated, AgentRadius, JoinType.Miter, EndType.Polygon);
            if (doScaling) inflated = ClipperUtils.Divide(inflated, matrix);
                
            var bounds = Clipper.GetBounds(inflated);
            _cachedInflatedObstacles.Add(path, inflated);
            
            foreach (var chunk in GetObstacleChunks(bounds))
            {
                chunk.Add(inflated, permanent);
                _dirtyChunks.Add(chunk);
            }
        }

        public void RemoveObstacle(PathD path)
        {
            if (!_cachedInflatedObstacles.Remove(path, out var inflated))
            {
                throw new ArgumentException("This obstacle wasn't included anyway!");
            }

            var bounds = Clipper.GetBounds(inflated);
            foreach (var chunk in GetObstacleChunks(bounds))
            {
                chunk.Remove(inflated);
                _dirtyChunks.Add(chunk);
            }
        }

        public bool ContainsObstacle(PathD path)
        {
            return _cachedInflatedObstacles.ContainsKey(path);
        }

        public bool Rebuild()
        {
            var anyChange = _dirtyChunks.Count > 0;
            
            foreach (var chunk in _dirtyChunks) chunk.Rebuild();
            _dirtyChunks.Clear();

            return anyChange;
        }

        public bool TryGetChunk(Vector2Int chunkPosition, out Chunk chunk)
        {
            return _chunks.TryGetValue(chunkPosition, out chunk);
        }

        public Chunk GetOrCreateChunk(Vector2Int chunkPosition)
        {
            if (!_chunks.TryGetValue(chunkPosition, out var chunk))
            {
                chunk = _chunks[chunkPosition] = new Chunk(this, chunkPosition);
                _dirtyChunks.Add(chunk);
            }

            return chunk;
        }
        
        private IEnumerable<Chunk> GetObstacleChunks(RectD bounds)
        {
            var min = GetChunkPosition(new Vector2((float)bounds.left, (float)bounds.top));
            var max = GetChunkPosition(new Vector2((float)bounds.right, (float)bounds.bottom));

            for (int x = min.x; x <= max.x; x++)
            for (int y = min.y; y <= max.y; y++)
            {
                var chunkPosition = new Vector2Int(x, y);
                var chunk = GetOrCreateChunk(chunkPosition);

                yield return chunk;
            }
        }
        
        private static Vector2Int GetChunkPosition(Vector2 position)
        {
            return new Vector2Int(Mathf.FloorToInt(position.x / Chunk.Size), Mathf.FloorToInt(position.y / Chunk.Size));
        }

        #region Finding Polygon For Point

        private NavMeshPolygon FindPolygonContainingPoint(Vector2 point)
        {
            var chunkPosition = GetChunkPosition(point);
            if (!_chunks.TryGetValue(chunkPosition, out var chunk)) return null;
            
            return chunk.Polygons.FirstOrDefault(polygon => polygon.ContainsPoint(point));
        }

        private NavMeshPolygon FindNearestPolygon(Vector2 point, float radius, Vector2 matrix, out Vector2 closestPoint)
        {
            var foundPolygons = new List<NavMeshPolygon>();
            
            var offset = matrix * radius;
            foreach (var chunkPosition in GeometryUtils.GetChunksInRect(point - offset, point + offset))
            {
                if (!_chunks.TryGetValue(chunkPosition, out var chunk)) continue;

                foreach (var polygon in chunk.Polygons)
                {
                    if (!GeometryUtils.IsPathIntersectsCircumference(polygon.Vertices, point, radius, matrix)) continue;
                    foundPolygons.Add(polygon);
                }
            }

            closestPoint = Vector2.zero;
            if (foundPolygons.Count == 0) return null;

            NavMeshPolygon result = null;
            var minDistance = float.MaxValue;

            foreach (var polygon in foundPolygons)
            {
                var distance = GeometryUtils.GetDistanceToTriangleContour(point, polygon.Vertices, out var closest);
                if (distance >= minDistance) continue;
                
                minDistance = distance;
                result = polygon;
                closestPoint = closest;
            }

            return result;
        }

        #endregion
        
        #region Gizmos
        
        public void DrawGizmos()
        {
            foreach (var (_, chunk) in _chunks)
            {
                foreach (var polygon in chunk.Polygons)
                {
                    GizmosUtils.DrawPolygon(polygon.Vertices, GizmosUtils.Magenta, .22f, .8f);
                }
            }
        }
        
        #endregion
    }
}