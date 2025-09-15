using Lab2.Entities;

namespace Lab2.Containers;

public class BfsNodeContainer(int ramLimitMb) : NodeContainer(ramLimitMb)
{
    private readonly HashSet<Vector> _visited = [];
    private readonly Queue<Node> _queue = new();
    
    public override int Count => _queue.Count;
    
    public override string AlgorithmName => "BFS";
    
    public override void Enqueue(Node node)
    {
        if (!_visited.Add(node.State)) return;
        
        _queue.Enqueue(node);
        TrackObj();
    }

    public override Node Dequeue() => _queue.Dequeue();
}