using System.Drawing;
using Lab3.Entities;
using Lab3.ScoringFunctions;

namespace Lab3;

public class MiniMaxAlgorithm(IImmutableGame game, Player player, int depth, IScoringFunc scoringFunc) : IPlayerInput
{
    public async Task<Cell?> GetMoveAsync() => 
        await Task.Run(GetMove);

    public Player Player { get; init; } = player;

    public Cell? GetMove()
    {
        Cell? bestMove = null;
        var bestValue = float.NegativeInfinity;
        
        List<Cell> availableMoves = Player.GetAvailableMoves();

        foreach (var move in availableMoves)
        {
            var newState = game.Copy();
            var newPlayer = newState.GetPlayer(Player.Color);
            var currentMove = newState.Board.Cells[move.X, move.Y];
            
            newState.MovePlayer(newPlayer, currentMove);
            
            var val = MinValue(newState,newState.GetOpponent(newPlayer), depth - 1);
            
            if (val > bestValue)
            {
                bestValue = val;
                bestMove = currentMove;
            }
        }
        
        if (bestMove == null)
            return null;
        
        return game.Board.Cells[bestMove.X, bestMove.Y];
    }

    private float MinValue(Game state, Player current, int d) =>
        MinMaxValue(state, current, d, float.PositiveInfinity, float.NegativeInfinity,
            Math.Min, Math.Max);

    private float MinMaxValue(Game state, Player current, int d, float startingValue, float oppositeStartingValue,
        Func<float, float, float> minMaxFunc, Func<float, float, float> oppositeFunc)
    {
        if (d == 0 || state.IsOver())
            return scoringFunc.Score(current);

        var bestValue = startingValue;

        List<Cell> availableMoves = current.GetAvailableMoves();
        
        foreach (var move in availableMoves)
        {
            Game newState = state.Copy();
            var newPlayer = newState.GetPlayer(current.Color);
            
            if (newPlayer.Color != current.Color)
                throw new InvalidOperationException("Player color mismatch after copying the game state.");
            
            if (newPlayer.Position != current.Position)
                throw new InvalidOperationException("Player position mismatch after copying the game state.");
            
            var currentMove = newState.Board.Cells[move.X, move.Y];
            
            if (currentMove.X != move.X || currentMove.Y != move.Y)
                throw new InvalidOperationException("Cell position mismatch after copying the game state.");
            
            if (newState.IsOver())
                return scoringFunc.Score(newPlayer);
            
            newState.MovePlayer(newPlayer, currentMove);

            var opponentVal = MinMaxValue(newState, newState.GetOpponent(newPlayer), d - 1,
                oppositeStartingValue, startingValue, oppositeFunc, minMaxFunc);
            bestValue = minMaxFunc(bestValue, opponentVal);
        }

        return bestValue;
    }
}