using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.Infrastructure.Cultures;
using GalArc.Infrastructure.Settings;
using System.Collections.Generic;
using System.Globalization;

namespace GalArc.ViewModels;

internal partial class GeneralOptions : SettingOptions
{
    [ObservableProperty]
    private IReadOnlyList<ThemeVariant> themes = [ThemeVariant.Default, ThemeVariant.Light, ThemeVariant.Dark];

    [ObservableProperty]
    private ThemeVariant theme = SettingsManager.Settings.AppTheme;

    [ObservableProperty]
    private IReadOnlyList<CultureInfo> languages = CultureManager.SupportedCultures;

    [ObservableProperty]
    private CultureInfo language = SettingsManager.Settings.AppLanguage;

    partial void OnThemeChanged(ThemeVariant value)
    {
        SettingsManager.Settings.AppTheme = value;
        Application.Current.RequestedThemeVariant = value;
    }

    partial void OnLanguageChanged(CultureInfo value)
    {
        SettingsManager.Settings.AppLanguage = value;
    }
}
