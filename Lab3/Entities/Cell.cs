namespace Lab3.Entities;

public record Cell
{
    public int X { get; init; }
    public int Y { get; init; }
    
    public Player? VisitedBy { get; private set; }
    public bool IsVisited => VisitedBy is not null;
    
    public void TrackVisit(Player player)
    {
        if (IsVisited)
            throw new InvalidOperationException("Cell is already visited.");
        
        VisitedBy = player;
    }
    
    public Cell Copy() => new() { X = X, Y = Y, VisitedBy = VisitedBy };
    
    public bool IsNeighbor(Cell other) =>
        (Math.Abs(X - other.X) <= 1) && (Math.Abs(Y - other.Y) <= 1) && !(X == other.X && Y == other.Y);
}