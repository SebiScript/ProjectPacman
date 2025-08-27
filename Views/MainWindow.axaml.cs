using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;

namespace ProjectPacman.Views;

public partial class MainWindow : Window
{
    private DispatcherTimer _gameTimer;

    public MainWindow()
    {
        InitializeComponent();
        ShowMenu();
    }
    
    public void ShowMenu()
    {
        MainContent.Content = new MenuView(this);
    }

    public void ShowGame()
    {
        MainContent.Content = new GameView(this);
    }
    
}