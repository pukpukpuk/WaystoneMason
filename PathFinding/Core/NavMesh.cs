using System;
using System.Collections.Generic;
using System.Linq;
using Clipper2Lib;
using UnityEngine;

namespace WaystoneMason.PathFinding.Core
{
    public class NavMesh
    {
        private readonly Dictionary<Vector2Int, Chunk> _chunks = new();

        private readonly Dictionary<PathsD, PathsD> _cachedInflatedObstacles = new();
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
        
        public void AddObstacle(PathsD paths, bool permanent = false)
        {
            if (_cachedInflatedObstacles.ContainsKey(paths))
            {
                throw new ArgumentException("This obstacle is already included!");
            }
            
            var inflated = Clipper.InflatePaths(paths, AgentRadius, JoinType.Miter, EndType.Polygon);
            var bounds = Clipper.GetBounds(inflated);
            
            _cachedInflatedObstacles.Add(paths, inflated);
            
            foreach (var chunk in GetObstacleChunks(bounds))
            {
                chunk.Add(inflated, permanent);
                _dirtyChunks.Add(chunk);
            }
        }

        public void RemoveObstacle(PathsD paths)
        {
            if (!_cachedInflatedObstacles.Remove(paths, out var inflated))
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

        public bool ContainsObstacle(PathsD paths)
        {
            return _cachedInflatedObstacles.ContainsKey(paths);
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
        
        private IEnumerable<Chunk> GetObstacleChunks(RectD bounds)
        {
            var min = GetChunkPosition(new Vector2((float)bounds.left, (float)bounds.top));
            var max = GetChunkPosition(new Vector2((float)bounds.right, (float)bounds.bottom));

            for (int x = min.x; x <= max.x; x++)
            for (int y = min.y; y <= max.y; y++)
            {
                var chunkPosition = new Vector2Int(x, y);
                if (!_chunks.TryGetValue(chunkPosition, out var set))
                {
                    set = _chunks[chunkPosition] = new Chunk(this, chunkPosition);
                }

                yield return set;
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
                
                Gizmos.color = new Color(.7f, .3f, .7f, .05f);
                Gizmos.DrawMesh(mesh);
                
                Gizmos.color = new Color(.7f, .3f, .7f, .65f);
                Gizmos.DrawWireMesh(mesh);
                
                foreach (var polygon in chunk.Polygons)
                {
                    if (!alreadyDrawn.Add(polygon)) continue;
                    
                    Gizmos.color = new Color(.2f, .8f, .8f);
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