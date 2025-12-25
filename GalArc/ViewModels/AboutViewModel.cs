using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GalArc.I18n;
using GalArc.Infrastructure.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace GalArc.ViewModels;

internal partial class AboutViewModel : ViewModelBase
{
    private static readonly Assembly CurrentAssembly = Assembly.GetExecutingAssembly();

    [ObservableProperty]
    private string assemblyName = CurrentAssembly.GetName().Name;

    [ObservableProperty]
    private string assemblyVersion = CurrentAssembly.GetName().Version?.ToString();

    [ObservableProperty]
    private string copyright = CurrentAssembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;

    [ObservableProperty]
    private string sourceUrl = "https://github.com/detached64/GalArc";

    [RelayCommand]
    private static async Task OpenUrlAsync(string url)
    {
        try
        {
            await App.Top.Launcher.LaunchUriAsync(new Uri(url));
        }
        catch (Exception ex)
        {
            Logger.Error(MsgStrings.ErrorOpenUrl, ex.Message);
        }
    }
}
