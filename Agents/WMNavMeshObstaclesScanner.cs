#region

using System.Collections.Generic;
using Clipper2Lib;
using UnityEngine;
using WaystoneMason.Utils;
using Vector2 = UnityEngine.Vector2;

#endregion

namespace WaystoneMason.Agents
{
    public class WMNavMeshObstaclesScanner : ObstaclesScannerBase
    {
        public ObstaclesScanType ScanType;
        public float ScanRadius;

        private Vector2 Matrix => (Holder ?? PreferredHolder).GetOrCreateMatrix();
        private Vector2 Center => transform.position;

        protected override IEnumerable<WMNavMeshObstacle> GetSeenObstacles()
        {
            var obstacles = ScanType == ObstaclesScanType.InfiniteRadius 
                ? WMObstaclesHolder.Instance.GetObstacles() 
                : WMObstaclesHolder.Instance.GetObstacles(Center, ScanRadius, Matrix);
            return obstacles;
        }

        protected override bool IsVisible(PathD contour, Rect bounds)
        {
            return WMObstaclesHolder.IsInRadius(contour, bounds, Center, ScanRadius, Matrix);
        }

        protected override Rect GetChunkPregenerationRect()
        {
            var size = ScanRadius * 2 * Matrix;
            return new Rect(Center - size / 2f, size);
        }

        private void OnDrawGizmosSelected()
        {
            var matrix = Matrix;
            var inverted = new Vector2(1 / matrix.x, 1 / matrix.y);

            var vertices = GizmosUtils.GetCircleContour(Center, ScanRadius, inverted);
            GizmosUtils.DrawPolygon(vertices, GizmosUtils.Yellow, .04f, .5f);
        }
    }
}