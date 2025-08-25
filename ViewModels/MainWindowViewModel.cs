using System.ComponentModel;
using ReactiveUI;
namespace ProjectPacman.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IReactiveObject
{
    private ViewModelBase _currentView;

    public ViewModelBase CurrentView
    {
        get => _currentView;
        set => this.RaiseAndSetIfChanged(ref _currentView, value);
    }

    public MainWindowViewModel()
    {
        // Empieza en el menú
        CurrentView = new MenuViewModel(this);
    }

    public void RaisePropertyChanging(PropertyChangingEventArgs args)
    {
        throw new System.NotImplementedException();
    }

    public void RaisePropertyChanged(PropertyChangedEventArgs args)
    {
        throw new System.NotImplementedException();
    }
}
