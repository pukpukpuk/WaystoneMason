#region

using System;
using System.Collections.Generic;
using UnityEngine;
using WaystoneMason.Utils;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

#endregion

namespace WaystoneMason.Agents
{
    public class WMAgent : MonoBehaviour
    {
        public float Speed = 3;
        
        public WMNavMeshHolder PreferredHolder;

        private WMNavMeshHolder _holder;
        private int _targetedCornerIndex;
        private List<Vector2> _path;
        
        public bool IsFollowingPath => _path != null;
        
        public void SetGoal(Vector2 goal)
        {
            _targetedCornerIndex = 0;
            if (!_holder.NavMesh.TryComputePath(transform.position, goal, out _path)) _path = null;
        }
    
        protected virtual void Update()
        {
            MoveThroughPath();
        }

        private void OnValidate()
        {
            _holder ??= GetComponent<WMNavMeshHolder>();
        }
        
        private void OnEnable()
        {
            _holder ??= PreferredHolder;
            _holder.OnAfterRebuildWithAnyChanges += HandleOnAfterRebuildWithAnyChanges;
        }
        
        private void OnDisable()
        {
            if (_holder) _holder.OnAfterRebuildWithAnyChanges -= HandleOnAfterRebuildWithAnyChanges;
        }

        #region Path Following

        private void MoveThroughPath()
        {
            if (!IsFollowingPath) return;

            var currentPosition = transform.position;
            var currentCorner = (Vector3)_path[_targetedCornerIndex];

            var matrix = _holder.NavMesh.FromScreenScaleMatrix;
            var delta = (currentCorner - currentPosition) * matrix;
            var vector = delta.normalized / matrix;
            
            var moveMagnitude = Time.deltaTime * Speed;
            var nextPosition = currentPosition + (Vector3)vector * moveMagnitude;

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
        
        private void HandleOnAfterRebuildWithAnyChanges()
        {
            if (!IsFollowingPath) return;
            
            SetGoal(_path[^1]);
        }
        
        #endregion
    
        #region Gizmos
    
        private void OnDrawGizmosSelected()
        {
            DrawPath();
        }

        private void DrawPath()
        {
            if (!IsFollowingPath) return;

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