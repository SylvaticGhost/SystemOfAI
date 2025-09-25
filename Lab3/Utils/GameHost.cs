using Lab3.Entities;

namespace Lab3.Utils;

public class GameHost(IPlayerInput playerInput1, IPlayerInput playerInput2, IGamePrinter printer)
{
    public async Task RunAsync(Game game)
    {
        var currentPlayer = playerInput1;
        
        await printer.PrintAsync();
        
        while (!game.IsOver()) 
        {
            var move = await currentPlayer.GetMoveAsync();

            if (move is null)
                break;
            
            if (move is null || !currentPlayer.Player.CanMoveTo(move))
            {
                Console.WriteLine("inv");
                throw new InvalidOperationException("Invalid move");
            }
            
            game.MovePlayer(currentPlayer.Player, move);
            currentPlayer = currentPlayer == playerInput1 ? playerInput2 : playerInput1;
            
            await printer.PrintAsync();
        }
        
        printer.PrintWinner();
    }
}