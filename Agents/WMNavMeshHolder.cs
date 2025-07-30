using System;
using System.Numerics;
using UnityEngine;
using WaystoneMason.Pathfinding.Core;
using WaystoneMason.Utils;

namespace WaystoneMason.Agents
{
    public class WMNavMeshHolder : MonoBehaviour
    {
        public float AgentRadius = .25f;
        public float RebuildCallPeriod = .5f;

        [HideInInspector] public bool IsIsometric;
        [HideInInspector] public float IsometryAngle = 30f;
        
        private float _nextRebuildAt;

        public event Action OnBeforeRebuild;
        public event Action OnAfterRebuild;
        
        public NavMesh NavMesh { get; private set; }

        public Matrix3x2 GetOrCreateMatrix()
        {
            return NavMesh?.FromScreenMatrix ?? CreateMatrix();
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
            NavMesh.Rebuild();
            OnAfterRebuild?.Invoke();
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

        private Matrix3x2 CreateMatrix()
        {
            return IsIsometric 
                ? MatrixUtils.CreateMatrixForIsometry(IsometryAngle)
                : Matrix3x2.Identity;
        }
    }
}