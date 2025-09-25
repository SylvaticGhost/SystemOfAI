using System.Collections.Concurrent;
using Lab3.Entities;
using Lab3.ScoringFunctions;
using Lab3.Utils;
using Spectre.Console;

namespace Lab3;

public static class MethodComparer
{
    private static readonly Func<Board, IScoringFunc>[] ScoringFuncConstructors =
    [
        board => new ScoringMaxMyVarianceFunc(board),
        board => new ScoringMyVarianceCenteredFunc(board),
        board => new ScoringOpponentCapturingFunc(board),
        board => new ScoringFuncAggressive(board)
    ];

    private const int M = 9;
    private const int N = 7;

    private record GameReport(string ScoringMethod1, string ScoringMethod2, int Rounds, int Depth, string Winner, string WinnerMethod);

    public static void Run()
    {
        ConcurrentBag<GameReport> reports = [];

        for (int p1 = 0; p1 < ScoringFuncConstructors.Length; p1++)
        for (int p2 = 0; p2 < ScoringFuncConstructors.Length; p2++)
        for (int d = 3; d <= 5; d++)
        {
            var game = Game.Init(M, N);
            var player1 = game.CurrentPlayer;
            var player1AlgFunc = ScoringFuncConstructors[p1](game.Board);
            var player1Alg = new MiniMaxAlgorithm(game, player1, d, player1AlgFunc);

            var player2 = game.GetOpponent(player1);
            var player2AlgFunc = ScoringFuncConstructors[p2](game.Board);
            var player2Alg = new MiniMaxAlgorithm(game, player2, d, player2AlgFunc);

            var onlyResultPrinter =
                new OnlyResultPrinter(game, d, player1AlgFunc.Name, player2AlgFunc.Name);

            var host = new GameHost(player1Alg, player2Alg, onlyResultPrinter);

            host.RunAsync(game).Wait();

            var winner = game.GetWinner();
            var winnerMethod = winner == player1 ? player1AlgFunc.Name : player2AlgFunc.Name;
            var gameReport = new GameReport(player1AlgFunc.Name, player2AlgFunc.Name, game.Round, d,
                winner.GetColorHex(), winnerMethod);
            reports.Add(gameReport);
            
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
        }


        PrintResultTable(reports);
        AnsiConsole.WriteLine();
        
        var methodScores = MethodScores(reports);
        PrintMethodScores(methodScores);
    }

    private static void PrintResultTable(IEnumerable<GameReport> reports)
    {
        reports = reports.OrderBy(r => r.Depth).ThenBy(r => r.ScoringMethod1).ThenBy(r => r.ScoringMethod2);
        
        var table = new Table();
        table.AddColumn("Depth");
        table.AddColumn("[red]Method 1[/]");
        table.AddColumn("[blue]Method 2[/]");
        table.AddColumn("Rounds");
        table.AddColumn("Winner");

        foreach (var report in reports)
        {
            var depthStr = $"[{DepthColor.GetValueOrDefault(report.Depth, "white")}] {report.Depth} [/]";
            var winnerStr = $"[#{report.Winner}]██[/]";
            table.AddRow(depthStr, report.ScoringMethod1, report.ScoringMethod2,
                report.Rounds.ToString(), winnerStr);
        }

        AnsiConsole.Write(table);
    }

    private static readonly Dictionary<int, string> DepthColor = new()
    {
        [1] = "green",
        [2] = "yellow",
        [3] = "orange1",
        [4] = "purple",
        [5] = "red",
    };

    private class OnlyResultPrinter(Game game, int d, string method1, string method2) : IGamePrinter
    {
        private readonly GamePrinter _basePrinter = new(game);

        public Task PrintAsync() => Task.CompletedTask;

        public void PrintWinner()
        {
            AnsiConsole.MarkupLine($"[yellow]Depth {d}[/], [green]{method1}[/] vs [blue]{method2}[/]");
            _basePrinter.PrintAsync().Wait();
            _basePrinter.PrintWinner();
        }
    }
    
    private record MethodScore(string Method, int Wins, int Total, float WinRate);
    
    private static IEnumerable<MethodScore> MethodScores(IEnumerable<GameReport> gameReports) =>
        gameReports
            .Where(r => r.ScoringMethod1 != r.ScoringMethod2)
            .SelectMany(r => new []
        {
            new { Player = r.ScoringMethod1, Opponent = r.ScoringMethod2, Winner = r.WinnerMethod, IsFirst = true },
                new { Player = r.ScoringMethod2, Opponent = r.ScoringMethod1, Winner = r.WinnerMethod, IsFirst = false }
        })
        .GroupBy(x => x.Player)
        .Select(g =>
        {
            int wins = g.Count(x => x.Winner == x.Player);
            int total = g.Count();
            return new MethodScore(g.Key, wins, total, (float)wins / total);
        })
        .OrderByDescending(r => r.WinRate)
        .ToList();

    private static void PrintMethodScores(IEnumerable<MethodScore> gameReports)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Scoring Function")
            .AddColumn("Wins")
            .AddColumn("Games")
            .AddColumn("Win Rate");

        foreach (var r in gameReports)
        {
            table.AddRow(
                r.Method,
                r.Wins.ToString(),
                r.Total.ToString(),
                $"{r.WinRate:P1}"
            );
        }

        AnsiConsole.Write(table);
    }
}