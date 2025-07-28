using System;
using System.Collections.Generic;
using System.Linq;
using Clipper2Lib;
using UnityEngine;
using WaystoneMason.Pathfinding;
using WaystoneMason.Pathfinding.Core;

namespace WaystoneMason.Agents
{
    public class WMDynamicObstacle : MonoBehaviour
    {
        public Collider2D Collider;
        
        private Vector2 _cachedPosition;
        private int _cachedColliderHash;
        
        private int _lastChangeId;
        private readonly Dictionary<NavMesh, (int id, PathD contour, Rect bounds)> _affectData = new();
        
        public PathD CurrentContour { get; private set; }
        public Rect Bounds { get; private set; }
        
        public void Affect(NavMesh navMesh)
        {
            TryConstructContour();

            if (IsActualOn(navMesh, out var contourOnNavMesh, out _)) return;

            if (contourOnNavMesh != null && navMesh.ContainsObstacle(contourOnNavMesh))
            {
                navMesh.RemoveObstacle(contourOnNavMesh);
            }
            navMesh.AddObstacle(CurrentContour);
            
            _affectData[navMesh] = (_lastChangeId, CurrentContour, Bounds);
        }

        public bool IsActualOn(NavMesh navMesh, out PathD currentContour, out Rect bounds)
        {
            var contains = _affectData.TryGetValue(navMesh, out var affectData);
            currentContour = affectData.contour;
            bounds = affectData.bounds;
            
            return contains && affectData.id >= _lastChangeId;
        }
        
        private void OnEnable()
        {
            TryConstructContour();
            WMObstaclesHolder.Instance.AddObstacle(this);
        }

        private void OnDisable()
        {
            WMObstaclesHolder.Instance.RemoveObstacle(this);
        }

        private void FixedUpdate()
        {
            var currentPosition = (Vector2)transform.position;
            if (currentPosition.Approximately(_cachedPosition)) return;
            
            var delta = currentPosition - _cachedPosition;
            CurrentContour = Clipper.TranslatePath(CurrentContour, delta.x, delta.y);
            
            UpdateBounds();
            OnSomeChange();
            
            WMObstaclesHolder.Instance.RemoveObstacle(this);
            WMObstaclesHolder.Instance.AddObstacle(this);
        }

        private void TryConstructContour()
        {
            var currentHash = GetCurrentColliderHash();
            if (CurrentContour != null && currentHash == _cachedColliderHash) return;
            _cachedColliderHash = currentHash;

            CurrentContour = Collider switch
            {
                PolygonCollider2D polygonCollider => GetPolygonColliderContour(polygonCollider),
                CircleCollider2D circleCollider => GetCircleColliderContour(circleCollider),
                _ => throw new Exception($"Collider type {Collider.GetType().Name} is not supported!")
            };

            UpdateBounds();
            OnSomeChange();
        }

        private void OnSomeChange()
        {
            _lastChangeId++;
            _cachedPosition = transform.position;
        }

        private void UpdateBounds()
        {
            var clipperBounds = Clipper.GetBounds(CurrentContour);
            Bounds = Rect.MinMaxRect((float)clipperBounds.left,   (float)clipperBounds.top, 
                                    (float)clipperBounds.right, (float)clipperBounds.bottom);
        }
        
        private int GetCurrentColliderHash() => Collider.GetHashCode();

        #region Utils

        private static PathD GetPolygonColliderContour(PolygonCollider2D collider)
        {
            var origin = GetColliderOrigin(collider);
        
            var colliderPath = collider.GetPath(0);
            var enumerable = colliderPath.Select(point => new PointD(point.x + origin.x, point.y + origin.y));
            
            var path = new PathD(enumerable);

            return path;
        }
    
        private static PathD GetCircleColliderContour(CircleCollider2D collider)
        {
            const float OneStepLength = .25f;

            var origin = GetColliderOrigin(collider);
            var center = new PointD(origin.x, origin.y);
            var radius = (double)collider.radius;
            var steps = Mathf.CeilToInt(collider.radius * Mathf.PI / OneStepLength);
        
            return Clipper.Ellipse(center, radius, radius, steps);
        }

        private static Vector2 GetColliderOrigin(Collider2D collider)
        {
            return (Vector2)collider.transform.position + collider.offset;
        }

        #endregion
    }
}