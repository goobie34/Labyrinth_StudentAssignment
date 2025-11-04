using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering;
using UnityEditor.Timeline;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public static class PathfindingAlgorithm
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
        Dictionary<Vector2Int, float> distanceTo = new();
        Dictionary<Vector2Int, Vector2Int?> edgeTo = new();
        SortedSet<(Vector2Int pos, float distance)> priorityQueue = new SortedSet<(Vector2Int, float)>(GetNodeCompare()); 

        //populate  distanceTo and edgeTo dictionaries so one entry exists for every node
        for (int x = 0; x < mapData.Width; x++)
        {
            for (int y = 0; y < mapData.Height; y++)
            {
                distanceTo.Add(new Vector2Int(x, y), float.PositiveInfinity);
                edgeTo.Add( new Vector2Int(x, y), null);
            }
        }

        distanceTo[start] = 0f;
        priorityQueue.Add((start, 0f));

        while(priorityQueue.Count > 0)
        {
            var currentNodeTuple = priorityQueue.Min;
            priorityQueue.Remove(currentNodeTuple);

            //this line reduces the amount of loops by about 30%, ignore queue item if we already found a faster path to this node
            if (distanceTo[currentNodeTuple.pos] < currentNodeTuple.distance) { continue; }

            Vector2Int currentNode = currentNodeTuple.pos;

            foreach (var adjNode in AdjacentNodes(currentNode, mapData))
            {
                float edgeCost = GetCost(currentNode, adjNode, mapData);

                if (distanceTo[currentNode] + edgeCost < distanceTo[adjNode])
                {
                    distanceTo[adjNode] = distanceTo[currentNode] + edgeCost;
                    edgeTo[adjNode] = currentNode;
                    
                    priorityQueue.Add((adjNode, distanceTo[adjNode]));
                }
            }
        }

        return PathTo(goal, start, distanceTo, edgeTo);
    }

    //Returns a custom comparer for the sorted set/priority queue implementation
    private static Comparer<(Vector2Int position, float distance)> GetNodeCompare()
    {
        var nodeCompare = Comparer<(Vector2Int position, float distance)>.Create((nodeA, nodeB) =>
        {
            int result = nodeA.distance.CompareTo(nodeB.distance);      //first, sorts by distance

            if (result == 0)        //if distance is same between two nodes, sort by x-coordinate
            {
                result = nodeA.position.x.CompareTo(nodeB.position.x);
                if (result == 0)    //if distance and x-coordinate are both same between two nodes, sort by y-coordinate 

                {
                    result = nodeA.position.y.CompareTo(nodeB.position.y);
                }
            }
            return result;
        });

        return nodeCompare;
    }

    private static bool HasPathTo(Vector2Int node, Dictionary<Vector2Int, float> distanceTo)
    {
        return distanceTo[node] < float.PositiveInfinity;
    } 

    private static List<Vector2Int> PathTo(Vector2Int targetNode, Vector2Int startNode, Dictionary<Vector2Int, float> distanceTo, Dictionary<Vector2Int, Vector2Int?> edgeTo)
    {
        if (!HasPathTo(targetNode, distanceTo)) return null;

        Stack<Vector2Int> revPath = new();
        List<Vector2Int> path = new();

        for (Vector2Int x = targetNode; x != startNode; x = (Vector2Int)edgeTo[x]) { revPath.Push(x); }

        for (int i = revPath.Count; i > 0; i--) { path.Add(revPath.Pop()); }

        return path;
    }

    public static bool IsMovementBlocked(Vector2Int from, Vector2Int to, IMapData mapData)
    {
        //check if "to"-node is outside labyrinth bounds
        if(to.x < 0 || to.x >= mapData.Width || to.y < 0 || to.y >= mapData.Height)
        {
            return true;
        }

        //movement is only blocked if cost == infinity
        if (GetCost(from, to, mapData) < float.PositiveInfinity)
        {
            return false;
        } else
        {
            return true;
        }
    }

    private static List<Vector2Int> AdjacentNodes(Vector2Int currentNode, IMapData mapData)
    {
        List<Vector2Int> adj = new();
        
        //add up, down, left, right neighbours
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

        //add neighbours connected by vents
        if (mapData.HasVent(currentNode.x, currentNode.y))
        {
            var otherVents = mapData.GetOtherVentPositions(currentNode);

            foreach (var vent in otherVents) { adj.Add(vent); }
        }

        return adj;
    }

    //returns the cost of moving between two nodes, float.PositiveInfinity if movement between nodes is not possible
    public static float GetCost(Vector2Int from, Vector2Int to, IMapData mapData)
    {
        float cost = float.PositiveInfinity;

        //up, down, left, right
        Vector2Int difference = to - from;
        if (difference == Vector2Int.up)
        {
            cost = mapData.GetHorizontalWallCost(to.x, to.y);
        }
        else if (difference == Vector2Int.right)
        {
            cost = mapData.GetVerticalWallCost(to.x, to.y);
        }
        if (difference == Vector2Int.down)
        {
            cost = mapData.GetHorizontalWallCost(from.x, from.y);
        }
        if (difference == Vector2Int.left)
        {
            cost = mapData.GetVerticalWallCost(from.x, from.y);
        }

        //vents
        if (mapData.HasVent(from.x, from.y) && mapData.HasVent(to.x, to.y))
        {
            cost = mapData.GetVentCost(from.x, from.y);
        }

        return cost;
    }
}