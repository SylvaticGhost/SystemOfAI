using System.Drawing;

namespace Lab3.Entities;

public class Game : IImmutableGame
{
    public Board Board { get; init; }

    public Player[] Players { get; init; }

    private short _currentPlayerIndex = 0;
    
    public Player PreviousPlayer => Players[(_currentPlayerIndex - 1 + Players.Length) % Players.Length];
    public Player CurrentPlayer => Players[_currentPlayerIndex];

    public int Round { get; private set; } = 1;
    
    private Game(Board board, Player[] players)
    {
        Board = board;
        Players = players;
    }

    private Game(short currentPlayerIndex, Board board, Player[] players, int round = 1)
    {
        _currentPlayerIndex = currentPlayerIndex;
        Board = board;
        Players = players;
        Round = round;
    }

    private const short NumberOfPlayers = 2;

    public Player GetPlayer(Color color) =>
        Players.First(p => p.Color == color);
    
    public static Game Init(int m, int n)
    {
        var board = Board.Init(m, n);
        var playerArray = new Player[NumberOfPlayers];
        var game = new Game(board, playerArray);

        (int X, int Y)[] startingPositions = GetStartingPositions(m, n);
        for (short playerN = 0; playerN < NumberOfPlayers; playerN++)
        {
            var playerColor = Player.DefaultColors[playerN];
            var (x, y) = startingPositions[playerN];
            var player = new Player(playerColor, (x, y), game);
            playerArray[playerN] = player;
            board.Cells[x, y].TrackVisit(player);
        }

        return game;
    }

    public Player GetWinner()
    {
        if (!IsOver())
            throw new InvalidOperationException("Game is not over yet.");

        return CurrentPlayer;
    }
    
    public Player GetOpponent(Player player) =>
        Players.First(p => p != player);

    private static (int X, int Y)[] GetStartingPositions(int m, int n) =>
    [
        (0, 0),
        (m - 1, n - 1),
    ];

    public Player GetNextPlayer()
    {
        var player = Players[_currentPlayerIndex];
        _currentPlayerIndex = (short)((_currentPlayerIndex + 1) % Players.Length);
        return player;
    }

    public bool IsOver() => Players.Count(p => p.GetAvailableMoves().Count > 0) <= 1;

    public List<Cell> GetPlayerAvailableMovesForPlayer(short order) =>
        Players[order].GetAvailableMoves();

    public void MovePlayer(Player player, Cell cell)
    {
        if (!player.CanMoveTo(cell))
        {
            Console.WriteLine("inv");
            throw new InvalidOperationException("Player cannot move to the specified cell.");
        }

        cell.TrackVisit(player);
        player.TrackVisit(cell);
        _currentPlayerIndex = (short)((_currentPlayerIndex + 1) % Players.Length);
        if (_currentPlayerIndex == 0)
            Round++;
    }

    public Game Copy()
    {
        var newBoard = Board.Copy();
        var newPlayers = new Player[Players.Length];
        var game = new Game(_currentPlayerIndex, newBoard, newPlayers, Round);
        for (short i = 0; i < Players.Length; i++)
            newPlayers[i] = Players[i].Copy(game);
        return game;
    }
}

public interface IImmutableGame
{
    public Board Board { get; }
    
    Player GetOpponent(Player player);
    Player GetNextPlayer();
    bool IsOver();
    List<Cell> GetPlayerAvailableMovesForPlayer(short order);
    Game Copy();
}