using Lab3.Entities;

namespace Lab3.ScoringFunctions;

public interface IScoringFunc
{
    float Score(Player player);
    
    public string Name { get;  }
}