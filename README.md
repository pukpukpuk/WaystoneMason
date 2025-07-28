<p align="center"><img src="Documentation/Images/Cover.png" width="500" ></p>

[Документация на русском](Documentation/README-RU.md)

[**Waystone Mason**](#semantics-of-the-name) is a library for 2D pathfinding using navmeshes.

What makes this project different from existing solutions?
- Navmeshes are not singletons, allowing agents to have personal navmeshes.
- You can manually control how obstacle updates affect the navmesh,
  which can be used, for example, to simulate agents’ memory.

# Brief Description of How It Works

### NavMesh Operation
The walkable area is marked with triangles — if there is no triangle, the area is not walkable.  
Triangles are stored via a spatial index — in other words, grouped into **chunks**, which are square areas of fixed size arranged in a grid.

Accordingly, a chunk will only be created automatically if an obstacle appears within its bounds.  
Therefore, it is recommended to pre-generate chunks in required areas either manually via `NavMesh.GetOrCreateChunk()`,  
or define a region for pre-generation via [WMObstaclesHolder.PregeneratedEmptyChunksRegion](#WMObstaclesHolder)

### Pathfinding Operation
Triangles can be represented as a graph, where each node corresponds to a triangle, and an edge exists between two nodes if the corresponding triangles share a common edge.
To find a path through this graph, the A* algorithm is used to compute a sequence of triangles that leads to the target.
This sequence is then processed by the String Pulling algorithm, which produces the final path for the agent.

# Documentation

**Waystone Mason** offers two levels of interaction with the navigation system:  
[manual](#manual-control) and [automatic](#automatic-control).

## Automatic Control

The project provides several components to set up basic pathfinding infrastructure.

### WMNavMeshHolder
On game start, it creates a NavMesh instance with a given agent radius and periodically rebuilds it.

Has an event `OnBeforeRebuild`, which is called before each rebuild. Useful for manually scanning obstacles.

### WMAgent
Has a method for setting a movement target – `SetGoal()`.  
When called, the agent computes a path to the goal using the NavMesh from the specified `WMNavMeshHolder` and starts following it.

### WMDynamicObstacle
Builds an obstacle contour based on the specified collider. Does nothing on its own —  
you must call the `Affect()` method to apply its state to the given navmesh.

### WMObstaclesHolder
Container for all obstacles in the scene. Allows efficiently querying obstacles within a radius.

The field `PregeneratedEmptyChunksRegion` defines a rectangular region where empty chunks will be pre-created at game start.

**This component is required for the others to work.**

### WMNavMeshObstaclesScanner
Scans for suitable `WMDynamicObstacle`s based on specified conditions  
and calls their `Affect()` method for the target `WMNavMeshHolder`.

## Manual Control

If the built-in components are not enough and you need finer control over navmesh content,  
drop down to the core library level.

To create a navmesh, you only need to specify the agent collider radius.  
Note that **the radius cannot be changed after the object is created**.

```cs
var agentRadius = 0.25f;
var navMesh = new NavMesh(agentRadius);
```

To manually add an obstacle, pass its contour as a `Clipper.PathD` object.  
To reflect the change, you must remove the old one and add the updated one.

```cs
var triangleObstacle = new PathsD { new PathD { new PointD(0, 0), new PointD(1, 0), new PointD(1, 1) } };
navMesh.AddObstacle(triangleObstacle);
navMesh.Rebuild();

// Later
navMesh.RemoveObstacle(triangleObstacle);
var translated = Clipper.TranslatePaths(triangleObstacle, 1, 0);
navMesh.AddObstacle(translated);

navMesh.Rebuild();
```

If the obstacle is meant to never change or disappear, you can add it permanently to the navmesh.  
Permanent obstacles cannot be removed, but are cheaper to process.

```cs
navMesh.AddObstacle(triangleObstacle, true);
```

NavMesh rebuilding is a **costly operation**, so adding/removing obstacles **does not rebuild automatically**.  
You must explicitly call `NavMesh.Rebuild()`.

To get a path, simply provide the start and goal points. The method returns `false` if the path cannot be computed.

```cs
var start = new Vector2(0, 0);
var goal = new Vector2(2, 2);
if (!navMesh.TryComputePath(start, goal, out var path)) Debug.Log("Agent can't reach the goal!");
```

# Semantics of the Name
**Waystone**: The final path for the agent is a series of corners — places to turn and then move straight —  
so they can be called waystones (guiding stones).

**Mason**: The library itself is the *mason*, because it builds the waystones for agents to follow.
