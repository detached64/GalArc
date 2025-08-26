using Avalonia.Controls;
using System;
using System.ComponentModel;

namespace GalArc.Views;

public partial class StatusView : Window
{
    public StatusView()
    {
        InitializeComponent();
        Closing += OnClosing;
    }

    private void OnClosing(object sender, CancelEventArgs e)
    {
        if (DataContext is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
