using System.Globalization;
using System.Windows.Data;

namespace FilePilot.Converters;

/// <summary>
/// Converts a file size in bytes (long) to a human-readable string
/// (e.g., "1.23 MB", "456 KB").
/// </summary>
public class FileSizeConverter : IValueConverter
{
    private static readonly string[] Suffixes = ["B", "KB", "MB", "GB", "TB"];

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not long bytes || bytes < 0)
            return "0 B";

        if (bytes == 0) return "0 B";

        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < Suffixes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.##} {Suffixes[order]}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
