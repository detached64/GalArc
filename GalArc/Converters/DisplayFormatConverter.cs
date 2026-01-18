using Avalonia;
using Avalonia.Data.Converters;
using GalArc.Infrastructure.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace GalArc.Converters;

internal sealed class DisplayFormatConverter : IMultiValueConverter
{
    public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
    {
        return values?.Count != 2 || values[0] is not string name || values[1] is not string description
            ? AvaloniaProperty.UnsetValue
            : string.Format(SettingsManager.Settings.DisplayFormat, name, description);
    }
}
