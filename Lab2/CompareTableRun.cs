using Lab2.Algorithms;
using Lab2.Containers;
using Spectre.Console;

namespace Lab2;

public class CompareTableRun
{
    public static void Run()
    {
        int[][] cases =
        [
            [3, 3, 2],
            [4, 4, 3],
            [5, 5, 3],
            [6, 6, 4],
            [7, 7, 4],
            [8, 8, 5],
            [9, 9, 5],
            [10, 10, 6],
            [100, 100, 60],
            [300, 300, 200],
            [500, 500, 300],
            [700, 700, 400],
            [1000, 1000, 600],
        ];

        var results = new List<CaseResults>();
        foreach (var testCase in cases)
        {
            var result = RunCase(testCase).GetAwaiter().GetResult();
            results.Add(result);
            Task.Delay(10).Wait();
        }
        
        DisplayResults(results);
    }
    
    private const int RamLimitMb = 512;
    private const int TimeoutSec = 15;

    private static async Task<CaseResults> RunCase(int[] testCase)
    {
        if (testCase.Length != 3)
            throw new ArgumentException(
                "Test case must have exactly three elements: missionaries, cannibals, boat size");

        int missionaries = testCase[0];
        int cannibals = testCase[1];
        int boatSize = testCase[2];

        var bfsAlgorithm = new Algorithm(new BfsNodeContainer(RamLimitMb));
        var bfsExecutor = new AlgorithmExecutor(bfsAlgorithm);

        var aStarAlgorithm = new Algorithm(new AStarNodeContainer(RamLimitMb));
        var aStarExecutor = new AlgorithmExecutor(aStarAlgorithm);

        var bsfResult = await bfsExecutor.SolveAsync(missionaries, cannibals, boatSize, TimeoutSec, RamLimitMb);
        var aStarResult = await aStarExecutor.SolveAsync(missionaries, cannibals, boatSize, TimeoutSec, RamLimitMb);

        var bfsCaseResult = new CaseResult(bfsAlgorithm.AlgorithmName, bsfResult.Success, bsfResult.Steps.Count,
            bsfResult.StateCount, bsfResult.PerformanceReport?.TimesMs ?? -1,
            bsfResult.PerformanceReport?.RamBytes ?? -1);
        var aStarCaseResult = new CaseResult(aStarAlgorithm.AlgorithmName, aStarResult.Success, aStarResult.Steps.Count,
            aStarResult.StateCount, aStarResult.PerformanceReport?.TimesMs ?? -1,
            aStarResult.PerformanceReport?.RamBytes ?? -1);
        return new CaseResults(missionaries, cannibals, boatSize, bfsCaseResult, aStarCaseResult);
    }

    private record CaseResult(
        string Algorithm,
        bool Success,
        int Steps,
        int StatesExplored,
        int TimeMs,
        long MemoryBytes);

    private record CaseResults(
        int Missionaries,
        int Cannibals,
        int BoatSize,
        CaseResult BfsResult,
        CaseResult AStarResult);

    private static void DisplayResults(List<CaseResults> results)
    {
        var table = new Table();

        table.AddColumn(new TableColumn("[bold]Missionaries[/]"));
        table.AddColumn(new TableColumn("[bold]Cannibals[/]"));
        table.AddColumn(new TableColumn("[bold]BoatSize[/]"));
        table.AddColumn(new TableColumn("[bold]BFS: Success[/]"));
        table.AddColumn(new TableColumn("[bold]BFS: Steps[/]"));
        table.AddColumn(new TableColumn("[bold]BFS: States[/]"));
        table.AddColumn(new TableColumn("[bold]BFS: Time(ms)[/]"));
        table.AddColumn(new TableColumn("[bold]BFS: Memory(bytes)[/]"));
        table.AddColumn(new TableColumn("[bold]A*: Success[/]"));
        table.AddColumn(new TableColumn("[bold]A*: Steps[/]"));
        table.AddColumn(new TableColumn("[bold]A*: States[/]"));
        table.AddColumn(new TableColumn("[bold]A*: Time(ms)[/]"));
        table.AddColumn(new TableColumn("[bold]A*: Memory(bytes)[/]"));

        foreach (var r in results)
        {
            table.AddRow(
                r.Missionaries.ToString(),
                r.Cannibals.ToString(),
                r.BoatSize.ToString(),
                r.BfsResult.Success ? "[green]Yes[/]" : "[red]No[/]",
                r.BfsResult.Steps.ToString(),
                r.BfsResult.StatesExplored.ToString(),
                r.BfsResult.TimeMs.ToString(),
                r.BfsResult.MemoryBytes.ToString(),
                r.AStarResult.Success ? "[green]Yes[/]" : "[red]No[/]",
                r.AStarResult.Steps.ToString(),
                r.AStarResult.StatesExplored.ToString(),
                r.AStarResult.TimeMs.ToString(),
                r.AStarResult.MemoryBytes.ToString()
            );
        }

        AnsiConsole.Write(table);
    }
}