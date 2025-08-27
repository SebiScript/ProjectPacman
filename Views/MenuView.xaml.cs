using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ProjectPacman.Views;

public sealed partial class MenuView : UserControl
{
    private MainWindow _main;

    public MenuView(MainWindow main)
    {
        InitializeComponent();
        _main = main;
    }

    private void Play_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _main.ShowGame();
    }

    private void Exit_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _main.Close();
    }
}