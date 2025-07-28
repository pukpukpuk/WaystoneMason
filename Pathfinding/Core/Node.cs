using UnityEngine;

namespace WaystoneMason.Pathfinding.Core
{
    public class Node
    {
        public NavMeshPolygon Polygon {get; private set;}
        public Node Parent {get; set;}
        
        public float CumulativeDistance {get; set;}
        public float Heuristic {get; set;}

        public Vector2 Entrance {get; set;}
        
        public float DistancesSum => CumulativeDistance + Heuristic;

        public Node(NavMeshPolygon polygon, Node parent, float cumulativeDistance, float heuristic, Vector2 entrance)
        {
            Polygon = polygon;
            Parent = parent;
            
            CumulativeDistance = cumulativeDistance;
            Heuristic = heuristic;
            
            Entrance = entrance;
        }
    }
}