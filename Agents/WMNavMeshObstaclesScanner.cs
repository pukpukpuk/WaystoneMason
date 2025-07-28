using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WaystoneMason.Utils;

namespace WaystoneMason.Agents
{
    public class WMNavMeshObstaclesScanner : MonoBehaviour
    {
        [HideInInspector] public ObstaclesScanType ScanType;
        [HideInInspector] public float ScanRadius;
        
        public WMNavMeshHolder PreferredHolder;

        private WMNavMeshHolder _holder;
        
        private readonly HashSet<WMDynamicObstacle> _obstaclesOnNavMesh = new();
        
        public Vector2 Center => transform.position;
        
        private void OnValidate()
        {
            PreferredHolder ??= GetComponent<WMNavMeshHolder>() ?? GetComponent<WMAgent>()?.Holder;
        }
        
        private void OnEnable()
        {
            _holder ??= PreferredHolder;
            _holder.OnBeforeRebuild += HandleOnBeforeRebuild;
        }
        
        private void OnDisable()
        {
            _holder.OnBeforeRebuild -= HandleOnBeforeRebuild;
        }

        private void HandleOnBeforeRebuild()
        {
            if (ScanType == ObstaclesScanType.Disabled) return;
            
            var obstaclesHolder = WMObstaclesHolder.Instance;
            var obstacles = ScanType == ObstaclesScanType.InfiniteRadius 
                ? obstaclesHolder.GetObstacles() 
                : obstaclesHolder.GetObstacles(Center, ScanRadius);

            var navMesh = _holder.NavMesh;
            foreach (var obstacle in obstacles)
            {
                obstacle.Affect(navMesh);
                _obstaclesOnNavMesh.Add(obstacle);
            }

            foreach (var obstacle in _obstaclesOnNavMesh.ToList())
            {
                var actualState = obstacle.IsActualOn(navMesh, out var currentContour, out var bounds);

                if (obstacle && actualState) continue;
                if (!WMObstaclesHolder.IsInRadius(currentContour, bounds, Center, ScanRadius)) continue;
                
                navMesh.RemoveObstacle(currentContour);
                _obstaclesOnNavMesh.Remove(obstacle);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            GizmosUtils.DrawCircle(Center, ScanRadius, GizmosUtils.Yellow, .005f, .25f);
        }
    }
}