using System;
using System.Linq;
using Clipper2Lib;
using UnityEngine;
using WaystoneMason.PathFinding;
using WaystoneMason.PathFinding.Core;

namespace WaystoneMason.Agents
{
    public class WMDynamicObstacle : MonoBehaviour
    {
        public Collider2D Collider;
    
        private PathsD _currentContour;
        private Vector2 _cachedPosition;

        private int _cachedColliderHash;
    
        public void Affect(NavMesh navMesh)
        {
            TryCreateContour();
        
            var currentPosition = (Vector2)transform.position;
            if (_cachedPosition.Approximately(currentPosition) && navMesh.ContainsObstacle(_currentContour)) return;
        
            var delta = currentPosition - _cachedPosition;
        
            navMesh.RemoveObstacle(_currentContour);
            _currentContour = Clipper.TranslatePaths(_currentContour, delta.x, delta.y);
            navMesh.AddObstacle(_currentContour);
        
            _cachedPosition = currentPosition;
        }

        private void TryCreateContour()
        {
            var currentHash = GetCurrentColliderHash();
            if (_currentContour != null && currentHash == _cachedColliderHash) return;
            _cachedColliderHash = currentHash;

            _currentContour = Collider switch
            {
                PolygonCollider2D polygonCollider => GetPolygonColliderContour(polygonCollider),
                CircleCollider2D circleCollider => GetCircleColliderContour(circleCollider),
                _ => throw new Exception($"Collider type {Collider.GetType().Name} is not supported!")
            };
        
            _cachedPosition = transform.position;
        }

        private int GetCurrentColliderHash() => Collider.GetHashCode();

        private static PathsD GetPolygonColliderContour(PolygonCollider2D collider)
        {
            var pathsCount = collider.pathCount;
            var result = new PathsD(pathsCount);

            var origin = GetColliderOrigin(collider);
        
            for (int i = 0; i < pathsCount; i++)
            {
                var colliderPath = collider.GetPath(i);
                var enumerable = colliderPath.Select(point => new PointD(point.x + origin.x, point.y + origin.y));
            
                var path = new PathD(enumerable);
                result.Add(path);
            }

            return result;
        }
    
        private static PathsD GetCircleColliderContour(CircleCollider2D collider)
        {
            const float OneStepLength = .25f;

            var origin = GetColliderOrigin(collider);
            var center = new PointD(origin.x, origin.y);
            var radius = (double)collider.radius;
            var steps = Mathf.CeilToInt(collider.radius * Mathf.PI / OneStepLength);
        
            return new PathsD {Clipper.Ellipse(center, radius, radius, steps)};
        }

        private static Vector2 GetColliderOrigin(Collider2D collider)
        {
            return (Vector2)collider.transform.position + collider.offset;
        }
    }
}