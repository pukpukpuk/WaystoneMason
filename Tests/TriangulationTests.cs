#region

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using WaystoneMason.Pathfinding;
using WaystoneMason.Pathfinding.Generation;

#endregion

namespace WaystoneMason.Tests
{
    public class TriangulationTests
    {
        [Test]
        public void LineColinear()
        {
            Assert.AreEqual(new Line(new Vector2(0, 0), new Vector2(1, 0)), 
                new Line(new Vector2(0, 0), new Vector2(1, 0)));
            
            Assert.AreEqual(new Line(new Vector2(0, 0), new Vector2(1, 0)), 
                new Line(new Vector2(1, 0), new Vector2(0, 0)));
            
            Assert.AreEqual(new Line(new Vector2(0, 0), new Vector2(1, 0)), 
                new Line(new Vector2(2, 0), new Vector2(0, 0)));
            
            Assert.AreEqual(new Line(new Vector2(0, 0), new Vector2(1, 0)), 
                new Line(new Vector2(2, 0), new Vector2(0, 0)));
            
            Assert.AreEqual(new Line(new Vector2(0, 0), new Vector2(1, 0)), 
                new Line(new Vector2(0, 0), new Vector2(10, 0)));
            
            Assert.AreEqual(new Line(new Vector2(0, 0), new Vector2(1, 0)), 
                new Line(new Vector2(0, 0), new Vector2(-10, 0)));
        }
        
        [Test]
        public void LineNonColinear()
        {
            Assert.AreNotEqual(new Line(new Vector2(0, 0), new Vector2(1, 0)), 
                new Line(new Vector2(0, 1), new Vector2(1, 1)));
            
            Assert.AreNotEqual(new Line(new Vector2(0, 0), new Vector2(0, 2)), 
                new Line(new Vector2(0, 0), new Vector2(1, 1)));
            
            Assert.AreNotEqual(new Line(new Vector2(0, 1), new Vector2(1, 2)), 
                new Line(new Vector2(0, 2), new Vector2(1, 3)));
            
            Assert.AreNotEqual(new Line(new Vector2(0, 1), new Vector2(1, 2)), 
                new Line(new Vector2(0,3), new Vector2(1, 2)));
        }

        [Test]
        public void ConvertToPolygonsFirstTest()
        {
            var triangles = new List<Vector2[]>
            {
                new Vector2[]
                {
                    new (0, 0),
                    new (1, 0),
                    new (1, 1),
                },                
                new Vector2[]
                {
                    new (0, 0),
                    new (1, 1),
                    new (0, 1),
                },
                new Vector2[]
                {
                    new (0, 5),
                    new (1, 5),
                    new (1, 6),
                },
                new Vector2[]
                {
                    new (0, 5.2f),
                    new (1, 5.2f),
                    new (1, 6.2f)
                }
            };

            CheckLinksCount(triangles, 4, 2);
        }
        
        [Test]
        public void ConvertToPolygonsWithOneOverlappingPoint()
        {
            var triangles = new List<Vector2[]>
            {
                new Vector2[]
                {
                    new (0, 0),
                    new (1, 0),
                    new (1, 1),
                },
                new Vector2[]
                {
                    new (1, 1),
                    new (2, 1),
                    new (2, 2),
                }
            };
            
            CheckLinksCount(triangles, 0, 0);
        }
        
        [Test]
        public void ConvertToPolygonsWithBigTriangle()
        {
            var triangles = new List<Vector2[]>
            {
                new Vector2[]
                {
                    new (0, 0),
                    new (2, 0),
                    new (2, 4),
                },
                new Vector2[]
                {
                    new (0, 0),
                    new (1, 2),
                    new (0, 2),
                },
                new Vector2[]
                {
                    new (1, 2),
                    new (2, 4),
                    new (1, 4),
                }
            };
            
            CheckLinksCount(triangles, 3, 2);
        }

        [Test]
        public void ConvertToPolygonsCheckPortalGeneration()
        {
            var triangles = new List<Vector2[]>
            {
                new Vector2[]
                {
                    new (0, 0),
                    new (3, 3),
                    new (0, 3),
                },
                new Vector2[]
                {
                    new (1, 1),
                    new (4, 1),
                    new (4, 4),
                }
            };
            
            var polygons = Triangulation.ConvertToPolygons(triangles);

            var bottomPolygon = polygons.Find(polygon => polygon.Vertices.Contains(new Vector2(4, 4)));
            var topPolygon = polygons.Find(polygon => polygon.Vertices.Contains(new Vector2(0, 0)));

            var bottomData = bottomPolygon.GetNeighborData(topPolygon);
            var topData = topPolygon.GetNeighborData(bottomPolygon);

            var expectedPortal1 = new Vector2(1, 1);
            var expectedPortal2 = new Vector2(3, 3);

            Assert.IsTrue(bottomData.Portal1.Approximately(expectedPortal2));
            Assert.IsTrue(bottomData.Portal2.Approximately(expectedPortal1));
            
            Assert.IsTrue(topData.Portal1.Approximately(expectedPortal1));
            Assert.IsTrue(topData.Portal2.Approximately(expectedPortal2));
        }

        private static void CheckLinksCount(List<Vector2[]> triangles, int withNeighborsCount, int linksCount)
        {
            var polygons = Triangulation.ConvertToPolygons(triangles);
            
            var actualWithNeighborsCount = polygons.Count(polygon => polygon.Neighbors.Any());
            Assert.AreEqual(withNeighborsCount, actualWithNeighborsCount);
            
            var actualLinksCount = polygons.Sum(polygon => polygon.Neighbors.Count()) / 2;
            Assert.AreEqual(linksCount, actualLinksCount);
        }
    }
}