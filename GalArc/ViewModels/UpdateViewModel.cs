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
            string latestVer = root.TryGetProperty("tag_name", out JsonElement tagNameElement)
                ? tagNameElement.GetString()
                : throw new JsonException(string.Format(MsgStrings.JsonKeyNotFound, "tag_name"));
            Version latestVersion = new(latestVer.TrimStart('v'));
            UpdateUrl = root.TryGetProperty("html_url", out JsonElement htmlUrlElement)
                ? htmlUrlElement.GetString()
                : throw new JsonException(string.Format(MsgStrings.JsonKeyNotFound, "html_url"));
            Changelog = root.TryGetProperty("body", out JsonElement bodyElement)
                ? bodyElement.GetString()
                : throw new JsonException(string.Format(MsgStrings.JsonKeyNotFound, "body"));
            NewerVersionExist = latestVersion > UpdateManager.GetCurrentVersion();
            StatusMessage = NewerVersionExist ? string.Format(GuiStrings.UpdateAvailable, latestVersion.ToString(3)) : GuiStrings.NoUpdatesAvailable;
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
