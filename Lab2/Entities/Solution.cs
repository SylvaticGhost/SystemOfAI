namespace Lab2.Entities;

public record Solution(string Method, bool Success, List<Vector> Steps, int StateCount, string? ErrorMessage)
{
    public static Solution Successful(string method, List<Vector> steps, int stateCount) =>
        new(method, true, steps, stateCount, null);

    public static Solution Failed(string method, string errorMessage, int stateCount = 0) =>
        new(method, false, [], stateCount, errorMessage);

    public PerformanceReport? PerformanceReport { get; set; }

    public class MethodFabric(string method)
    {
        public Solution Successful(List<Vector> steps, int stateCount) => Solution.Successful(method, steps, stateCount);
        public Solution Failed(string errorMessage) => Solution.Failed(method, errorMessage);
    }
}