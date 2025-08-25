using System;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace ProjectPacman.Views;

public partial class MainWindow : Window
{
    private DispatcherTimer _gameTimer;
    public MainWindow()
    {
        InitializeComponent();
    }
}