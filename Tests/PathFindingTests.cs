using NUnit.Framework;
using UnityEngine;
using WaystoneMason.Pathfinding;
using WaystoneMason.Pathfinding.Core;

namespace WaystoneMason.Tests
{
    public class PathFindingTests
    {
        [Test]
        public void TestForwardMoveThroughOnePortal()
        {
            var portals = new Portals();
            Vector2 start = new(0, -2), goal = new(0, 2);
            
            portals.AddPortal(start);
            portals.AddPortal(new Vector2(-1, 0), new Vector2(1, 0));
            portals.AddPortal(goal);

            var path = AStar.StringPull(portals);
            Assert.AreEqual(2, path.Count);
            Assert.IsTrue(path[0].Approximately(start));
            Assert.IsTrue(path[1].Approximately(goal));
        }
        
        [Test]
        public void TestForwardMoveThroughTwoPortals()
        {
            var portals = new Portals();
            Vector2 start = new(2, -1), goal = new(-1, 2);
            
            portals.AddPortal(start);
            portals.AddPortal(new Vector2(0, 0), new Vector2(2, 0));
            portals.AddPortal(new Vector2(0, 0), new Vector2(0, 2));
            portals.AddPortal(goal);

            var path = AStar.StringPull(portals);
            Assert.AreEqual(2, path.Count);
            Assert.IsTrue(path[0].Approximately(start));
            Assert.IsTrue(path[1].Approximately(goal));
        }
        
        [Test]
        public void TestForwardMoveThroughCornerOfTwoPortals()
        {
            var portals = new Portals();
            Vector2 start = new(1, -1), goal = new(-1, 1);
            
            portals.AddPortal(start);
            portals.AddPortal(new Vector2(0, 0), new Vector2(2, 0));
            portals.AddPortal(new Vector2(0, 0), new Vector2(0, 2));
            portals.AddPortal(goal);

            var path = AStar.StringPull(portals);
            Assert.AreEqual(2, path.Count);
            Assert.IsTrue(path[0].Approximately(start));
            Assert.IsTrue(path[1].Approximately(goal));
        }
        
        [Test]
        public void TestMoveWithTurnThroughTwoPortals()
        {
            var portals = new Portals();
            Vector2 start = new(1, -2), goal = new(-2, 1);
            Vector2 corner = new (0, 0);
            
            portals.AddPortal(start);
            portals.AddPortal(corner, new Vector2(2, 0));
            portals.AddPortal(corner, new Vector2(0, 2));
            portals.AddPortal(goal);

            var path = AStar.StringPull(portals);
            Assert.AreEqual(3, path.Count);
            Assert.IsTrue(path[0].Approximately(start));
            Assert.IsTrue(path[1].Approximately(corner));
            Assert.IsTrue(path[2].Approximately(goal));
        }
        
        [Test]
        public void TestMoveAlongThePortalLine()
        {
            var portals = new Portals();
            Vector2 start = new(0, 0), goal = new(0, 3);
            
            portals.AddPortal(start);
            portals.AddPortal(new Vector2(0, 1), new Vector2(0, 2));
            portals.AddPortal(goal);

            var path = AStar.StringPull(portals);
            Assert.AreEqual(2, path.Count);
            Assert.IsTrue(path[0].Approximately(start));
            Assert.IsTrue(path[1].Approximately(goal));
        }
        
        [Test]
        public void TestMoveWithTurnThroughTwoNonOverlappingPortals()
        {
            var portals = new Portals();
            Vector2 start = new(0, 0), goal = new(0, 4);
            
            portals.AddPortal(start);
            portals.AddPortal(new Vector2(-1, 1), new Vector2(1, 1));
            portals.AddPortal(new Vector2(-2, 1), new Vector2(-2, 3));
            portals.AddPortal(goal);

            var path = AStar.StringPull(portals);
            Assert.AreEqual(3, path.Count);
            Assert.IsTrue(path[0].Approximately(start));
            Assert.IsTrue(path[1].Approximately(new Vector2(-2, 3)));
            Assert.IsTrue(path[2].Approximately(goal));
        }
    }
}