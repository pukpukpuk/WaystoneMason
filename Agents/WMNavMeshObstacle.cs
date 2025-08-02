#region

using System;
using System.Collections.Generic;
using Clipper2Lib;
using UnityEngine;
using WaystoneMason.Pathfinding.Core;

#endregion

namespace WaystoneMason.Agents
{
    public class WMNavMeshObstacle : MonoBehaviour
    {
        public Collider2D Collider;

        private readonly HashSet<NavMesh> _affectedNavMeshes = new();
        
        public PathD CurrentContour { get; private set; }
        public Rect Bounds { get; private set; }
        
        /// <summary>
        /// Adds self to specified NavMesh
        /// </summary>
        /// <param name="navMesh">The NavMesh to add</param>
        public virtual void Affect(NavMesh navMesh)
        {
            TryUpdateContour();
            if (_affectedNavMeshes.Contains(navMesh)) return;

            navMesh.AddObstacle(CurrentContour, true);
            _affectedNavMeshes.Add(navMesh);
        }
        
        /// Update contour if required 
        protected virtual void TryUpdateContour()
        {
            if (CurrentContour != null) return;

            SetNewContour(CreateContour());
        }
        
        protected PathD CreateContour()
        {
            return Collider switch 
            {
                PolygonCollider2D polygonCollider => GetPolygonColliderContour(polygonCollider),
                CircleCollider2D circleCollider => GetCircleColliderContour(circleCollider),
                _ => throw new Exception($"Collider type {Collider.GetType().Name} is not supported!")
            };
        }
        
        protected void SetNewContour(PathD contour)
        {
            CurrentContour = contour;
            UpdateBounds();
        }
        
        private void OnEnable()
        {
            TryUpdateContour();
            WMObstaclesHolder.Instance.AddObstacle(this);
        }

        private void OnDisable()
        {
            WMObstaclesHolder.Instance.RemoveObstacle(this);
        }
        
        private void UpdateBounds()
        {
            var clipperBounds = Clipper.GetBounds(CurrentContour);
            Bounds = Rect.MinMaxRect((float)clipperBounds.left,   (float)clipperBounds.top, 
                                    (float)clipperBounds.right, (float)clipperBounds.bottom);
        }
        
        #region Utils

        private static PathD GetPolygonColliderContour(PolygonCollider2D collider) 
        {
            var origin = GetColliderOrigin(collider);
            var scale = collider.transform.lossyScale;
            
            var colliderPath = collider.GetPath(0);
            var path = new PathD(colliderPath.Length);
            
            foreach (var point in colliderPath)
            {
                var scaled = point * scale;
                var pointD = new PointD(scaled.x + origin.x, scaled.y + origin.y);
                path.Add(pointD);
            }
            
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