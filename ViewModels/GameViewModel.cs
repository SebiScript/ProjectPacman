using System.ComponentModel;
using System.Reactive;
using ReactiveUI;

namespace ProjectPacman.ViewModels;

public class GameViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _main;

    public GameViewModel(MainWindowViewModel main)
    {
        _main = main;
    }
}
