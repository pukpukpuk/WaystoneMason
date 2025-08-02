#region

using System;
using UnityEngine;

#endregion

namespace WaystoneMason.Pathfinding.Generation
{
    /// <summary>
    /// A structure for checking the collinearity of vectors
    /// </summary>
    public readonly struct Line : IEquatable<Line>
    {
        private readonly float A;
        private readonly float B;
        private readonly float C;

        public Line(Vector2 point1, Vector2 point2)
        {
            if (point1.y > point2.y || (point1.y == point2.y && point1.x > point2.x))
            {
                (point2, point1) = (point1, point2);
            }

            point2 = point1 + (point2 - point1).normalized;

            A = Normalize(point1.y - point2.y);
            B = Normalize(point2.x - point1.x);
            C = Normalize(-(A * point1.x + B * point1.y));
        }

        public bool Equals(Line other)
        {
            return A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C);
        }

        public override bool Equals(object obj)
        {
            return obj is Line other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(A, B, C);
        }

        private static float Normalize(float f)
        {
            return Mathf.Round(f * 1000) / 1000f;
        }

        public override string ToString()
        {
            return $"Line{{A: {A}, B: {B}, C: {C}}}";
        }
    }
}