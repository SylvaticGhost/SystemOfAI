using Lab2.Entities;

namespace Lab2.Algorithms;

public class StateProvider(int m = 3, int c = 3, int boatSize = 2)
{
    public Vector GetStartState() => new(m, c, 0);

    private List<Vector> GetStateChangers(int boatSide)
    {
        if (boatSide is not (0 or 1))
            throw new ArgumentOutOfRangeException(nameof(boatSide), "boatSide must be 0 or 1");

        var direction = boatSide == 0 ? 1 : -1;
        List<Vector> list = [];

        var maxChange = int.Min(int.Min(m, c), boatSize);

        for (var i = 0; i <= maxChange; i++)
        for (var j = 0; j <= maxChange - i; j++)
            if (i + j > 0)
                list.Add(new Vector(-i * direction, -j * direction, direction));

        return list;
    }

    private readonly Dictionary<int, List<Vector>> _stateChangers = new();

    private List<Vector> GetCachedStateChangers(int p)
    {
        if (!_stateChangers.ContainsKey(p))
            _stateChangers[p] = GetStateChangers(p);
        return _stateChangers[p];
    }

    public List<Vector> GetNextStates(Vector currentState)
    {
        List<Vector> availableChangers = GetCachedStateChangers(currentState.BoatSide);
        IEnumerable<Vector> generatedStates = availableChangers.Select(changer => currentState + changer);
        return generatedStates.Where(IsValid).ToList();
    }

    private bool IsValid(Vector state)
    {
        if (state.BoatSide is not (0 or 1))
            return false;

        return (state.LeftMissionaries == 0 || state.LeftMissionaries >= state.LeftCannibals) &&
               (m - state.LeftMissionaries == 0 || m - state.LeftMissionaries >= c - state.LeftCannibals) &&
               state is { LeftMissionaries: >= 0, LeftCannibals: >= 0 } && state.LeftMissionaries <= m &&
               state.LeftCannibals <= c;
    }

    public static bool IsFinal(Vector state) => state is { LeftMissionaries: 0, LeftCannibals: 0, BoatSide: 1 };
}