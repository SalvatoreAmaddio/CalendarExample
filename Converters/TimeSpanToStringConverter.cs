using System.Globalization;
using System.Windows.Data;

namespace CalendarExample.Converters;

public class TimeSpanToStringConverter : IValueConverter
{
    // TimeSpan → string (e.g., "14:30")
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan ts)
            return ts.ToString(@"hh\:mm"); // 24-hour format
        return string.Empty;
    }

    // string → TimeSpan
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string input && TimeSpan.TryParseExact(input, @"hh\:mm", CultureInfo.InvariantCulture, out var result))
            return result;

        return Binding.DoNothing; // prevent invalid values from crashing
    }
}