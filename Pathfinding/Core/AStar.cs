using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using WaystoneMason.Utils;
using Vector2 = UnityEngine.Vector2;

namespace WaystoneMason.Pathfinding.Core
{
    public static class AStar
    {
        private static readonly Comparer<Node> Comparer =
            Comparer<Node>.Create((a, b) => a.DistancesSum.CompareTo(b.DistancesSum));
        
        /// <summary>
        /// Builds a path between two polygons
        /// </summary>
        /// <param name="startPolygon">The starting polygon</param>
        /// <param name="goalPolygon">The target polygon</param>
        /// <param name="startPosition">The starting position</param>
        /// <param name="goalPosition">The target position</param>
        /// <param name="matrix">Matrix for transforming from screen space to world space</param>
        /// <exception cref="ArgumentNullException">Thrown if either polygon is null</exception>
        public static List<NavMeshPolygon> ComputePathBetweenPolygons(
            NavMeshPolygon startPolygon, 
            NavMeshPolygon goalPolygon, 
            Vector2 startPosition,
            Vector2 goalPosition,
            Matrix3x2 matrix)
        {
            if (startPolygon == null) throw new ArgumentNullException(nameof(startPolygon));
            if (goalPolygon == null) throw new ArgumentNullException(nameof(goalPolygon));

            var lookup = new Dictionary<NavMeshPolygon, Node>();
            
            var queue = new SortedSet<Node>(Comparer);
            var closed = new HashSet<NavMeshPolygon>();

            var startNode = new Node(startPolygon, null, 0, DistanceToGoal(startPosition), startPosition);
            queue.Add(startNode);
            
            while (queue.Count > 0)
            {
                var current = queue.Min;
                if (current.Polygon == goalPolygon) return ReconstructPath(current);

                queue.Remove(current);
                lookup.Remove(current.Polygon);
                closed.Add(current.Polygon);

                foreach (var neighbor in current.Polygon.Neighbors)
                {
                    if (closed.Contains(neighbor)) continue;
                    
                    var neighborData = current.Polygon.GetNeighborData(neighbor);
                    var portalStart = neighborData.Portal1;
                    var portalEnd = neighborData.Portal2;
                    
                    var portalNearestPoint = ClosestPointOnSegment(current.Entrance, portalStart, portalEnd);

                    var transitionDistance = matrix.GetDistance(current.Entrance, portalNearestPoint);
                    var cumulativeDistance = current.CumulativeDistance + transitionDistance;
                    
                    if (!lookup.TryGetValue(neighbor, out var existing))
                    {
                        var node = new Node(neighbor, current, cumulativeDistance, DistanceToGoal(portalNearestPoint), portalNearestPoint);
                        queue.Add(node);
                        lookup.Add(node.Polygon, node);
                    } 
                    else if (cumulativeDistance < existing.CumulativeDistance)
                    {
                        queue.Remove(existing);
                        
                        existing.CumulativeDistance = cumulativeDistance;
                        existing.Parent = current;
                        existing.Entrance = portalNearestPoint;
                        existing.Heuristic = DistanceToGoal(portalNearestPoint);
                        
                        queue.Add(existing);
                    }
                }
            }
            
            return null;

            float DistanceToGoal(Vector2 point)
            {
                return matrix.GetDistance(point, goalPosition);
            }
            
            static Vector2 ClosestPointOnSegment(Vector2 point, Vector2 a, Vector2 b)
            {
                var delta = b - a;
                var t = Vector2.Dot(point - a, delta) / delta.sqrMagnitude;
                t = Mathf.Clamp01(t);
                return a + t * delta;
            }
        }

        /// <summary>
        /// Constructs a set of portals from the given polygon path, including the start and end points of the route
        /// </summary>
        public static Portals GetPortals(List<NavMeshPolygon> polygonPath, Vector2 start, Vector2 goal)
        {
            var polygonPortals = GetPortals(polygonPath);
            
            var portals = new Portals(polygonPath.Count + 2);
            portals.AddPortal(start);
            portals.AddPortals(polygonPortals);
            portals.AddPortal(goal);

            return portals;
        }
        
        /// <summary>
        /// Constructs a set of portals from the given polygon path
        /// </summary>
        public static Portals GetPortals(List<NavMeshPolygon> polygonPath)
        {
            var portals = new Portals(polygonPath.Count);
            
            for (int i = 0; i < polygonPath.Count - 1; i++)
            {
                var current = polygonPath[i];
                var next = polygonPath[i + 1];
                
                var portal = current.GetNeighborData(next);
                var firstPortal = portal.Portal1;
                var secondPortal = portal.Portal2;

                var firstArea = TriArea2(current.Centroid, next.Centroid, firstPortal);
                var secondArea = TriArea2(current.Centroid, next.Centroid, secondPortal);

                if (firstArea > secondArea) portals.AddPortal(firstPortal, secondPortal); 
                else portals.AddPortal(secondPortal, firstPortal);
            }

            return portals;
        }
        
        /// <summary>
        /// Finds the shortest path through the given portals
        /// </summary>
        public static List<Vector2> StringPull(Portals portals)
        {
            var (left, right) = portals;
            
            var apexes = new List<Vector2> { left[0] };
            
            Vector2 portalLeft = left[0], portalRight = right[0], apex = left[0];
            int leftIndex = 0, rightIndex = 0;

            for (int i = 1; i < left.Count; i++)
            {
                var currentLeft = left[i];
                var currentRight = right[i];

                if (TriArea2(apex, portalRight, currentRight) >= 0)
                {
                    if (!apex.Approximately(portalRight) && TriArea2(apex, portalLeft, currentRight) > 0)
                    {
                        portalRight = apex = portalLeft;
                        apexes.Add(apex);
                        
                        i = rightIndex = leftIndex;
                        continue;
                    }

                    portalRight = currentRight;
                    rightIndex = i;
                }
                
                if (TriArea2(apex, portalLeft, currentLeft) <= 0)
                {
                    if (!apex.Approximately(portalLeft) && TriArea2(apex, portalRight, currentLeft) < 0)
                    {
                        portalLeft = apex = portalRight;
                        apexes.Add(apex);
                        
                        i = leftIndex = rightIndex;
                        continue;
                    }

                    portalLeft = currentLeft;
                    leftIndex = i;
                }
                
            }
            
            apexes.Add(left[^1]);
            
            return apexes;
        }
        
        private static float TriArea2(Vector2 a, Vector2 b, Vector2 c)
        {
            return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
        }
        
        private static List<NavMeshPolygon> ReconstructPath(Node node) 
        {
            var path = new List<NavMeshPolygon>();
            while (node != null) 
            {
                path.Add(node.Polygon);
                node = node.Parent;
            }

            path.Reverse();
            return path;
        }
    }
}