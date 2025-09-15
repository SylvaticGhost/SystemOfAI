using System.Diagnostics;
using Lab2.Entities;
using Lab2.Utils;

namespace Lab2.Algorithms;

public class AlgorithmExecutor(ISolvingAlgorithm solvingAlgorithm)
{
    private readonly SolvingAlgorithmAsyncAdapter _algorithm = new(solvingAlgorithm, solvingAlgorithm.AlgorithmName);
    
    public async Task Run(int m, int c, int boatSize, int timeoutSec = 30, int ramLimitMb = 512)
    {
        var solution = await SolveAsync(m, c, boatSize, timeoutSec, ramLimitMb);
        
        var displayer = new ResultDisplayer(m, c, boatSize);
        displayer.DisplayResult(solution);
    }
    
    public async Task<Solution> SolveAsync(int m, int c, int boatSize, int timeoutSec = 30, int ramLimitMb = 512)
    {
        var stateProvider = new StateProvider(m, c, boatSize);
        
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSec));
        var cancellationToken = cts.Token;
        return await _algorithm.SolveAsync(stateProvider, cancellationToken);
    }
    
    private class SolvingAlgorithmAsyncAdapter(ISolvingAlgorithm inner, string algorithmName)
    {
        public async Task<Solution> SolveAsync(StateProvider stateProvider, CancellationToken cancellationToken)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            long memoryBefore = GC.GetTotalMemory(true);
            var stopWatch = Stopwatch.StartNew();
            
            Solution result;
            try
            {
                result = await Task.Run(() => inner.Solve(stateProvider, cancellationToken), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                result = Solution.Failed(algorithmName,"Operation timed out");
            }
            catch (OutOfMemoryException)
            {
                result = Solution.Failed(algorithmName, "Memory limit exceeded");
            }
            
            stopWatch.Stop();
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            long memoryAfter = GC.GetTotalMemory(true);
            
            var performanceReport = new PerformanceReport((int)stopWatch.ElapsedMilliseconds, memoryAfter - memoryBefore);
            result.PerformanceReport = performanceReport;
            
            return result;
        }
    }

}