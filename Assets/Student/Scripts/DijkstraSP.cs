using System;
using System.Collections.Generic;

public class DijkstraSP
{
    struct Node : IComparable<Node>
    {
        public Node(int v, int weight) { this.v = v; this.weight = weight; }
        public int v;
        public int weight;
        public int CompareTo(Node other)
        {
            int result = weight.CompareTo(other.weight);
            if (result == 0) result = v.CompareTo(other.v);
            return result;
        }
    }

    private DirectedEdge[] edgeTo;
    private int[] distTo;
    private SortedSet<Node> pq;
    public DijkstraSP(EdgeWeightedDigraph G, int s)
    {
        int whileCount = 0;
        edgeTo = new DirectedEdge[G.V];
        distTo = new int[G.V];
        pq = new SortedSet<Node>();
        for (int v = 0; v < G.V; v++)
            distTo[v] = int.MaxValue;
        distTo[s] = 0;
        pq.Add(new Node(s, 0));
        while (pq.Count > 0)
        {
            Node min = pq.Min;
            pq.Remove(min);
            if (min.weight > distTo[min.v]) continue; //needed for lazy delete
            Relax(G, min.v);

            whileCount++;
        }

        UnityEngine.Debug.Log("Number of times through while loop: " + whileCount);
    }
    private void Relax(EdgeWeightedDigraph G, int v)
    {
        foreach (DirectedEdge e in G.Adj(v))
        {
            int w = e.To;
            if (distTo[w] > distTo[v] + e.Weight)
            {
                distTo[w] = distTo[v] + e.Weight;
                edgeTo[w] = e;
                pq.Add(new Node(w, distTo[w])); //lazy delete, just add instead of updating

                //if (pq.contains(w)) pq.change(w, distTo[w]);
                //else pq.insert(w, distTo[w]);
            }
        }
    }
    public double DistTo(int v) { return distTo[v]; }
    public bool hasPathTo(int v) { return distTo[v] < int.MaxValue; }
    public IEnumerable<DirectedEdge> pathTo(int v)
    {
        if (!hasPathTo(v)) return null;
        Stack<DirectedEdge> path = new Stack<DirectedEdge>();
        for (DirectedEdge e = edgeTo[v]; e != null; e = edgeTo[e.From])
            path.Push(e);
        return path;
    }
}