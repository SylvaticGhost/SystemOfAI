namespace Lab3.Entities;

public class Board
{
    public Cell[,] Cells { get; init; }
    
    public int XDimension { get; init; }
    public int YDimension { get; init; }
    
    public int TotalCells => XDimension * YDimension;

    public Board(Cell[,] cells, int xDimension, int yDimension)
    {
        Cells = cells;
        XDimension = xDimension;
        YDimension = yDimension;
    }
    
    public int Radius => Math.Min(XDimension, YDimension) / 2;
    
    public int DistanceToCenter(int x, int y)
    {
        var centerX = (XDimension - 1) / 2.0;
        var centerY = (YDimension - 1) / 2.0;
        return (int)(Math.Abs(x - centerX) + Math.Abs(y - centerY));
    }
    
    public List<Cell> GetAvailableNeighbours(int x, int y) => 
        GetNeighbours(x, y).Where(c => !c.IsVisited).ToList();
    
    public List<Cell> GetAvailableNeighbours(int x, int y, IEnumerable<Cell> excluded) => 
        GetNeighbours(x, y).Where(c => !c.IsVisited && !excluded.Contains(c)).ToList();

    public static int DistanceBetweenCells(Cell a, Cell b)
    {
        var dx = Math.Abs(a.X - b.X);
        var dy = Math.Abs(a.Y - b.Y);
        return Math.Max(dx, dy);
    }
    
    public List<Cell> GetAllCellsCanBeVisitedBfs(Player player)
    {
        HashSet<Cell> visited = [];
        Queue<Cell> queue = new();
        
        var startCell = Cells[player.Position.X, player.Position.Y];
        queue.Enqueue(startCell);
        visited.Add(startCell);
        
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            List<Cell> availableNeighbours = GetAvailableNeighbours(current.X, current.Y, visited);
            foreach (var neighbour in availableNeighbours)
            {
                if (!visited.Add(neighbour)) continue;
                queue.Enqueue(neighbour);
            }
        }
        
        visited.Remove(Cells[player.Position.X, player.Position.Y]);
        return visited.ToList();
    }

    private IEnumerable<Cell> GetNeighbours(int x, int y)
    {
        for (var dx = -1; dx <= 1; dx++)
        for (var dy = -1; dy <= 1; dy++)
        {
            var newX = x + dx;
            var newY = y + dy;
            
            if (newX == x && newY == y || !IsInBounds(newX, newY)) continue;
            
            yield return Cells[newX, newY];
        }
    }
    
    private bool IsInBounds(int x, int y) => x >= 0 && x < XDimension && y >= 0 && y < YDimension;

    public static Board Init(int m, int n)
    {
        Cell[,] cells = GenerateCells(m, n);
        return new Board(cells, m, n);
    }
    
    private static Cell[,] GenerateCells(int xDimension, int yDimension)
    {
        var cells = new Cell[xDimension, yDimension];
        for (var x = 0; x < xDimension; x++)
        for (var y = 0; y < yDimension; y++)
            cells[x, y] = new Cell { X = x, Y = y };
        return cells;
    }
    
    public Board Copy()
    {
        var newCells = new Cell[XDimension, YDimension];
        for (var x = 0; x < XDimension; x++)
        for (var y = 0; y < YDimension; y++)
            newCells[x, y] = Cells[x, y].Copy();
        return new Board(newCells, XDimension, YDimension);
    }
}