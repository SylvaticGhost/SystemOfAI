using Lab2.Entities;
using Spectre.Console;

namespace Lab2.Utils;

public class ResultDisplayer(int m, int c, int boatSize)
{
    public void DisplayResult(Solution solution)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[blue bold] {solution.Method} Solution [/]");
        if (solution.Success)
        {
            DisplaySuccess(solution);
            if (solution.PerformanceReport != null)
                DisplayPerformanceReport(solution.PerformanceReport, solution.StateCount);
        }  
        else
        {
            DisplayFailure(solution.ErrorMessage ?? "Unknown error");
        }
    }

    private void DisplaySuccess(Solution solution)
    {
        var table = new Table();
        table.Border = TableBorder.Rounded;
        table.AddColumn("[bold]Step[/]");
        table.AddColumn("M(L)");
        table.AddColumn("C(L)");
        table.AddColumn("Boat");
        table.AddColumn("M(R)");
        table.AddColumn("C(R)");

        for (int step = 0; step < solution.Steps.Count; step++)
        {
            var s = solution.Steps[step];
            int mL = s.LeftMissionaries;
            int cL = s.LeftCannibals;
            int mR = m - mL;
            int cR = c - cL;
            string boat = s.BoatSide == 0 ? "L" : "R"; 

            table.AddRow(
                $"[yellow]{step}[/]",
                mL.ToString(),
                cL.ToString(),
                boat,
                mR.ToString(),
                cR.ToString()
            );
        }
        
        AnsiConsole.Write(table);
    }

    private static void DisplayPerformanceReport(PerformanceReport report, int stateCount)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold underline]Performance Report[/]");
        AnsiConsole.MarkupLine($"[green]Time Taken:[/] {report.TimesMs} ms");
        AnsiConsole.MarkupLine($"[green]Memory Used:[/] {report.RamBytes / (1024.0 * 1024.0):F4} MB");
        AnsiConsole.MarkupLine($"[green]States Explored:[/] {stateCount}");
    }
    
    private static void DisplayFailure(string errorMessage)
    {
        AnsiConsole.MarkupLine($"[red]Error:[/] {errorMessage}");
    }
}