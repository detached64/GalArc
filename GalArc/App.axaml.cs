using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GalArc.Infrastructure.Logging;
using GalArc.Infrastructure.Settings;
using GalArc.Services;
using GalArc.ViewModels;
using GalArc.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;

namespace GalArc;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }

    public static TopLevel Top { get; private set; }

    private static ServiceProvider ConfigureService()
    {
        ServiceCollection services = new();
        // Views
        services.AddSingleton<MainView>();
        services.AddTransient<StatusView>();
        services.AddTransient<SettingsView>();
        services.AddTransient<AboutView>();
        services.AddTransient<UpdateView>();
        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<StatusViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<AboutViewModel>();
        services.AddTransient<UpdateViewModel>();
        // Services
        services.AddSingleton<IShowDialogService, ShowDialogService>();

        return services.BuildServiceProvider();
    }

    public override void Initialize()
    {
        ServiceProvider = ConfigureService();
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Thread.CurrentThread.CurrentCulture = SettingsManager.Settings.AppLanguage;
        Thread.CurrentThread.CurrentUICulture = SettingsManager.Settings.AppLanguage;

        MainView mainView = ServiceProvider.GetRequiredService<MainView>();
        mainView.DataContext = ServiceProvider.GetRequiredService<MainViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = mainView;
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
            desktop.Exit += (_, _) =>
            {
                SettingsManager.SaveSettings();
                Logger.Persist();
            };
            Top = TopLevel.GetTopLevel(desktop.MainWindow);
            Application.Current.RequestedThemeVariant = SettingsManager.Settings.AppTheme;
        }
        base.OnFrameworkInitializationCompleted();
    }
}
