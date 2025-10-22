using Avalonia.Controls;
using GalArc.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace GalArc.Services;

internal sealed class ShowDialogService(IServiceProvider serviceProvider) : IShowDialogService
{
    public void ShowDialog<TView, TViewModel>(Window parent) where TView : Window where TViewModel : ViewModelBase
    {
        Window view = serviceProvider.GetService<TView>() as Window ?? throw new InvalidOperationException($"Could not resolve view of type {typeof(TView).FullName}");
        view.DataContext = serviceProvider.GetService<TViewModel>();
        _ = view.ShowDialog(parent);
    }

    public void ShowDialog<TView>(ViewModelBase vm, Window parent) where TView : Window
    {
        Window view = serviceProvider.GetService<TView>() as Window ?? throw new InvalidOperationException($"Could not resolve view of type {typeof(TView).FullName}");
        view.DataContext = vm;
        _ = view.ShowDialog(parent);
    }

    public async Task<TResult> ShowDialogAsync<TView, TViewModel, TResult>(Window parent) where TView : Window where TViewModel : ViewModelBase where TResult : class
    {
        Window view = serviceProvider.GetService<TView>() as Window ?? throw new InvalidOperationException($"Could not resolve view of type {typeof(TView).FullName}");
        view.DataContext = serviceProvider.GetService<TViewModel>();
        return await view.ShowDialog<TResult>(parent);
    }

    public async Task<TResult> ShowDialogAsync<TView, TResult>(ViewModelBase vm, Window parent) where TView : Window where TResult : class
    {
        Window view = serviceProvider.GetService<TView>() as Window ?? throw new InvalidOperationException($"Could not resolve view of type {typeof(TView).FullName}");
        view.DataContext = vm;
        return await view.ShowDialog<TResult>(parent);
    }

    public async Task ShowDialogAsync<TView, TViewModel>(Window parent) where TView : Window where TViewModel : ViewModelBase
    {
        Window view = serviceProvider.GetService<TView>() as Window ?? throw new InvalidOperationException($"Could not resolve view of type {typeof(TView).FullName}");
        view.DataContext = serviceProvider.GetService<TViewModel>();
        await view.ShowDialog(parent);
    }

    public async Task ShowDialogAsync<TView>(ViewModelBase vm, Window parent) where TView : Window
    {
        Window view = serviceProvider.GetService<TView>() as Window ?? throw new InvalidOperationException($"Could not resolve view of type {typeof(TView).FullName}");
        view.DataContext = vm;
        await view.ShowDialog(parent);
    }
}
