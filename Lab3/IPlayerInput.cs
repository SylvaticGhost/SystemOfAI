using Lab3.Entities;

namespace Lab3;

public interface IPlayerInput
{
    public Task<Cell?> GetMoveAsync();
    
    public Player Player { get; init; }
}