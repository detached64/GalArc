using GalArc.Enums;
using GalArc.Infrastructure.Settings;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;

namespace GalArc.Infrastructure.Updates;

internal static class UpdateManager
{
    private const string ApiUrl = "https://api.github.com/repos/detached64/GalArc/releases/latest";

    private static readonly Assembly CurrentAssembly = Assembly.GetExecutingAssembly();

    public static Version GetCurrentVersion()
    {
        return CurrentAssembly.GetName().Version;
    }

    public static async Task SaveUpdateResponse()
    {
        if (SettingsManager.Settings.UpdateSuccess && !string.IsNullOrEmpty(SettingsManager.Settings.UpdateResponse))
        {
            return;
        }
        using HttpClient client = ConfigureClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(CurrentAssembly.GetName().Name, GetCurrentVersion().ToString(3)));
        try
        {
            HttpResponseMessage response = await client.GetAsync(ApiUrl);
            response.EnsureSuccessStatusCode();
            SettingsManager.Settings.UpdateResponse = await response.Content.ReadAsStringAsync();
            SettingsManager.Settings.UpdateSuccess = true;
        }
        catch (Exception ex)
        {
            SettingsManager.Settings.UpdateSuccess = false;
            SettingsManager.Settings.UpdateResponse = ex.Message;
        }
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
