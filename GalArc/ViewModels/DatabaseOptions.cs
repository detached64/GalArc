using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GalArc.Infrastructure.Settings;
using GalArc.Models.Database.Commons;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GalArc.ViewModels;

internal partial class DatabaseOptions : SettingOptions
{
    [ObservableProperty]
    private string databasePath = SettingsManager.Settings.DatabasePath;

    [RelayCommand]
    private async Task BrowseDatabasePath()
    {
        IReadOnlyList<IStorageFolder> folders = await App.Top.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            AllowMultiple = false
        });
        if (folders.Count > 0)
        {
            DatabasePath = folders[0].Path.LocalPath;
        }
    }

    [RelayCommand]
    private void ResetDatabasePath()
    {
        DatabasePath = DatabaseManager.DefaultPath;
    }

    partial void OnDatabasePathChanged(string value)
    {
        SettingsManager.Settings.DatabasePath = value;
    }
}
