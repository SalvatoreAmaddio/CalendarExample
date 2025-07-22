using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace CalendarExample.ViewModels;

public abstract class AbstractViewModel : INotifyPropertyChanged, IDisposable
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private bool _isLoading = false;

    public bool IsLoading 
    {
        get => _isLoading;
        set 
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public virtual void Dispose()
    {
        PropertyChanged = null;
        GC.SuppressFinalize(this);
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}