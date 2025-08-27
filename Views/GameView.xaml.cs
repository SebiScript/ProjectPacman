using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ProjectPacman.Views;

using Avalonia.Controls;
using Avalonia.Threading;
using System;

public partial class GameView : UserControl
{
    private MainWindow _main;
    private DispatcherTimer _gameTimer;

    public GameView(MainWindow main)
    {
        InitializeComponent();
        _main = main;

        // Timer del juego
        _gameTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _gameTimer.Tick += GameLoop;
        _gameTimer.Start();
    }

    private void GameLoop(object? sender, EventArgs e)
    {
        GameCanvas.InvalidateVisual();
    }
}