using Lab3.Entities;

namespace Lab3.ScoringFunctions;

public class ScoringOpponentCapturingFunc(Board board) : ScoringFunc(board)
{
    public override string Name => nameof(ScoringOpponentCapturingFunc);

    protected override float InnerScore(Player player, Player opponent)
    {
        var distance = Board.DistanceBetweenCells(player.CurrentCell, opponent.CurrentCell);
        return (Board.TotalCells - CanBeVisitedByOpponents)*0.6f + (Board.Radius*2 - distance)*0.6f;
    }
}