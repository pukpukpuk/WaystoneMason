#region

using UnityEngine;
using Vector2 = UnityEngine.Vector2;

#endregion

namespace WaystoneMason.Utils
{
    public static class MatrixUtils
    {
        public static float GetSquaredMagnitude(Vector2 matrix, Vector2 vector)
        {
            var x = vector.x * matrix.x;
            var y = vector.y * matrix.y;
            return x*x + y*y;
        }
        
        public static float GetDistanceSquared(Vector2 matrix, Vector2 a, Vector2 b)
        {
            var vector = b - a;
            return GetSquaredMagnitude(matrix, vector);
        }

        public static float GetDistance(Vector2 matrix, Vector2 a, Vector2 b)
        {
            return Mathf.Sqrt(GetDistanceSquared(matrix, a, b));
        }
    }
}