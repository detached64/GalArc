using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GalArc.I18n;
using GalArc.Infrastructure.Logging;
using GalArc.Infrastructure.Settings;
using GalArc.Infrastructure.Updates;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace GalArc.ViewModels;

internal partial class UpdateViewModel : ViewModelBase
{
    private static string UpdateUrl;

    [ObservableProperty]
    private string currentVersion;

    [ObservableProperty]
    private string latestVersion = GuiStrings.Unknown;

    [ObservableProperty]
    private string statusMessage;

    [ObservableProperty]
    private bool newerVersionExist;

    [ObservableProperty]
    private string changelog;

    public UpdateViewModel()
    {
        if (Design.IsDesignMode)
            return;
        ParseUpdateResponse();
    }

    private void ParseUpdateResponse()
    {
        Version currentVersion = UpdateManager.GetCurrentVersion();
        CurrentVersion = currentVersion.ToString(3);
        if (!SettingsManager.Settings.UpdateSuccess)
        {
            StatusMessage = string.Format(MsgStrings.ErrorCheckingUpdates, SettingsManager.Settings.UpdateResponse);
            Logger.Error(MsgStrings.ErrorCheckingUpdates, SettingsManager.Settings.UpdateResponse);
            return;
        }
        try
        {
            using JsonDocument doc = JsonDocument.Parse(SettingsManager.Settings.UpdateResponse);
            JsonElement root = doc.RootElement;
            LatestVersion = root.TryGetProperty("tag_name", out JsonElement tagNameElement)
                ? tagNameElement.GetString().TrimStart('v')
                : throw new JsonException(string.Format(MsgStrings.JsonKeyNotFound, "tag_name"));
            Version latestVersion = new(LatestVersion);
            UpdateUrl = root.TryGetProperty("html_url", out JsonElement htmlUrlElement)
                ? htmlUrlElement.GetString()
                : throw new JsonException(string.Format(MsgStrings.JsonKeyNotFound, "html_url"));
            Changelog = root.TryGetProperty("body", out JsonElement bodyElement)
                ? bodyElement.GetString()
                : throw new JsonException(string.Format(MsgStrings.JsonKeyNotFound, "body"));
            NewerVersionExist = latestVersion > currentVersion;
            StatusMessage = NewerVersionExist ? GuiStrings.UpdateAvailable : GuiStrings.NoUpdatesAvailable;
        }
        catch (Exception ex)
        {
            StatusMessage = string.Format(MsgStrings.ErrorCheckingUpdates, ex.Message);
            Logger.Error(MsgStrings.ErrorCheckingUpdates, ex.Message);
        }
    }

    [RelayCommand]
    private static async Task OpenUpdateUrlAsync()
    {
        if (!string.IsNullOrEmpty(UpdateUrl))
        {
            try
            {
                await App.Top.Launcher.LaunchUriAsync(new Uri(UpdateUrl));
            }
            catch (Exception ex)
            {
                Logger.Error(MsgStrings.ErrorOpenUrl, ex.Message);
            }
        }
    }

    [RelayCommand]
    private static void Exit(Window window)
    {
        window?.Close();
    }
}
