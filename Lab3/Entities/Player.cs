using System.Drawing;

namespace Lab3.Entities;

public record Player
{
    public static readonly Color[] DefaultColors =
    [
        Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Purple, Color.Orange
    ];

    public Player(Color color, (int X, int Y) position, Game game)
    {
        Color = color;
        Position = position;
        Game = game;
    }

    public Color Color { get; init; }
    public (int X, int Y) Position { get; private set; }
    public Game Game { get; init; }
    
    public List<Cell> GetAvailableMoves() => 
        Game.Board.GetAvailableNeighbours(Position.X, Position.Y).Where(CanMoveTo).ToList();
    
    public bool CanMoveTo(Cell cell) => !cell.IsVisited && cell.IsNeighbor(Game.Board.Cells[Position.X, Position.Y]);

    public void TrackVisit(Cell cell) => Position = (cell.X, cell.Y);

    public Player Copy(Game game) => new(Color,(Position.X, Position.Y), game);
    
    public string GetColorHex() => $"#{Color.R:X2}{Color.G:X2}{Color.B:X2}";
    
    public Cell CurrentCell => Game.Board.Cells[Position.X, Position.Y];
}