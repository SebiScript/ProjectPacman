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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ProjectPacman.Views;

public sealed partial class GameCanvas : UserControl
{
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
    }

    private void Update(double deltaTime)
    {
        // Movimiento
        _playerX += _direction.X * _speed * (float)deltaTime;
        _playerY += _direction.Y * _speed * (float)deltaTime;

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

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        // Dibuja el fondo negro
        context.FillRectangle(Brushes.Black, new Rect(0, 0, Bounds.Width, Bounds.Height));

        // Si los sprites están cargados y la dirección existe
        if (_pacmanSprites != null && _pacmanSprites.ContainsKey(_currentDirection))
        {
            var sprite = _pacmanSprites[_currentDirection][_frameIndex];
        
            // Define el área de origen (toda la imagen)
            var sourceRect = new Rect(0, 0, sprite.PixelSize.Width, sprite.PixelSize.Height);
        
            // Define el área de destino (donde se dibujará el sprite en el canvas)
            var destRect = new Rect(_playerX, _playerY, 32, 32);

            // Dibuja la imagen
            context.DrawImage(
                source: sprite,
                sourceRect: sourceRect,
                destRect: destRect
            );
        }
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
}