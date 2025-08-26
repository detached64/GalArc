using Avalonia.Data.Converters;
using Avalonia.Media;
using GalArc.Infrastructure.Logging;
using System;
using System.Globalization;

namespace GalArc.Converters;

internal sealed class LogLevelToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is LogLevel level
            ? level switch
            {
                LogLevel.Info => Brushes.Green,
                LogLevel.Error => Brushes.Red,
                _ => Brushes.Gray,
            }
            : Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
