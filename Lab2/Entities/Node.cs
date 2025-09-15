namespace Lab2.Entities;

public record Node(Vector State, Node? Parent = null)
{
    public List<Node> GetPath()
    {
        List<Node> path = [];
        var current = this;
        while (current != null)
        {
            path.Add(current);
            current = current.Parent;
        }
        path.Reverse();
        return path;
    }

    public override int GetHashCode() => HashCode.Combine(State, Parent);
    
    public virtual Node Next(Vector nextState) => this with { State = nextState, Parent = this };
}