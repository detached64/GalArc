using Avalonia.Data.Converters;
using Avalonia.Styling;
using GalArc.Enums;
using GalArc.I18n;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace GalArc.Converters;

internal sealed class ThemeVariantTranslationConverter : IValueConverter
{
    private static readonly Dictionary<ThemeVariant, string> ThemeLocalizations = new()
    {
        { ThemeVariant.Default, GuiStrings.System },
        { ThemeVariant.Light,   GuiStrings.Light },
        { ThemeVariant.Dark,    GuiStrings.Dark }
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not ThemeVariant theme ? value : ThemeLocalizations.TryGetValue(theme, out string localized) ? localized : value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

internal sealed class ProxyTypeTranslationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not ProxyType proxy
            ? value
            : proxy switch
            {
                ProxyType.None => GuiStrings.None,
                ProxyType.Http => "Http",
                ProxyType.Socks => "Socks",
                _ => value,
            };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
