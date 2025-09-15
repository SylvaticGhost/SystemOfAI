using Lab2.Containers;
using Lab2.Entities;

namespace Lab2.Algorithms;

public class Algorithm(NodeContainer nodeContainer) : ISolvingAlgorithm
{
    private readonly Solution.MethodFabric _resultFabric = new(nodeContainer.AlgorithmName);

    public Solution Solve(StateProvider stateProvider, CancellationToken cancellationToken = default)
    {
        nodeContainer.SpawnInitNode(stateProvider.GetStartState());

        while (nodeContainer.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var node = nodeContainer.Dequeue();
            
            if (StateProvider.IsFinal(node.State))
                return _resultFabric.Successful(node.GetPath().Select(n => n.State).ToList(), nodeContainer.StateCount);

            List<Vector> nextStates = stateProvider.GetNextStates(node.State);
            foreach (var nextState in nextStates)
                nodeContainer.Enqueue(node.Next(nextState));
        }

        return _resultFabric.Failed("No solution found");
    }

    public string AlgorithmName => nodeContainer.AlgorithmName;
}