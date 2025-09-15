using Lab2.Entities;

namespace Lab2.Containers;

public class AStarNodeContainer(int ramLimitMb) : NodeContainer(ramLimitMb)
{
    private readonly PriorityQueue<AStarNode, float> _queue = new();
    private readonly Dictionary<Vector, float> _bestCost = new();
    
    public override int Count => _queue.Count;
    public override string AlgorithmName => "A*";
    
    protected override Node CreateInitNode(Vector state) => new AStarNode(state);

    public override void Enqueue(Node node)
    {
        if (node is not AStarNode aStarNode)
            throw new ArgumentException("Node must be of type AStarNode", nameof(node));

        if (_bestCost.TryGetValue(aStarNode.State, out var bestG) && bestG <= aStarNode.G)
            return;

        _bestCost[aStarNode.State] = aStarNode.G;

        _queue.Enqueue(aStarNode, aStarNode.F);
        TrackObj();
    }

    public override Node Dequeue() => _queue.Dequeue();
}