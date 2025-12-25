using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GalArc.Enums;
using GalArc.I18n;
using GalArc.Infrastructure.Logging;
using GalArc.Infrastructure.Settings;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace GalArc.ViewModels;

internal partial class UpdateViewModel : ViewModelBase
{
    private static readonly Assembly CurrentAssembly = Assembly.GetExecutingAssembly();

    private const string ApiUrl = "https://api.github.com/repos/detached64/GalArc/releases/latest";

    private static string UpdateUrl;

    public UpdateViewModel()
    {
        if (Design.IsDesignMode)
            return;
        CheckUpdatesCommand.Execute(null);
    }

    [ObservableProperty]
    private string statusMessage;

    [ObservableProperty]
    private bool isChecking;

    [ObservableProperty]
    private bool newerVersionExist;

    [RelayCommand]
    private async Task CheckUpdatesAsync()
    {
        StatusMessage = MsgStrings.CheckingUpdates;
        IsChecking = true;

        using HttpClient client = ConfigureClient();
        Version currentVersion = CurrentAssembly.GetName().Version;
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(CurrentAssembly.GetName().Name, currentVersion.ToString(3)));

        try
        {
            HttpResponseMessage response = await client.GetAsync(ApiUrl);
            response.EnsureSuccessStatusCode();
            string jsonResponse = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(jsonResponse);
            JsonElement root = doc.RootElement;
            string latestVer = root.TryGetProperty("tag_name", out JsonElement tagNameElement)
                ? tagNameElement.GetString()
                : throw new JsonException(string.Format(MsgStrings.JsonKeyNotFound, "tag_name"));
            Version latestVersion = new(latestVer.TrimStart('v'));
            UpdateUrl = root.TryGetProperty("html_url", out JsonElement htmlUrlElement)
                ? htmlUrlElement.GetString()
                : throw new JsonException(string.Format(MsgStrings.JsonKeyNotFound, "html_url"));

            NewerVersionExist = latestVersion > currentVersion;
            StatusMessage = NewerVersionExist ? string.Format(GuiStrings.UpdateAvailable, latestVersion.ToString(3)) : GuiStrings.NoUpdatesAvailable;
        }
        catch (Exception ex)
        {
            StatusMessage = string.Format(MsgStrings.ErrorCheckingUpdates, ex.Message);
            Logger.Error(MsgStrings.ErrorCheckingUpdates, ex.Message);
        }
        finally
        {
            IsChecking = false;
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

    private static HttpClient ConfigureClient()
    {
        if (SettingsManager.Settings.ProxyType == ProxyType.None)
        {
            return new HttpClient();
        }
        else
        {
            WebProxy proxy = new()
            {
                Address = new Uri($"{SettingsManager.Settings.ProxyType switch
                {
                    ProxyType.Http => "http",
                    ProxyType.Socks => "socks5",
                    _ => throw new NotSupportedException("Unsupported proxy type.")
                }}://{SettingsManager.Settings.ProxyAddress}:{SettingsManager.Settings.ProxyPort}")
            };

            if (!string.IsNullOrEmpty(SettingsManager.Settings.ProxyUsername) && !string.IsNullOrEmpty(SettingsManager.Settings.ProxyPassword))
            {
                proxy.Credentials = new NetworkCredential(SettingsManager.Settings.ProxyUsername, SettingsManager.Settings.ProxyPassword);
            }

            HttpClientHandler httpClientHandler = new()
            {
                Proxy = proxy,
                UseProxy = true,
            };

            return new HttpClient(httpClientHandler);
        }
    }
}
