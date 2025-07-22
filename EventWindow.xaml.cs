using CalendarExample.Models;
using CalendarExample.ViewModels;
using System.Windows;

namespace CalendarExample;
public partial class EventWindow : Window
{
    private readonly EventViewModel viewModel;
    private bool saved = false;
    public EventWindow(EventModel model)
    {
        InitializeComponent();
        viewModel = new(model);
        viewModel.Saved += Saved;
        DataContext = viewModel;
    }

    private void Saved(object? sender, bool args)
    {
        saved = args;
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        if (Owner is MainWindow mainWindow && saved)
        {
            double? viewPortPosition = null;
            bool isWeekView = mainWindow.viewModel.IsWeekView;

            if (isWeekView)
            {
                viewPortPosition = mainWindow.calendar.VerticalScrollPosition;
            }

            mainWindow.viewModel.ReplaceRecord(viewModel.Record);

            if (isWeekView)
            {
                mainWindow.calendar.ScrollIntoView(viewPortPosition);
            }
        }

        viewModel.Dispose();
        base.OnClosed(e);
    }
}