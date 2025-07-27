using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WaystoneMason.PathFinding.Core
{
    public class NavMeshPolygon
    {
        private readonly Dictionary<NavMeshPolygon, NeighborData> _neighborData = new();
        
        public Vector2[] Vertices { get; }
        public Vector2 Centroid { get; }
        public IEnumerable<NavMeshPolygon> Neighbors => _neighborData.Keys;
        
        public NavMeshPolygon(Vector2[] vertices)
        {
            Vertices = vertices;
            Centroid = CalculateCentroid();
        }

        public void AddNeighbor(NavMeshPolygon other, (Vector2, Vector2) portal)
        {
            if (this == other) throw new ArgumentException("A polygon cannot be a neighbor of itself");
            if (IsNeighbor(other)) throw new ArgumentException("This polygon is already a neighbor");

            var data = new NeighborData(portal.Item1, portal.Item2);
            _neighborData.Add(other, data);
        }
        
        public void RemoveNeighbor(NavMeshPolygon other)
        {
            if (!IsNeighbor(other)) throw new ArgumentException("This polygon is not a neighbor");

            _neighborData.Remove(other);
        }
        
        public bool ContainsPoint(Vector2 point)
        {
            for (int i = 0; i < Vertices.Length; i++) {
                var a = Vertices[i];
                var b = Vertices[(i + 1) % Vertices.Length];
                var edge = b - a;
                var toPoint = point - a;
                if (edge.x * toPoint.y - edge.y * toPoint.x > 0) return false;
            }
            return true;
        }

        public NeighborData GetNeighborData(NavMeshPolygon other) => _neighborData[other];

        public bool IsNeighbor(NavMeshPolygon other) => _neighborData.Keys.Contains(other);
        
        private Vector2 CalculateCentroid()
        {
            var sum = Vector2.zero;
            foreach (var vertex in Vertices) sum += vertex;
            return sum / Vertices.Length;
        }
    }

    public struct NeighborData
    {
        public readonly Vector2 Portal1;
        public readonly Vector2 Portal2;

        public NeighborData(Vector2 portal1, Vector2 portal2)
        {
            Portal1 = portal1;
            Portal2 = portal2;
        }
    }
}