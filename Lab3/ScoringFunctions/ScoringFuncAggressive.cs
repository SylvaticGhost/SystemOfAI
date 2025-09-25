using Lab3.Entities;

namespace Lab3.ScoringFunctions;

public class ScoringFuncAggressive(Board board) : ScoringFunc(board)
{
    public override string Name => nameof(ScoringFuncAggressive);

    protected override float InnerScore(Player player, Player opponent)
    {
        var distance = Board.DistanceBetweenCells(player.CurrentCell, opponent.CurrentCell);

        float aggressionScore = Board.Radius * 2 - distance;
        float mobilityScore = CanBeVisited - 0.5f * CanBeVisitedByOpponents;
        
        return 0.6f * aggressionScore + 0.4f * mobilityScore;
    }
}