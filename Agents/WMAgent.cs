using System;
using System.Collections.Generic;
using UnityEngine;
using WaystoneMason.Utils;

namespace WaystoneMason.Agents
{
    public class WMAgent : MonoBehaviour
    {
        public float Speed = 3;
        
        public WMNavMeshHolder Holder;
        
        private int _targetedCornerIndex;
        private List<Vector2> _path;
    
        public void SetGoal(Vector2 goal)
        {
            Holder.NavMesh.TryComputePath(transform.position, goal, out _path);
            _targetedCornerIndex = 0;
        }
    
        protected virtual void Update()
        {
            MoveThroughPath();
        }

        private void OnValidate()
        {
            Holder ??= GetComponent<WMNavMeshHolder>();
        }

        private void MoveThroughPath()
        {
            if (_path == null) return;
        
            var currentCorner = (Vector3)_path[_targetedCornerIndex];

            var currentPosition = transform.position;
            var delta = currentCorner - currentPosition;
            var vector = delta.normalized;

            var moveMagnitude = Time.deltaTime * Speed;
            var nextPosition = currentPosition + vector * moveMagnitude;

            if (Mathf.Pow(moveMagnitude, 2) >= delta.sqrMagnitude)
            {
                nextPosition = currentCorner;
                _targetedCornerIndex++;
                if (_targetedCornerIndex >= _path.Count)
                {
                    _path = null;
                }
            } 

            transform.position = nextPosition;
        }
    
        #region Gizmos
    
        private void OnDrawGizmosSelected()
        {
            DrawPath();
        }

        private void DrawPath()
        {
            if (_path == null) return;

            var corners = new List<Vector3> { transform.position };
            for (int i = _targetedCornerIndex; i < _path.Count; i++) corners.Add(_path[i]);

            var lines = new Vector3[(corners.Count - 1) * 2];
            for (int i = 0; i < corners.Count; i++)
            {                   
                if (i > 0)                 lines[i * 2 - 1] = corners[i];
                if (i < corners.Count - 1) lines[i * 2] = corners[i];
            }
        
            Gizmos.color = GizmosUtils.Green.WithAlpha(.8f);     
            Gizmos.DrawLineList(new ReadOnlySpan<Vector3>(lines));
        }
        
        #endregion
    }
}