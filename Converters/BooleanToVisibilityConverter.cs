using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FilePilot.Converters;

/// <summary>
/// Converts a boolean value to a <see cref="Visibility"/> value.
/// True = Visible, False = Collapsed. Pass "Invert" as parameter to reverse.
/// </summary>
public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool boolValue = value is bool b && b;
        bool invert = parameter is string s && s.Equals("Invert", StringComparison.OrdinalIgnoreCase);

        if (invert) boolValue = !boolValue;

        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool invert = parameter is string s && s.Equals("Invert", StringComparison.OrdinalIgnoreCase);
        bool result = value is Visibility v && v == Visibility.Visible;

        return invert ? !result : result;
    }
}
