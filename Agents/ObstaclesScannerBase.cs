using System.Collections.Generic;
using System.Linq;
using Clipper2Lib;
using UnityEngine;
using WaystoneMason.Pathfinding.Core;
using WaystoneMason.Utils;

namespace WaystoneMason.Agents
{
    public abstract class ObstaclesScannerBase : MonoBehaviour
    {
        public WMNavMeshHolder PreferredHolder;
        
        private readonly HashSet<WMNavMeshDynamicObstacle> _obstaclesOnNavMesh = new();

        public WMNavMeshHolder Holder { get; private set; }
        
        #region Unity Events

        private void OnValidate()
        {
            PreferredHolder ??= GetComponent<WMNavMeshHolder>();
        }
        
        private void OnEnable()
        {
            Holder ??= PreferredHolder;
            Holder.OnBeforeRebuild += HandleOnBeforeRebuild;
        }
        
        private void OnDisable()
        {
            Holder.OnBeforeRebuild -= HandleOnBeforeRebuild;
        }

        #endregion

        #region Abstract Getters

        protected abstract IEnumerable<WMNavMeshObstacle> GetSeenObstacles();
        
        protected abstract bool IsVisible(PathD contour, Rect bounds);

        protected abstract Rect GetChunkPregenerationRect();
        
        #endregion

        private void HandleOnBeforeRebuild()
        {
            Scan();
            PregenerateChunks();
        }
        
        private void Scan()
        {
            var obstacles = GetSeenObstacles();
            
            var navMesh = Holder.NavMesh;
            foreach (var obstacle in obstacles)
            {
                obstacle.Affect(navMesh);
                if (obstacle is WMNavMeshDynamicObstacle dynamic) _obstaclesOnNavMesh.Add(dynamic);
            }
            
            foreach (var obstacle in _obstaclesOnNavMesh.ToList())
            {
                var actualState = obstacle.IsActualOn(navMesh, out var currentContour, out var bounds);

                if (obstacle && actualState) continue;
                if (!IsVisible(currentContour, bounds)) continue;
                
                navMesh.RemoveObstacle(currentContour);
                _obstaclesOnNavMesh.Remove(obstacle);
            }
            
        }

        private void PregenerateChunks()
        {
            var rect = GetChunkPregenerationRect();
            foreach (var chunkPosition in GeometryUtils.GetChunksInRect(rect.min, rect.max))
            {
                Holder.NavMesh.GetOrCreateChunk(chunkPosition);
            }
        }
    }
}