using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.I18n;
using GalArc.Infrastructure.Settings;
using System.Collections.Generic;

namespace GalArc.ViewModels;

internal partial class SettingsViewModel : ViewModelBase
{
    public IReadOnlyList<SettingItem> SettingItems { get; }

    [ObservableProperty]
    private SettingItem selectedItem;

    [ObservableProperty]
    private SettingOptions currentSettingOptions;

    public SettingsViewModel()
    {
        SettingItems =
        [
            new()
            {
                Title = GuiStrings.General,
                SettingViewModel = new GeneralOptions(),
            },
            new()
            {
                Title = GuiStrings.Logging,
                SettingViewModel = new LoggingOptions(),
            },
            new()
            {
                Title = GuiStrings.Database,
                SettingViewModel = new DatabaseOptions(),
            }
        ];
    }

    partial void OnSelectedItemChanged(SettingItem value)
    {
        if (value != null && value.SettingViewModel != null)
        {
            CurrentSettingOptions = value.SettingViewModel;
        }
        else
        {
            CurrentSettingOptions = null;
        }
    }
}
