using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.I18n;
using GalArc.Infrastructure.Settings;
using System.Collections.Generic;

namespace GalArc.ViewModels;

internal partial class SettingsViewModel : ViewModelBase
{
    public Dictionary<string, SettingOptions> SettingItems { get; }

    [ObservableProperty]
    public partial string SelectedItem { get; set; }

    [ObservableProperty]
    public partial SettingOptions CurrentSettingOptions { get; set; }

    public SettingsViewModel()
    {
        SettingItems = new()
        {
            {
                GuiStrings.General,
                new GeneralOptions()
            },
            {
                GuiStrings.Database,
                new DatabaseOptions()
            },
            {
                GuiStrings.Network,
                new NetworkOptions()
            },
            {
                GuiStrings.Logging,
                new LoggingOptions()
            },
        };
    }
}
