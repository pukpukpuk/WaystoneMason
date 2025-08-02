using System.Collections.Generic;
using Clipper2Lib;
using UnityEngine;
using WaystoneMason.Pathfinding;
using WaystoneMason.Pathfinding.Core;
using HashCode = System.HashCode;

namespace WaystoneMason.Agents
{
    public class WMNavMeshDynamicObstacle : WMNavMeshObstacle
    {
        private Vector2 _cachedPosition;
        private int _cachedColliderHash;
        
        private int _lastChangeId;
        private readonly Dictionary<NavMesh, (int id, PathD contour, Rect bounds)> _affectData = new();
        
        /// <summary>
        /// Updates its state on the specified NavMesh
        /// </summary>
        /// <param name="navMesh">The NavMesh to update</param>
        public override void Affect(NavMesh navMesh)
        {
            TryUpdateContour();

            if (IsActualOn(navMesh, out var contourOnNavMesh, out _)) return;

            if (contourOnNavMesh != null && navMesh.ContainsObstacle(contourOnNavMesh))
            {
                navMesh.RemoveObstacle(contourOnNavMesh);
            }
            navMesh.AddObstacle(CurrentContour);
            
            _affectData[navMesh] = (_lastChangeId, CurrentContour, Bounds);
        }
        
        /// <summary>
        /// Returns the validity status for the specified NavMesh
        /// </summary>
        /// <param name="navMesh">The target NavMesh</param>
        /// <param name="currentContour">The current obstacle contour on the NavMesh</param>
        /// <param name="bounds">The bounds of the current contour on the NavMesh</param>
        /// <returns>True if the current obstacle position is valid</returns>
        public bool IsActualOn(NavMesh navMesh, out PathD currentContour, out Rect bounds)
        {
            var contains = _affectData.TryGetValue(navMesh, out var affectData);
            currentContour = affectData.contour;
            bounds = affectData.bounds;
            
            return contains && affectData.id >= _lastChangeId;
        }
        
        protected override void TryUpdateContour()
        {
            var currentHash = GetCurrentColliderHash();
            if (CurrentContour != null && currentHash == _cachedColliderHash) return;
            _cachedColliderHash = currentHash;
            
            SetNewContour(CreateContour());
            OnSomeChange();
        }
        
        private void OnSomeChange()
        {
            _lastChangeId++;
            _cachedPosition = transform.position;
        }
        
        private int GetCurrentColliderHash()
        {
            return HashCode.Combine(Collider.GetHashCode(), transform.lossyScale, Collider.offset);
        }
        
        private void FixedUpdate()
        {
            var currentPosition = (Vector2)transform.position;
            if (currentPosition.Approximately(_cachedPosition)) return;
            
            var delta = currentPosition - _cachedPosition;
            var translated = Clipper.TranslatePath(CurrentContour, delta.x, delta.y);
            SetNewContour(translated);
            OnSomeChange();
            
            WMObstaclesHolder.Instance.RemoveObstacle(this);
            WMObstaclesHolder.Instance.AddObstacle(this);
        }
    }
}