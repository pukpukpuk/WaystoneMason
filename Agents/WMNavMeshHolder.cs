#region

using System;
using UnityEngine;
using WaystoneMason.Pathfinding.Core;
using WaystoneMason.Utils;
using Vector2 = UnityEngine.Vector2;

#endregion

namespace WaystoneMason.Agents
{
    public class WMNavMeshHolder : MonoBehaviour
    {
        public float AgentRadius = .25f;
        public float RebuildCallPeriod = .5f;

        public bool IsIsometric;
        public float IsometryYScale = 0.5f;
        
        private float _nextRebuildAt;

        public event Action OnBeforeRebuild;
        public event Action OnAfterRebuildWithAnyChanges;
        
        public NavMesh NavMesh { get; private set; }

        public Vector2 GetOrCreateMatrix()
        {
            return NavMesh?.FromScreenScaleMatrix ?? CreateMatrix();
        }
        
        private void Awake()
        {
            NavMesh = new NavMesh(AgentRadius, CreateMatrix());
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
            var anyChange = NavMesh.Rebuild();
            if (anyChange) OnAfterRebuildWithAnyChanges?.Invoke();
        }
        
        private void OnDrawGizmosSelected()
        {
            NavMesh?.DrawGizmos();
        }

        private void CreateEmptyChunks()
        {
            var rect = WMObstaclesHolder.Instance.PregeneratedEmptyChunksRegion;

            foreach (var chunkPosition in GeometryUtils.GetChunksInRect(rect.min, rect.max))
            {
                NavMesh.GetOrCreateChunk(chunkPosition);
            }
            
            NavMesh.Rebuild();
        }

        private Vector2 CreateMatrix()
        {
            return new Vector2(1, IsIsometric ? 1 / IsometryYScale : 1);
        }
    }
}