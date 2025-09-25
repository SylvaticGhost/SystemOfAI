using Lab3.Entities;

namespace Lab3.ScoringFunctions;

public class ScoringMyVarianceCenteredFunc(Board board) : ScoringFunc(board)
{
    protected override float InnerScore(Player player, Player opponent) => 
        CanBeVisited + 0.5f * (Board.Radius - Board.DistanceToCenter(player.Position.X, player.Position.Y));

    public override string Name => nameof(ScoringMyVarianceCenteredFunc);
}
