using CalendarExample.Models;
using CalendarExample.ViewModels;
using System.Globalization;
using System.Windows;

namespace CalendarExample;

public partial class MainWindow : Window
{
    public readonly CalendarViewModel viewModel = new();
    public MainWindow()
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.OpenView += OnOpenView;
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= MainWindow_Loaded;
        await viewModel.GetDataAsync();
    }

    private void OnOpenView(object? sender, object arg)
    {
        if (arg is EventModel eventModel) 
        { 
            new EventWindow(eventModel) 
            { 
                Title = eventModel.Id == 0 ? "Add Event" : "Edit Event",
                Owner = this,
            }.ShowDialog();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        viewModel.Dispose();
        base.OnClosed(e);
    }
}

public static class Cultures
{
    public static readonly CultureInfo US = new("en-US");
    public static readonly CultureInfo IT = new("it-IT");
}