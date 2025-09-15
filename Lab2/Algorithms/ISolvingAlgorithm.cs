using Lab2.Entities;

namespace Lab2.Algorithms;

public interface ISolvingAlgorithm
{
    public Solution Solve(StateProvider stateProvider, CancellationToken cancellationToken = default);
    
    public string AlgorithmName { get;  }
}