namespace Lab2.Entities;

public record AStarNode(Vector State, Node? Parent = null, int G = 0) : Node(State, Parent)
{
    private static float Heuristic(Vector state) => state.LeftMissionaries + state.LeftCannibals;
    
    public override int GetHashCode() => HashCode.Combine(State, Parent, G);

    public float F => G + Heuristic(State);
    
    public override Node Next(Vector nextState) => this with { State = nextState, Parent = this, G = G + 1 };
}