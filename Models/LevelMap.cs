using System;
using System.Linq;

namespace ProjectPacman.Models;

public sealed class LevelMap
{
    public const int TileSize = 32;

    private readonly Tile[,] _grid;
    public int Rows => _grid.GetLength(0);
    public int Cols => _grid.GetLength(1);

    public LevelMap(Tile[,] grid) => _grid = grid;

    public Tile this[int r, int c] =>
        (r >= 0 && r < Rows && c >= 0 && c < Cols) ? _grid[r, c] : Tile.Wall;

    public static LevelMap FromAscii(string[] lines)
    {
        int rows = lines.Length;
        int cols = lines.Max(l => l.Length);
        var grid = new Tile[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            string line = lines[r];
            for (int c = 0; c < cols; c++)
            {
                char ch = c < line.Length ? line[c] : ' ';
                grid[r, c] = ch switch
                {
                    '#' => Tile.Wall,
                    '.' => Tile.Pellet,
                    'o' => Tile.Power,
                    '-' => Tile.Gate,
                    _ => Tile.Empty
                };
            }
        }

        return new LevelMap(grid);
    }

    public bool IsWallAtPixel(float px, float py)
    {
        int c = (int)Math.Floor(px / TileSize);
        int r = (int)Math.Floor(py / TileSize);
        var t = this[r, c];
        return t == Tile.Wall || t == Tile.Gate;
    }

    public bool ConsumePelletAtPixel(float px, float py, out Tile consumed)
    {
        int c = (int)Math.Floor(px / TileSize);
        int r = (int)Math.Floor(py / TileSize);
        consumed = this[r, c];
        if (consumed == Tile.Pellet || consumed == Tile.Power)
        {
            _grid[r, c] = Tile.Empty;
            return true;
        }

        return false;
    }

    public Tile GetTileAtPixel(float px, float py)
    {
        int c = (int)Math.Floor(px / TileSize);
        int r = (int)Math.Floor(py / TileSize);
        return this[r, c];
    }
}