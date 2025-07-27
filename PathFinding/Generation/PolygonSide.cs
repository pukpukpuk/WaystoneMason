using UnityEngine;
using WaystoneMason.PathFinding.Core;

namespace WaystoneMason.PathFinding.Generation
{
    public record PolygonSide(Vector2 A, Vector2 B, NavMeshPolygon Polygon)
    {
        public void AddNeighbor(PolygonSide neighborSide, Vector2 direction, Vector2 point1, Vector2 point2)
        {
            var delta = B - A;
            var divided = !Mathf.Approximately(direction.x, 0)
                ? delta.x / direction.x
                : delta.y / direction.y;
            
            var isCodirectional = divided > 0;
            var portal = isCodirectional ? (point1, point2) : (point2, point1);
            Polygon.AddNeighbor(neighborSide.Polygon, portal);
        }
    }
}