using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace WaystoneMason.Utils
{
    public static class MatrixUtils
    {
        /// Creates a matrix for converting screen positions to world
        public static Matrix3x2 CreateMatrixForIsometry(float visualDegrees) 
        {
            var radians = visualDegrees * Mathf.Deg2Rad;
            var sin = Mathf.Sin(radians);
            var cos = Mathf.Cos(radians);
            
            var matrix = new Matrix3x2
            {
                M11 = 1 / (2f * cos),
                M21 = 1 / (2f * sin),
                M12 = 1 / (-2f * cos),
                M22 = 1 / (2f * sin)
            };

            return matrix;
        }
        
        public static float GetSquaredMagnitude(this Matrix3x2 matrix, Vector2 vector)
        {
            var multiplied = matrix.Multiply(vector);
            return multiplied.sqrMagnitude;
        }
        
        public static float GetDistanceSquared(this Matrix3x2 matrix, Vector2 a, Vector2 b)
        {
            var vector = b - a;
            return matrix.GetSquaredMagnitude(vector);
        }

        public static float GetDistance(this Matrix3x2 matrix, Vector2 a, Vector2 b)
        {
            return Mathf.Sqrt(matrix.GetDistanceSquared(a, b));
        }
        
        public static Vector2 Multiply(this Matrix3x2 m, Vector2 v)
        {
            var x = m.M11 * v.x + m.M21 * v.y + m.M31;
            var y = m.M12 * v.x + m.M22 * v.y + m.M32;
            return new Vector2(x, y);
        }
    }
}