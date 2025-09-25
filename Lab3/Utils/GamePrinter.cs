using Lab3.Entities;
using Spectre.Console;

namespace Lab3.Utils;

public class GamePrinter(Game game, GamePrinter.PrintMode mode = GamePrinter.PrintMode.Inline) : IGamePrinter
{
    public enum PrintMode
    {
        Interactive,
        Inline
    }
    
    public async Task PrintAsync()
    {
        var table = new Table();
        table.Border = TableBorder.Rounded;
        
        table.AddColumn(new TableColumn("[grey]x/y[/]").Centered());
        for (int y = 0; y < game.Board.YDimension; y++)
            table.AddColumn(new TableColumn($"[grey]{y}[/]").Centered());
        
        for (int x = 0; x < game.Board.XDimension; x++)
        {
            List<string> row = [$"[grey]{x}[/]"];
            for (int y = 0; y < game.Board.YDimension; y++)
            {
                var cell = game.Board.Cells[x, y];
                string cellText;

                if (cell.VisitedBy is not null)
                {
                    var player = cell.VisitedBy;
                    var colorHex = $"#{player.Color.R:X2}{player.Color.G:X2}{player.Color.B:X2}";
                    
                    bool lastCell = player.Position == (x, y);
                    
                    if (lastCell)
                        cellText = $"[bold {colorHex}]>[/]";
                    else
                        cellText = $"[bold {colorHex}]█[/]";
                }
                else
                {
                    cellText = "[grey]█[/]";
                }

                row.Add(cellText);
            }
            table.AddRow(row.ToArray());
        }

        var currentPlayer = game.CurrentPlayer;
        
        if (mode == PrintMode.Interactive)
            AnsiConsole.Clear();
        
        AnsiConsole.MarkupLine($"Round {game.Round}, Current Player: [#{currentPlayer.Color.R:X2}{currentPlayer.Color.G:X2}{currentPlayer.Color.B:X2}]>[/]");

        AnsiConsole.Write(table);

        if (mode == PrintMode.Interactive)
            await Task.Delay(50);
    }
    
    public void PrintWinner()
    {
        var winner = game.GetWinner();
        var colorHex = $"#{winner.Color.R:X2}{winner.Color.G:X2}{winner.Color.B:X2}";
        AnsiConsole.MarkupLine($"[bold {colorHex}]Player[/] wins!");
    }
}