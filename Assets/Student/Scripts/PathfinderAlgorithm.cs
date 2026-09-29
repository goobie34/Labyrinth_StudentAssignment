using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class PathfindingAlgorithm
{
    // TODO: Implement the pathfinding assignment here.
    // Find the lowest-cost path from start to goal using the map dimensions, wall costs and directed jumps exposed by IMapData.
    public static List<Vector2Int> FindShortestPath(Vector2Int start, Vector2Int goal, IMapData mapData)
    {
        // Most of your solution should be implemented in this method.

        //Debug.LogWarning("FindShortestPath is not implemented.");
        EdgeWeightedDigraph graph = new EdgeWeightedDigraph(mapData.Width * mapData.Height);
        List<Vector2Int> path = new();


        int startIndex = GetIndex(start, mapData);
        int goalIndex = GetIndex(goal, mapData);

        for (int y = 0; y < mapData.Height; y++)
        {
            for (int x = 0; x < mapData.Width; x++)
            {
                Vector2Int from = new Vector2Int(x, y);
                Vector2Int to;
                if (!ValidatePosition(from, mapData)) throw new System.Exception("PathfinderAlgorithm: Invalid from position");

                CheckAndAddEdge(from, from + Vector2Int.up, mapData, ref graph);
                CheckAndAddEdge(from, from + Vector2Int.right, mapData, ref graph);
                CheckAndAddEdge(from, from + Vector2Int.down, mapData, ref graph);
                CheckAndAddEdge(from, from + Vector2Int.left, mapData, ref graph);

                if (mapData.HasJumpFrom(x, y))
                {
                    foreach(JumpConnection jump in mapData.GetJumpsFrom(from))
                    {
                        graph.AddEdge(new DirectedEdge(GetIndex(from, mapData), GetIndex(jump.To, mapData), (int)jump.Cost));
                    }
                }







                //to = from + Vector2Int.up;
                //if (ValidatePosition(to, mapData) && !IsMovementBlocked(from, to, mapData)) graph.AddEdge(new DirectedEdge(GetIndex(from, mapData), GetIndex(to, mapData), 1));

                //to = from + Vector2Int.right;
                //if (ValidatePosition(to, mapData) && !IsMovementBlocked(from, to, mapData)) graph.AddEdge(new DirectedEdge(GetIndex(from, mapData), GetIndex(to, mapData), 1));

                //to = from + Vector2Int.down;
                //if (ValidatePosition(to, mapData) && !IsMovementBlocked(from, to, mapData)) graph.AddEdge(new DirectedEdge(GetIndex(from, mapData), GetIndex(to, mapData), 1));

                //to = from + Vector2Int.left;
                //if (ValidatePosition(to, mapData) && !IsMovementBlocked(from, to, mapData)) graph.AddEdge(new DirectedEdge(GetIndex(from, mapData), GetIndex(to, mapData), 1));
            }
        }

        DijkstraSP shortestPath = new DijkstraSP(graph, GetIndex(start, mapData));
        foreach(DirectedEdge edge in shortestPath.pathTo(GetIndex(goal, mapData)))
        {
            path.Add(GetPosition(edge.From, mapData));
        }
        path.Add(goal);

        return path;
    }

    private static void CheckAndAddEdge(Vector2Int from, Vector2Int to, IMapData mapData, ref EdgeWeightedDigraph graph)
    {
        if (!ValidatePosition(to, mapData)) return;
        if (IsMovementBlocked(from, to, mapData)) return;
            
        graph.AddEdge(new DirectedEdge(GetIndex(from, mapData), GetIndex(to, mapData), (int)GetMovementCost(from, to, mapData)));
    }

    public static float GetMovementCost(Vector2Int from, Vector2Int to, IMapData mapData)
    {
        float cost = -1f;

        int deltaX = to.x - from.x;
        int deltaY = to.y - from.y;

        if (Mathf.Abs(deltaX) + Mathf.Abs(deltaY) != 1) throw new System.Exception("Trying to get movement cost for illegal move");

        if (deltaX != 0)
        {
            int wallX = deltaX > 0 ? to.x : from.x;
            cost = mapData.GetVerticalWallCost(wallX, from.y);
            return cost;
        }

        int wallY = deltaY > 0 ? to.y : from.y;
        cost = mapData.GetHorizontalWallCost(from.x, wallY);
        return cost;
    }

    public static bool IsMovementBlocked(Vector2Int from, Vector2Int to, IMapData mapData)
    {
        if (mapData.TryGetJumpCost(from, to, out _))
            return false;

        int deltaX = to.x - from.x;
        int deltaY = to.y - from.y;

        if (Mathf.Abs(deltaX) + Mathf.Abs(deltaY) != 1)
            return true;

        if (deltaX != 0)
        {
            int wallX = deltaX > 0 ? to.x : from.x;
            float wallCost = mapData.GetVerticalWallCost(wallX, from.y);
            return float.IsPositiveInfinity(wallCost) || wallCost >= float.MaxValue;
        }

        int wallY = deltaY > 0 ? to.y : from.y;
        float horizontalWallCost = mapData.GetHorizontalWallCost(from.x, wallY);
        return float.IsPositiveInfinity(horizontalWallCost) || horizontalWallCost >= float.MaxValue;
    }

    public static int GetIndex(Vector2Int point, IMapData mapData) { return point.y * mapData.Width + point.x; }
    public static Vector2Int GetPosition(int index, IMapData mapData)
    {
        int x = index % mapData.Width;
        int y = index / mapData.Width;
        return new Vector2Int(x, y);
    }

    public static bool ValidatePosition(Vector2Int point, IMapData mapData)
    {
        if (point.x < 0 || point.y < 0) return false;
        if (point.x >= mapData.Width)   return false;
        if (point.y >= mapData.Height)  return false;

        return true;
    }
}