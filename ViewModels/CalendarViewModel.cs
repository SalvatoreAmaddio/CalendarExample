using CalendarControl;
using CalendarExample.Models;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CalendarExample.ViewModels;

public class CalendarViewModel : AbstractViewModel
{
    public event EventHandler<EventArgs>? RefreshRequest;
    public event EventHandler<object>? OpenView;

    #region backing fields
    private bool _isWeekView = false;
    private DateTime _date = DateTime.Today;
    #endregion

    public DateTime Date
    {
        get => _date;
        set
        {
            _date = value;
            OnPropertyChanged();
            RefreshRequest?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsWeekView
    {
        get => _isWeekView;
        set
        {
            _isWeekView = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<IDatable> Events { get; set; } = [];

    public ICommand AddEventCommand => new RelayCommand<object?>(AddEvent);
    public ICommand DeleteCommand => new AsyncRelayCommand<IDatable>(DeleteAsync);
    public ICommand EventDropCommand => new AsyncRelayCommand<IDatable>(EventDropAsync);
    public ICommand SelectedEventCommand => new AsyncRelayCommand<IDatable?>(SelectedEventAsync);

    public CalendarViewModel()
    {
        RefreshRequest += ViewModel_ViewChanged;
    }

    private async void ViewModel_ViewChanged(object? sender, EventArgs args)
    {
        await GetDataAsync();
    }

    public async Task GetDataAsync()
    {
        IsLoading = true;   

        if (IsWeekView)
        {           
            Events = [.. await Task.Run(()=>DatabaseManager.GetEventsByWeekAsync(Date))];
        }
        else
        {
            Events = [.. await Task.Run(()=>DatabaseManager.GetByMonthWithFullWeeksAsync(Date))];
        }

        OnPropertyChanged(nameof(Events));

        IsLoading = false;
    }

    private void AddEvent(object? obj)
    {
        EventModel eventModel = new();

        if (obj is DateTime date)
        {
            eventModel.DateOf = date;
        }
        else if (obj is PlaceholderEvent pEvent)
        {
            eventModel.DateOf = pEvent.DateOf;
            eventModel.StartTime = pEvent.StartTime;
            eventModel.EndTime = pEvent.EndTime;
        }

        OpenView?.Invoke(this, eventModel);
    }


    private async Task<bool> DeleteAsync(IDatable? datable)
    {
        bool deleted = false;

        MessageBoxResult answer = MessageBox.Show("Are you sure you want to delete this event?", "Wait!", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (answer == MessageBoxResult.No)
            return deleted;

        if (datable is not null)
        {
            deleted = await DatabaseManager.DeleteAsync(datable.Id);
            Events.Remove(datable);
            Events = [.. Events];
            OnPropertyChanged(nameof(Events));
        }

        return deleted;
    }


    /// <summary>
    /// The same event that was moved has now a differen Date and Time, 
    /// so it must be saved in the database
    /// </summary>
    private async Task EventDropAsync(IDatable? datable)
    {
        if (datable is EventModel model)
        {
            await DatabaseManager.UpdateEventAsync(model);
        }
    }

    private async Task SelectedEventAsync(IDatable? datable)
    {
        if (datable is null)
            return;

        //fetch the full event, you might want to also fetch other joining tables if you are using EF
        EventModel? eventModel = await DatabaseManager.GetByIdAsync(datable.Id);

        if (eventModel is not null)
        {
            OpenView?.Invoke(this, eventModel);
        }
    }

    public void ReplaceRecord(EventModel record)
    {
        int index = Events.IndexOf(record);
        if (index >= 0)
        {
            Events[index] = record;
        }
        else
        {
            Events.Add(record);
        }

        Events = [.. Events];
        OnPropertyChanged(nameof(Events));
    }

    public override void Dispose()
    {
        base.Dispose();
        RefreshRequest = null;
        OpenView = null;
        GC.SuppressFinalize(this);
    }
}