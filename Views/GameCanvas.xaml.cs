using System;
using System.Windows;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ProjectPacman.Models;
using Avalonia.Media;
using Point = Avalonia.Point;


// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ProjectPacman.Views;

public sealed partial class GameCanvas : UserControl
{
    private LevelMap _level;
    private int _score = 0;

    private const float SpriteSize = 32f;   
    private const float CollisionRadius = 12f;
    
    private readonly DispatcherTimer _gameLoop;
    private DateTime _lastUpdate;
    private Dictionary<string, Bitmap[]> _pacmanSprites;
    private int _frameIndex = 0;
    private double _frameTimer = 0;
    private double _frameInterval = 0.1; // 100ms por frame
    private string _currentDirection = "right";

    private Vector2 _direction = Vector2.Zero;
    private float _playerX = 100;
    private float _playerY = 100;
    private float _speed = 80;

    

    public GameCanvas()
    {
        InitializeComponent();

        _gameLoop = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(20) 
        };

        _gameLoop.Tick += (_, __) =>
        {
            var now = DateTime.Now;
            var deltaTime = (now - _lastUpdate).TotalSeconds;
            _lastUpdate = now;

            Update(deltaTime);
            InvalidateVisual();
        };

        _lastUpdate = DateTime.Now;
        _gameLoop.Start();
        
        Focusable = true;
        this.AttachedToVisualTree += (_, __) => Focus();
        
        LoadPacmanSprites();
        Focusable = true;
        Focus();
        _level = LevelMap.FromAscii(Levels.Level1);
    }

    private void Update(double deltaTime)
    {
        float nextX = _playerX + _direction.X * _speed * (float)deltaTime;
        float nextY = _playerY + _direction.Y * _speed * (float)deltaTime;

        // Si el siguiente paso no es pared, actualiza la posición
        if (CanMove(nextX + SpriteSize / 2, nextY + SpriteSize / 2))
        {
            _playerX = nextX;
            _playerY = nextY;
        }

        // Animación
        if (_direction != Vector2.Zero)
        {
            _frameTimer += deltaTime;
            if (_frameTimer >= _frameInterval)
            {
                _frameTimer = 0;
                _frameIndex = (_frameIndex + 1) % 3;
            }
        }
        else
        {
            _frameIndex = 1; // boca cerrada cuando está quieto
        }

        InvalidateVisual(); // repinta
    }


    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        switch (e.Key)
        {
            case Key.Up:    _direction = new Vector2(0, -1); _currentDirection = "up"; break;
            case Key.Down:  _direction = new Vector2(0, 1);  _currentDirection = "down"; break;
            case Key.Left:  _direction = new Vector2(-1, 0); _currentDirection = "left"; break;
            case Key.Right: _direction = new Vector2(1, 0);  _currentDirection = "right"; break;
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        _direction = Vector2.Zero; 
    }

    public override void Render(Avalonia.Media.DrawingContext context)
    {
        // Fondo
        context.FillRectangle(Brushes.Black, new Rect(0, 0, Bounds.Width, Bounds.Height));

        // Tiles
        for (int r = 0; r < _level.Rows; r++)
        {
            for (int c = 0; c < _level.Cols; c++)
            {
                var t = _level[r, c];
                double x = c * LevelMap.TileSize;
                double y = r * LevelMap.TileSize;

                if (t == Tile.Wall)
                {
                    context.FillRectangle(Brushes.DarkBlue, new Rect(x, y, LevelMap.TileSize, LevelMap.TileSize));
                }
                else if (t == Tile.Pellet)
                {
                    // Pellet 6x6 -> radios 3, centrado en el tile
                    var cx = x + LevelMap.TileSize / 2.0;
                    var cy = y + LevelMap.TileSize / 2.0;
                    context.DrawEllipse(Brushes.Gold, pen: null, center: new Point(cx, cy), radiusX: 3, radiusY: 3);
                }
                else if (t == Tile.Power)
                {
                    // Power-up 16x16 -> radios 8
                    var cx = x + LevelMap.TileSize / 2.0;
                    var cy = y + LevelMap.TileSize / 2.0;
                    context.DrawEllipse(Brushes.Gold, pen: null, center: new Point(cx, cy), radiusX: 8, radiusY: 8);
                }
            }
        }

        // Pacman
        if (_pacmanSprites != null && _pacmanSprites.TryGetValue(_currentDirection, out var sprites))
        {
            var sprite = sprites[_frameIndex];
            var sourceRect = new Rect(0, 0, sprite.PixelSize.Width, sprite.PixelSize.Height);
            var destRect = new Rect(_playerX, _playerY, 32, 32);
            context.DrawImage(sprite, sourceRect, destRect);
        }

        // Si hay hijos en el UserControl, se dibujan encima
        base.Render(context);
    }

    
    private void LoadPacmanSprites()
    {
        _pacmanSprites = new Dictionary<string, Bitmap[]>();

        string[] directions = { "up", "down", "left", "right" };

        foreach (var dir in directions)
        {
            _pacmanSprites[dir] = new Bitmap[3];
            for (int i = 1; i <= 3; i++)
            {
                var uri = new Uri($"avares://ProjectPacman/Assets/Pacman/pacman-{dir}/{i}.png");
                _pacmanSprites[dir][i - 1] = new Bitmap(AssetLoader.Open(uri));
            }
        }
    }
    private bool CanMove(float nextX, float nextY)
    {
        // Tile actual
        int col = (int)(nextX / LevelMap.TileSize);
        int row = (int)(nextY / LevelMap.TileSize);

        // Si está fuera del mapa
        if (row < 0 || row >= _level.Rows || col < 0 || col >= _level.Cols)
            return false;

        // Si es pared, no puede moverse
        return _level[row, col] != Tile.Wall;
    }

}