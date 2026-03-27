using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GalArc.Infrastructure.Cultures;
using GalArc.Infrastructure.Settings;
using System.Collections.Generic;
using System.Globalization;

namespace GalArc.ViewModels;

internal partial class GeneralOptions : SettingOptions
{
    [ObservableProperty]
    public partial IReadOnlyList<ThemeVariant> Themes { get; set; } = [ThemeVariant.Default, ThemeVariant.Light, ThemeVariant.Dark];

    [ObservableProperty]
    public partial ThemeVariant Theme { get; set; } = SettingsManager.Settings.AppTheme;

    [ObservableProperty]
    public partial IReadOnlyList<CultureInfo> Languages { get; set; } = CultureManager.SupportedCultures;

    [ObservableProperty]
    public partial CultureInfo Language { get; set; } = SettingsManager.Settings.AppLanguage;

    [ObservableProperty]
    public partial string DisplayFormat { get; set; } = SettingsManager.Settings.DisplayFormat;

    [RelayCommand]
    private void ResetDisplayFormat()
    {
        DisplayFormat = AppSettings.DefaultDisplayFormat;
    }

    partial void OnThemeChanged(ThemeVariant value)
    {
        SettingsManager.Settings.AppTheme = value;
        Application.Current.RequestedThemeVariant = value;
    }

    partial void OnLanguageChanged(CultureInfo value)
    {
        SettingsManager.Settings.AppLanguage = value;
    }

    partial void OnDisplayFormatChanged(string value)
    {
        SettingsManager.Settings.DisplayFormat = value;
    }
}
