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
    
    private double _playerX = 100;
    private double _playerY = 100;
    private double _speed = 100; 
    private Vector2 _direction = new Vector2(0,0);

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
    }

    private void Update(double deltaTime)
    {
        _playerX += _direction.X * _speed * deltaTime;
        _playerY += _direction.Y * _speed * deltaTime;
    }
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        switch (e.Key)
        {
            case Key.Up: _direction = new Vector2(0, -1); break;
            case Key.Down: _direction = new Vector2(0, 1); break;
            case Key.Left: _direction = new Vector2(-1, 0); break;
            case Key.Right: _direction = new Vector2(1, 0); break;
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

        // Fondo negro
        context.FillRectangle(Brushes.Black, new Avalonia.Rect(0, 0, Bounds.Width, Bounds.Height));

        // 🔹 Usar la fuente embebida
        // OJO: el "#Pacman Arcade" debe ser el nombre real de la familia tipográfica dentro del TTF
        var FontFamily = new FontFamily("avares://ProjectPacman/Assets/Fonts/Pacman.ttf#PacFont");
        var typeface = new Typeface(FontFamily);

        var textLayout = new TextLayout(
            "Loop corriendo...",
            typeface,
            32,               // tamaño
            Brushes.Yellow    // color
        );

        textLayout.Draw(context, new Avalonia.Point(20, 20));
    }
}