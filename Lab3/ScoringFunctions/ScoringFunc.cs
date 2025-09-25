using Lab3.Entities;

namespace Lab3.ScoringFunctions;

public abstract class ScoringFunc(Board board) : IScoringFunc
{
    protected int CanBeVisited = 0;
    protected int CanBeVisitedByOpponents = 0;
    
    protected Board Board { get; init; } = board;
    
    public float Score(Player player)
    {
        var opponent = player.Game.GetOpponent(player);
        List<Cell> canBeVisited = Board.GetAllCellsCanBeVisitedBfs(player);
        List<Cell> canBeVisitedByOpponents = Board.GetAllCellsCanBeVisitedBfs(opponent);
        
        CanBeVisited = canBeVisited.Count;
        CanBeVisitedByOpponents = canBeVisitedByOpponents.Count;
        
        if (canBeVisited.Count == 0)
            return float.MinValue;
        
        if (canBeVisitedByOpponents.Count == 0)
            return float.MaxValue;
        
        return InnerScore(player, opponent);
    }

    public abstract string Name { get; }

    protected abstract float InnerScore(Player player, Player opponent);
}