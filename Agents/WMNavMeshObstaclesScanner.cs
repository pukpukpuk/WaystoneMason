using UnityEngine;

namespace WaystoneMason.Agents
{
    public class WMNavMeshObstaclesScanner : MonoBehaviour
    {
        [HideInInspector] public ObstaclesScanType ScanType;
        [HideInInspector] public float FiniteScanRadius;
        
        public WMNavMeshHolder Holder;
        
        private void OnValidate()
        {
            Holder ??= GetComponent<WMNavMeshHolder>() ?? GetComponent<WMAgent>()?.Holder;
        }

        private void OnEnable()
        {
            Holder.OnBeforeRebuild += HandleOnBeforeRebuild;
        }
        
        private void OnDisable()
        {
            Holder.OnBeforeRebuild -= HandleOnBeforeRebuild;
        }

        private void HandleOnBeforeRebuild()
        {
            if (ScanType == ObstaclesScanType.Disabled) return;
            
            // TODO синглтон в который все препятствия каждый раз при перемещении отчитываются
        }
        
        // TODO гизмосом показывай область скана 
    }
}