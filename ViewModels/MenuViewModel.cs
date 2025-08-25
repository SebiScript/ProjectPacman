using System.Reactive;
using ReactiveUI;

namespace ProjectPacman.ViewModels;

public class MenuViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> StartGameCommand { get; }

    public MenuViewModel(MainWindowViewModel main)
    {
        StartGameCommand = ReactiveCommand.Create(() => { main.CurrentView = new GameViewModel(main); });
    }
    
}