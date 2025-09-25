using Lab3.Entities;

namespace Lab3.ScoringFunctions;

public class ScoringMaxMyVarianceFunc(Board board) : ScoringFunc(board)
{
    public override string Name => nameof(ScoringMaxMyVarianceFunc);
    protected override float InnerScore(Player player, Player opponent) => CanBeVisited;
}