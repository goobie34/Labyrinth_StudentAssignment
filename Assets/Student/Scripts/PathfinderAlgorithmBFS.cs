using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Timeline;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public static class PathfindingAlgorithmBFS
{
    /* <summary>
     TODO: Implement pathfinding algorithm here
     Find the shortest path from start to goal position in the maze.
     
     Dijkstra's Algorithm Steps:
     1. Initialize distances to all nodes as infinity
     2. Set distance to start node as 0
     3. Add start node to priority queue
     4. While priority queue is not empty:
        a. Remove node with minimum distance
        b. If it's the goal, reconstruct path
        c. For each neighbor:
           - Calculate new distance through current node
           - If shorter, update distance and add to queue
     
     MAZE FEATURES TO HANDLE:
     - Basic movement cost: 1.0 between adjacent cells
     - Walls: Some have infinite cost (impassable), others have climbing cost
     - Vents (teleportation): Allow instant travel between distant cells with usage cost
     
     AVAILABLE DATA STRUCTURES:
     - Dictionary<Vector2Int, float> - for tracking distances
     - Dictionary<Vector2Int, Vector2Int> - for tracking previous nodes (path reconstruction)
     - SortedSet<T> or List<T> - for priority queue implementation
     - mapData provides methods to check walls, vents, and boundaries
     
     HINT: Start simple with BFS (ignore wall costs and vents), then extend to weighted Dijkstra
     </summary> */
    public static List<Vector2Int> FindShortestPath(Vector2Int start, Vector2Int goal, IMapData mapData)
    {
        Dictionary<Vector2Int, bool> marked = new();
        Dictionary<Vector2Int, Vector2Int?> edgeTo = new();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        for (int x = 0; x < mapData.Width; x++)
        {
            for (int y = 0; y < mapData.Height; y++)
            {
                marked.Add(new Vector2Int(x, y), false);
                edgeTo.Add(new Vector2Int(x, y), null);
            }
        }

        marked[start] = true;
        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            Vector2Int currentNode = queue.Dequeue();
            foreach(Vector2Int adjNode in AdjacentNodes(currentNode, mapData)) {
                if (!marked[adjNode])
                {
                    edgeTo[adjNode] = currentNode;
                    marked[adjNode] = true;
                    queue.Enqueue(adjNode);
                }
            }
        }

        return PathTo(goal, start, marked, edgeTo);

        //foreach(KeyValuePair<Vector2Int, bool> pair in marked)
        //{
        //    Debug.Log(pair);
        //}

        //foreach(KeyValuePair<Vector2Int, Vector2Int?> pair in edgeTo)
        //{
        //    Debug.Log(pair);
        //}
    }

    private static bool HasPathTo(Vector2Int node, Dictionary<Vector2Int, bool> marked)
    {
        return marked[node];
    } 

    private static List<Vector2Int> PathTo(Vector2Int targetNode, Vector2Int startNode, Dictionary<Vector2Int, bool> marked, Dictionary<Vector2Int, Vector2Int?> edgeTo)
    {
        if (!HasPathTo(targetNode, marked)) return null;

        Stack<Vector2Int> revPath = new();

        for(Vector2Int x = targetNode; x != startNode; x = (Vector2Int)edgeTo[x])
            { revPath.Push(x); }

        List<Vector2Int> path = new();

        Debug.Log("STACK COUNT IS: " + revPath.Count);
        int pathLength = revPath.Count;
        for (int i = 0; i < pathLength; i++)
        {
            path.Add(revPath.Pop());
        }

        Debug.Log("PATH COUNT IS: " + path.Count);

        return path;
    }

    public static bool IsMovementBlocked(Vector2Int from, Vector2Int to, IMapData mapData)
    {
        // TODO: Implement movement blocking logic
        // For now, allow all movement so character can move while you work on pathfinding

        if(to.x < 0  || to.x >= mapData.Width || to.y < 0 || to.y >= mapData.Height)
        {
            return true;
        }

        Vector2Int difference = to - from;
        if (difference == Vector2Int.up && mapData.HasHorizontalWall(to.x, to.y))
        {
            return true;
        }  
        else if (difference == Vector2Int.right && mapData.HasVerticalWall(to.x, to.y))
        {
            return true;
        }
        if (difference == Vector2Int.down && mapData.HasHorizontalWall(from.x, from.y))
        {
            return true;
        }
        if (difference == Vector2Int.left && mapData.HasVerticalWall(from.x, from.y))
        {
            return true;
        }

        return false;
    }

    private static List<Vector2Int> AdjacentNodes(Vector2Int currentNode, IMapData mapData)
    {
        List<Vector2Int> adj = new();

        if (!IsMovementBlocked(currentNode, currentNode + Vector2Int.up, mapData))
        {
            adj.Add(currentNode + Vector2Int.up);
        }

        if (!IsMovementBlocked(currentNode, currentNode + Vector2Int.right, mapData))
        {
            adj.Add(currentNode + Vector2Int.right);
        }

        if (!IsMovementBlocked(currentNode, currentNode + Vector2Int.down, mapData))
        {
            adj.Add(currentNode + Vector2Int.down);
        }

        if (!IsMovementBlocked(currentNode, currentNode + Vector2Int.left, mapData))
        {
            adj.Add(currentNode + Vector2Int.left);
        }
         
        return adj;
    }
}