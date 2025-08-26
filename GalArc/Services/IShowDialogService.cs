using Avalonia.Controls;
using GalArc.ViewModels;
using System.Threading.Tasks;

namespace GalArc.Services;

internal interface IShowDialogService
{
    void ShowDialog<TView, TViewModel>(Window parent) where TView : Window where TViewModel : ViewModelBase;

    void ShowDialog<TView>(ViewModelBase vm, Window parent) where TView : Window;

    Task<TResult> ShowDialogAsync<TView, TViewModel, TResult>(Window parent) where TView : Window where TViewModel : ViewModelBase where TResult : class;

    Task<TResult> ShowDialogAsync<TView, TResult>(ViewModelBase vm, Window parent) where TView : Window where TResult : class;

    Task ShowDialogAsync<TView, TViewModel>(Window parent) where TView : Window where TViewModel : ViewModelBase;

    Task ShowDialogAsync<TView>(ViewModelBase vm, Window parent) where TView : Window;
}
