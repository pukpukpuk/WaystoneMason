using System.Collections.Generic;
using Clipper2Lib;
using UnityEngine;
using WaystoneMason.PathFinding.Generation;

namespace WaystoneMason.PathFinding.Core
{
    public class Chunk
    {
        public const int Size = 5;
        
        private readonly NavMesh _navMesh;
        private readonly Vector2Int _chunkPosition;
        private readonly List<(Line, Vector2Int)> _borders = new();
        
        private readonly HashSet<NavMeshPolygon> _polygons = new();
        private readonly Dictionary<Line, List<PolygonSide>> _borderSides = new();

        private PathsD _baseContour;
        private readonly HashSet<PathsD> _obstacles = new();

        public IEnumerable<NavMeshPolygon> Polygons => _polygons;

        public Chunk(NavMesh navMesh, Vector2Int chunkPosition)
        {
            _navMesh = navMesh;
            _chunkPosition = chunkPosition;

            var bottomLeft = (Vector2) (chunkPosition * Size);
            var bottomRight = bottomLeft + new Vector2(Size, 0);
            var topLeft     = bottomLeft + new Vector2(0,    Size);
            var topRight    = bottomLeft + new Vector2(Size, Size);
            
            _borders.Add((new Line(bottomLeft, bottomRight), Vector2Int.down));
            _borders.Add((new Line(bottomLeft, topLeft),     Vector2Int.left));
            _borders.Add((new Line(topRight, topLeft),       Vector2Int.up));
            _borders.Add((new Line(topRight, bottomRight),   Vector2Int.right));

            _baseContour = CreateRectPath(chunkPosition);
        }

        public void Add(PathsD path, bool permanent)
        {
            if (!permanent)
            {
                _obstacles.Add(path);
                return;
            }

            _baseContour = Difference(_baseContour, path);
        }

        public void Remove(PathsD path) => _obstacles.Remove(path);
        
        public void Rebuild()
        {
            DestroyLinksToNeighborChunks();
            
            var freeSpaceContour = ComputeFreeSpacePath();
            Triangulate(freeSpaceContour);
            
            var lineDictionary = Triangulation.GroupByCollinearity(_polygons);
            LinkOwnPolygons(lineDictionary);
			UpdateBorderPolygons(lineDictionary);
            LinkBordersToNeighborChunks();
        }
        
        private void DestroyLinksToNeighborChunks()
        {
            var processed = new HashSet<NavMeshPolygon>();
            foreach (var list in _borderSides.Values)
            {
                foreach (var polygonSide in list)
                {
                    var polygon = polygonSide.Polygon;
                    if (!processed.Add(polygon)) continue;
                    
                    foreach (var neighbor in polygon.Neighbors) neighbor.RemoveNeighbor(polygon);
                }
                list.Clear();
            }
        }

        private PathsD ComputeFreeSpacePath()
        {
            var eraser = new PathsD();
            foreach (var paths in _obstacles) eraser.AddRange(paths);
            
            eraser = Clipper.Union(eraser, FillRule.EvenOdd);

            var freeSpacePath = Difference(_baseContour, eraser);
            return freeSpacePath;
        }

        private void Triangulate(PathsD freeSpaceContour)
        {
            _polygons.Clear();
            
            var triangles = Triangulation.Triangulate(freeSpaceContour);
            _polygons.EnsureCapacity(triangles.Count);

            foreach (var triangle in triangles)
            {
                var polygon = new NavMeshPolygon(triangle);
                _polygons.Add(polygon);
            }
        }

        private void LinkOwnPolygons(Dictionary<Line, List<PolygonSide>> lineDictionary)
        {
            foreach (var list in lineDictionary.Values)
            {
                Triangulation.LinkOverlappingColinearSides(list);
            }
        }
        
        private void UpdateBorderPolygons(Dictionary<Line, List<PolygonSide>> lineDictionary)
        {
            foreach (var (borderLine, _) in _borders)
            {
                if (!lineDictionary.TryGetValue(borderLine, out var actualOwnBorderSides)) continue;

                if (!_borderSides.TryGetValue(borderLine, out var cachedBorderSides))
                {
                    _borderSides[borderLine] = cachedBorderSides = new List<PolygonSide>();
                }
                
                cachedBorderSides.AddRange(actualOwnBorderSides);
            }
        }

        private void LinkBordersToNeighborChunks()
        {
            foreach (var (borderLine, vectorToNeighbor) in _borders)
            {
                if (!_navMesh.TryGetChunk(_chunkPosition + vectorToNeighbor, out var chunk)) continue;
                if (!chunk._borderSides.TryGetValue(borderLine, out var neighborBorderSides)) continue;
                
                var ownBorderSides = _borderSides[borderLine];
                
                var combined = new List<PolygonSide>(ownBorderSides.Count + neighborBorderSides.Count);
                combined.AddRange(ownBorderSides);
                combined.AddRange(neighborBorderSides);
                
                Triangulation.LinkOverlappingColinearSides(combined);
            }
        }

        #region Equals
        
        public override bool Equals(object obj)
        {
            if (obj is not Chunk other) return false;
            return _chunkPosition == other._chunkPosition && _navMesh == other._navMesh;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_chunkPosition.GetHashCode(), _navMesh.GetHashCode());
        }

        #endregion
        
        #region Static

        private static PathsD CreateRectPath(Vector2Int chunkPosition)
        {
            var chunkRect = new Rect64(
                chunkPosition.x * Size, 
                chunkPosition.y * Size + Size, 
                chunkPosition.x * Size + Size, 
                chunkPosition.y * Size);

            return new PathsD { Clipper.PathD(chunkRect.AsPath()) };
        }

        private static PathsD Difference(PathsD subject, PathsD eraser)
        {
            return Clipper.Difference(subject, eraser, FillRule.EvenOdd);
        }

        #endregion
    }
}