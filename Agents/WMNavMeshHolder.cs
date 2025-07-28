using System;
using UnityEngine;
using WaystoneMason.Pathfinding.Core;

namespace WaystoneMason.Agents
{
    public class WMNavMeshHolder : MonoBehaviour
    {
        public float AgentRadius = .25f;
        public float RebuildCallPeriod = .5f;
        
        private float _nextRebuildAt;

        public event Action OnBeforeRebuild;
        
        public NavMesh NavMesh { get; private set; }
        
        private void Awake()
        {
            NavMesh = new NavMesh(AgentRadius);
            CreateEmptyChunks();
        }

        private void Update()
        {
            if (!Mathf.Approximately(AgentRadius, NavMesh.AgentRadius))
            {
                Debug.LogWarning("The agent's radius can only be changed before the NavMesh initialization");
            }
            
            UpdateRebuild();
        }

        private void UpdateRebuild()
        {
            if (Time.time < _nextRebuildAt) return;
            _nextRebuildAt += Mathf.Max(RebuildCallPeriod, 0);
            
            OnBeforeRebuild?.Invoke();
            NavMesh.Rebuild();
        }
        
        private void OnDrawGizmosSelected()
        {
            NavMesh?.DrawGizmos();
        }

        private void CreateEmptyChunks()
        {
            var rect = WMObstaclesHolder.Instance.PregeneratedEmptyChunksRegion;

            foreach (var chunkPosition in WMObstaclesHolder.GetChunksInRect(rect.min, rect.max))
            {
                NavMesh.GetOrCreateChunk(chunkPosition);
            }
            
            NavMesh.Rebuild();
        }
    }
}