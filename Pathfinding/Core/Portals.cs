using System.Collections.Generic;
using UnityEngine;

namespace WaystoneMason.Pathfinding.Core
{
    public record Portals
    {
        public readonly List<Vector2> Left;
        public readonly List<Vector2> Right;

        public Portals(int capacity = 0)
        {
            Left = new List<Vector2>(capacity);
            Right = new List<Vector2>(capacity);
        }

        public void AddPortal(Vector2 point) => AddPortal(point, point);
        
        public void AddPortal(Vector2 left, Vector2 right)
        {
            Left.Add(left);
            Right.Add(right);
        }

        public void AddPortals(Portals portals)
        {
            Left.AddRange(portals.Left);
            Right.AddRange(portals.Right);
        }

        public void Deconstruct(out List<Vector2> left, out List<Vector2> right)
        {
            left = Left;
            right = Right;
        }
    }
}