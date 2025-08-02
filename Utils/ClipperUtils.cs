#region

using Clipper2Lib;
using UnityEngine;

#endregion

namespace WaystoneMason.Utils
{
    public static class ClipperUtils
    {
        public static PathsD Multiply(PathsD paths, Vector2 vector)
        {
            var pathsD = new PathsD(paths.Count);
            foreach (var path in paths)
            {
                var scaledPath = new PathD(path.Count);
                foreach (var point in path)
                {
                    var scaledPoint = new PointD(point.x * vector.x, point.y * vector.y);
                    scaledPath.Add(scaledPoint);
                }
                pathsD.Add(scaledPath);
            }
                
            return pathsD;
        }
        
        public static PathsD Divide(PathsD paths, Vector2 vector)
        {
            return Multiply(paths, new Vector2(1 / vector.x, 1 / vector.y));
        }
    }
}