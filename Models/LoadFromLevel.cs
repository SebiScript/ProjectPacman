
namespace ProjectPacman.Models;

public void LoadFromLevel(Level level)
{
    // Supón que LevelMap expone celdas y tipo de celda
    // Cambia estas líneas a cómo se llaman en tu modelo:
    int rows = level.Map.Rows;
    int cols = level.Map.Cols;
    var grid = new int[rows, cols];

    for (int r = 0; r < rows; r++)
    for (int c = 0; c < cols; c++)
    {
        var cell = level.Map[r, c];   // o level.Map.Get(r,c)
        grid[r, c] = cell switch
        {
            CellType.Wall        => 1,
            CellType.Dot         => 2,
            CellType.PowerPellet => 3,
            _                    => 0
        };
    }

    Map = grid;

    // Posición inicial de Pac-Man según tu modelo:
    var start = level.StartPacman; // p.ej. (col, row)
    _pacPosPx = new Point(start.Col * Tile, start.Row * Tile);
}
