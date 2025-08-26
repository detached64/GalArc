using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GalArc.Infrastructure.Logging;
using GalArc.Infrastructure.Settings;
using System.Threading.Tasks;

namespace GalArc.ViewModels;

internal partial class LoggingOptions : SettingOptions
{
    [ObservableProperty]
    private bool saveLogs = SettingsManager.Settings.SaveLogs;

    [ObservableProperty]
    private string logFilePath = SettingsManager.Settings.LogFilePath;

    [RelayCommand]
    private async Task BrowseLogPath()
    {
        IStorageFile resultFile = await App.Top.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            SuggestedFileName = "log.txt"
        });
        if (resultFile != null)
        {
            LogFilePath = resultFile.Path.LocalPath;
        }
    }

    [RelayCommand]
    private void ResetLogPath()
    {
        LogFilePath = Logger.DefaultPath;
    }

    partial void OnSaveLogsChanged(bool value)
    {
        SettingsManager.Settings.SaveLogs = value;
    }

    partial void OnLogFilePathChanged(string value)
    {
        SettingsManager.Settings.LogFilePath = value;
    }
}
