using UnityEngine;

namespace WaystoneMason.PathFinding
{
    public static class VectorExtensions
    {
        public static bool Approximately(this Vector2 a, Vector2 b)
        {
            return (a - b).sqrMagnitude <= 0.001 * 0.001;
        }
    }
}