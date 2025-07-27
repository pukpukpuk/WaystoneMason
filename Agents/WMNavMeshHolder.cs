using System;
using UnityEngine;
using WaystoneMason.PathFinding.Core;

namespace WaystoneMason.Agents
{
    public class WMNavMeshHolder : MonoBehaviour
    {
        public float AgentRadius = .25f;
        public float RebuildCallPeriod = 1f;
        
        private float _nextRebuildAt;

        public event Action OnBeforeRebuild;
        
        public NavMesh NavMesh { get; private set; }
        
        private void Awake()
        {
            NavMesh = new NavMesh(AgentRadius);
        }

        private void Update()
        {
            if (!Mathf.Approximately(AgentRadius, NavMesh.AgentRadius))
            {
                Debug.Log("The agent's radius can only be changed before the NavMesh initialization");
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
    }
}