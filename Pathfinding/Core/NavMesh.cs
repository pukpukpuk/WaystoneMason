using System;
using System.Collections.Generic;
using System.Linq;
using Clipper2Lib;
using UnityEngine;
using WaystoneMason.Utils;

namespace WaystoneMason.Pathfinding.Core
{
    public class NavMesh
    {
        private readonly Dictionary<Vector2Int, Chunk> _chunks = new();

        private readonly Dictionary<PathD, PathsD> _cachedInflatedObstacles = new();
        private readonly HashSet<Chunk> _dirtyChunks = new();

        public float AgentRadius { get; }

        public NavMesh(float agentRadius)
        {
            AgentRadius = agentRadius; 
        }
        
        public bool TryComputePath(Vector2 start, Vector2 goal, out List<Vector2> path)
        {
            path = null;

            var startPolygon = FindPolygonContainingPoint(start);
            var goalPolygon = FindPolygonContainingPoint(goal);
            if (startPolygon == null || goalPolygon == null) return false;

            var polygonPath = AStar.ComputePathBetweenPolygons(startPolygon, goalPolygon, start, goal);
            if (polygonPath == null) return false;

            var portals = AStar.GetPortals(polygonPath, start, goal);

            path = AStar.StringPull(portals);
            return true;
        }
        
        public void AddObstacle(PathD path, bool permanent = false)
        {
            if (_cachedInflatedObstacles.ContainsKey(path))
            {
                throw new ArgumentException("This obstacle is already included!");
            }
            
            var inflated = Clipper.InflatePaths(new PathsD {path}, AgentRadius, JoinType.Miter, EndType.Polygon);
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

        public void Rebuild()
        {
            foreach (var chunk in _dirtyChunks) chunk.Rebuild();
            _dirtyChunks.Clear();
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
        
        private NavMeshPolygon FindPolygonContainingPoint(Vector2 point)
        {
            var chunkPosition = GetChunkPosition(point);
            if (!_chunks.TryGetValue(chunkPosition, out var chunk)) return null;
            
            return chunk.Polygons.FirstOrDefault(polygon => polygon.ContainsPoint(point));
        }
        
        #region Gizmos

#if UNITY_EDITOR

        private readonly Dictionary<Vector2Int, Mesh> _cachedMeshes = new();
        
        public void DrawGizmos()
        {
            var alreadyDrawn = new HashSet<NavMeshPolygon>();
            
            foreach (var (chunkPosition, chunk) in _chunks)
            {
                var vertices = chunk.Polygons
                    .SelectMany(polygon => polygon.Vertices)
                    .Select(v => (Vector3)v)
                    .ToArray();
                
                var triangles = new int[vertices.Length];
                for (int i = 0; i < triangles.Length; i++) triangles[i] = i;

                if (!_cachedMeshes.TryGetValue(chunkPosition, out var mesh))
                {
                    _cachedMeshes[chunkPosition] = mesh = new Mesh();
                }
                
                mesh.Clear();
                mesh.SetVertices(vertices);
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateNormals();

                GizmosUtils.DrawMesh(mesh, GizmosUtils.Magenta, .05f, .65f);
                
                foreach (var polygon in chunk.Polygons)
                {
                    if (!alreadyDrawn.Add(polygon)) continue;
                    
                    Gizmos.color = GizmosUtils.Cyan;
                    foreach (var neighbor in polygon.Neighbors)
                    {
                        Gizmos.DrawLine(polygon.Centroid, neighbor.Centroid);
                    }
                }
            }
        }
#else
        public void DrawGizmos() { }
#endif
        
        #endregion
    }
}