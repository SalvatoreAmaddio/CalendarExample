using CalendarExample.Models;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Input;

namespace CalendarExample.ViewModels;

public class EventViewModel : AbstractViewModel
{
    public event EventHandler<bool>? Saved;
    public EventModel Record { get; set; }
    public ICommand SaveCommand => new AsyncRelayCommand(SaveAsync);

    public EventViewModel(EventModel record)
    {
        Record = record;
    }

    private async Task SaveAsync()
    {

        if (!Record.AllowUpdate())
        {
            MessageBox.Show("Ensure all mandatory fields are filled.", "Missing fields", MessageBoxButton.OK, MessageBoxImage.Stop);
            return;
        }

        bool saved;

        if (Record.Id == 0)
        {
            int id = await DatabaseManager.InsertEventAsync(Record);
            Record.Id = id;
            saved = true;
        }
        else
        {
            saved = await DatabaseManager.UpdateEventAsync(Record);
        }

        Saved?.Invoke(this, saved);
    }

    public override void Dispose()
    {
        base.Dispose();
        Saved = null;
        GC.SuppressFinalize(this);
    }
}