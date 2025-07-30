using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using WaystoneMason.Utils;
using Vector2 = UnityEngine.Vector2;

namespace WaystoneMason.Agents
{
    public class WMNavMeshObstaclesScanner : MonoBehaviour
    {
        [HideInInspector] public ObstaclesScanType ScanType;
        [HideInInspector] public float ScanRadius;
        
        public WMNavMeshHolder PreferredHolder;

        public event Action OnAfterRebuildWithAnyChanges;

        private readonly HashSet<WMDynamicObstacle> _obstaclesOnNavMesh = new();
        private bool _navMeshIsChanged;

        public Vector2 Center => transform.position;
        public WMNavMeshHolder Holder { get; private set; }

        private void OnValidate()
        {
            PreferredHolder ??= GetComponent<WMNavMeshHolder>();
        }
        
        private void OnEnable()
        {
            Holder ??= PreferredHolder;
            Holder.OnBeforeRebuild += HandleOnBeforeRebuild;
            Holder.OnAfterRebuild += HandleOnAfterRebuild;
        }
        
        private void OnDisable()
        {
            Holder.OnBeforeRebuild -= HandleOnBeforeRebuild;
        }

        private void HandleOnBeforeRebuild()
        {
            if (ScanType == ObstaclesScanType.Disabled) return;
            
            var matrix = Holder.NavMesh.FromScreenMatrix;
            
            var obstaclesHolder = WMObstaclesHolder.Instance;
            var obstacles = ScanType == ObstaclesScanType.InfiniteRadius 
                ? obstaclesHolder.GetObstacles() 
                : obstaclesHolder.GetObstacles(Center, ScanRadius, matrix);

            var anyChange = false;
            
            var navMesh = Holder.NavMesh;
            foreach (var obstacle in obstacles)
            {
                var changed = obstacle.Affect(navMesh);
                _obstaclesOnNavMesh.Add(obstacle);
                anyChange |= changed;
            }

            foreach (var obstacle in _obstaclesOnNavMesh.ToList())
            {
                var actualState = obstacle.IsActualOn(navMesh, out var currentContour, out var bounds);

                if (obstacle && actualState) continue;
                if (!WMObstaclesHolder.IsInRadius(currentContour, bounds, Center, ScanRadius, matrix)) continue;
                
                navMesh.RemoveObstacle(currentContour);
                _obstaclesOnNavMesh.Remove(obstacle);
                anyChange = true;
            }
            
            if (anyChange) _navMeshIsChanged = true;
        }

        private void HandleOnAfterRebuild()
        {
            if (!_navMeshIsChanged) return;
            _navMeshIsChanged = false;
            
            OnAfterRebuildWithAnyChanges?.Invoke();
        }
        
        private void OnDrawGizmosSelected()
        {
            var matrix = (Holder ?? PreferredHolder).GetOrCreateMatrix();
            Matrix3x2.Invert(matrix, out var inverted);
            GizmosUtils.DrawCircle(Center, ScanRadius, GizmosUtils.Yellow, .01f, .3f, inverted);
        }
    }
}