#region

using System.Collections.Generic;
using System.Linq;
using Clipper2Lib;
using LibTessDotNet;
using UnityEngine;
using WaystoneMason.Pathfinding.Core;

#endregion

namespace WaystoneMason.Pathfinding.Generation
{
    public static class Triangulation
    {
        public static List<Vector2[]> Triangulate(PathsD paths)
        {
            var tess = new Tess();
            tess.NoEmptyPolygons = true;

            foreach (var path in paths)
            {
                var points = path.Select(ToContourVertex);
                var list = points.ToList();
                tess.AddContour(list, ContourOrientation.CounterClockwise);
            }
            
            tess.Tessellate();
            var trianglesCount = tess.ElementCount;
            
            var triangles = new List<Vector2[]>();
            
            for (int i = 0; i < trianglesCount; i++)
            {
                var triangle = new Vector2[3];
                triangle[0] = ToVector2(tess.Vertices[tess.Elements[i * 3]]);
                triangle[1] = ToVector2(tess.Vertices[tess.Elements[i * 3 + 1]]);
                triangle[2] = ToVector2(tess.Vertices[tess.Elements[i * 3 + 2]]);
                
                triangles.Add(triangle);
            }

            return triangles;
        }

        public static List<NavMeshPolygon> ConvertToPolygons(List<Vector2[]> triangles)
        {
            var polygonsEnumerable = triangles.Select(triangle => new NavMeshPolygon(triangle));
            var polygons = new List<NavMeshPolygon>(polygonsEnumerable);

            var lineDictionary = GroupByCollinearity(polygons);

            foreach (var sides in lineDictionary.Values)
            {
                LinkOverlappingColinearSides(sides);
            }

            return polygons;
        }
        
        public static Dictionary<Line, List<PolygonSide>> GroupByCollinearity(IEnumerable<NavMeshPolygon> polygons)
        {
            var lineDictionary = new Dictionary<Line, List<PolygonSide>>();
            
            foreach (var polygon in polygons)
            {
                for (int i = 0; i < 3; i++)
                {
                    var a = polygon.Vertices[i];
                    var b = polygon.Vertices[(i + 1) % 3];

                    var key = new Line(a, b);
                    if (!lineDictionary.TryGetValue(key, out var list))
                    {
                        list = lineDictionary[key] = new List<PolygonSide>();
                    }

                    var side = new PolygonSide(a, b, polygon);
                    list.Add(side);
                }
            }

            return lineDictionary;
        }
        
        public static void LinkOverlappingColinearSides(List<PolygonSide> sides)
        {
            if (sides.Count < 2) return;
            
            var origin = sides[0].A;
            var direction = (sides[0].B - origin).normalized;

            var projected = new List<(PolygonSide side, float t0, float t1)>(sides.Count);
            foreach (var side in sides)
            {
                var a = Vector2.Dot(side.A - origin, direction);
                var b = Vector2.Dot(side.B - origin, direction);
                float t0 = Mathf.Min(a, b), t1 = Mathf.Max(a, b);
                    
                projected.Add((side, t0, t1));
            }
            
            projected.Sort((a, b) => a.t0.CompareTo(b.t0));
            for (int i = 0; i < projected.Count; i++)
            {
                var a = projected[i];
                for (int j = i + 1; j < projected.Count; j++)
                {
                    var b = projected[j];
                    if (b.t0 > a.t1)
                        break;
                    
                    if (a.side.Polygon == b.side.Polygon || a.side.Polygon.IsNeighbor(b.side.Polygon)) continue;
                        
                    var point1 = direction * Mathf.Max(a.t0, b.t0) + origin;
                    var point2 = direction * Mathf.Min(a.t1, b.t1) + origin;
                        
                    if ((point2 - point1).sqrMagnitude < 0.0001f) continue;

                    a.side.AddNeighbor(b.side, direction, point1, point2);
                    b.side.AddNeighbor(a.side, direction, point1, point2);
                }
            }
        }
        
        #region Utils
        
        private static ContourVertex ToContourVertex(PointD point)
        {
            return new ContourVertex(new Vec3((float)point.x, (float)point.y, 0));
        }
        
        private static Vector2 ToVector2(ContourVertex vertex)
        {
            return new Vector2(vertex.Position.X, vertex.Position.Y);
        }

        #endregion
    }
}