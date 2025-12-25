using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GalArc.Enums;
using GalArc.I18n;
using GalArc.Infrastructure.Settings;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace GalArc.ViewModels;

internal partial class NetworkOptions : SettingOptions
{
    public IEnumerable<ProxyType> ProxyTypes { get; } = Enum.GetValues<ProxyType>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsProxyEditingEnabled))]
    private ProxyType proxyType = SettingsManager.Settings.ProxyType;

    [ObservableProperty]
    private string proxyAddress = SettingsManager.Settings.ProxyAddress;

    [ObservableProperty]
    private int proxyPort = SettingsManager.Settings.ProxyPort;

    [ObservableProperty]
    private string proxyUsername = SettingsManager.Settings.ProxyUsername;

    [ObservableProperty]
    private string proxyPassword = SettingsManager.Settings.ProxyPassword;

    [ObservableProperty]
    private string message;

    public bool IsProxyEditingEnabled => ProxyType != ProxyType.None;

    private const int TimeOut = 3000;

    [RelayCommand]
    private async Task CheckProxyAsync()
    {
        Message = MsgStrings.CheckingProxy;
        Message = SettingsManager.Settings.ProxyType switch
        {
            ProxyType.None => true,
            ProxyType.Http => await CheckHTTPProxyAsync(),
            ProxyType.Socks => await CheckSOCKSProxyAsync(),
            _ => false,
        } ? MsgStrings.ProxyAvailable : MsgStrings.ProxyNotAvailable;
    }

    private static async Task<bool> CheckHTTPProxyAsync()
    {
        try
        {
            using TcpClient client = new();
            Task task = client.ConnectAsync(SettingsManager.Settings.ProxyAddress, SettingsManager.Settings.ProxyPort);
            return await Task.WhenAny(task, Task.Delay(TimeOut)) == task && task.Exception == null && client.Connected;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<bool> CheckSOCKSProxyAsync()
    {
        try
        {
            using TcpClient client = new();
            Task connectTask = client.ConnectAsync(SettingsManager.Settings.ProxyAddress, SettingsManager.Settings.ProxyPort);
            if (await Task.WhenAny(connectTask, Task.Delay(TimeOut)) != connectTask || connectTask.IsFaulted)
            {
                return false;
            }

            if (!client.Connected)
                return false;

            NetworkStream stream = client.GetStream();
            stream.ReadTimeout = TimeOut;
            stream.WriteTimeout = TimeOut;

            byte[] handshakeRequest = [0x05, 0x01, 0x00];
            await stream.WriteAsync(handshakeRequest);
            await stream.FlushAsync();

            byte[] handshakeResponse = new byte[2];
            await stream.ReadExactlyAsync(handshakeResponse);

            return handshakeResponse[0] == 0x05 && handshakeResponse[1] == 0x00;
        }
        catch
        {
            return false;
        }
    }

    partial void OnProxyTypeChanged(ProxyType value)
    {
        SettingsManager.Settings.ProxyType = value;
    }

    partial void OnProxyAddressChanged(string value)
    {
        SettingsManager.Settings.ProxyAddress = value;
    }

    partial void OnProxyPortChanged(int value)
    {
        SettingsManager.Settings.ProxyPort = value;
    }

    partial void OnProxyUsernameChanged(string value)
    {
        SettingsManager.Settings.ProxyUsername = value;
    }

    partial void OnProxyPasswordChanged(string value)
    {
        SettingsManager.Settings.ProxyPassword = value;
    }
}
