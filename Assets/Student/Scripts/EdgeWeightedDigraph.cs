using System.Collections.Generic;
public class DirectedEdge
{
    public int From { get; private set; }
    public int To { get; private set; }
    public int Weight { get; private set; }
    public DirectedEdge(int from, int to, int weight)
    {
        this.From = from;
        this.To = to;
        this.Weight = weight;
    }
}
public class EdgeWeightedDigraph
{
    public int V { get; private set; }
    public int E { get; private set; }
    private List<DirectedEdge>[] adj;

    public EdgeWeightedDigraph(int v)
    {
        this.V = v;
        this.E = 0;
        adj = (List<DirectedEdge>[])new List<DirectedEdge>[V];
        for (int i = 0; i < V; i++)
            adj[i] = new List<DirectedEdge>();
    }

    public void AddEdge(DirectedEdge e)
    {
        adj[e.From].Add(e);
        E++;
    }

    public IEnumerable<DirectedEdge> Adj(int v) { return adj[v]; }
    public IEnumerable<DirectedEdge> Edges()
    {
        List<DirectedEdge> bag = new List<DirectedEdge>();
        for (int v = 0; v < V; v++)
            foreach (DirectedEdge e in adj[v])
                bag.Add(e);
        return bag;
    }
}