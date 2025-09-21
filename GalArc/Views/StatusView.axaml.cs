using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace GalArc.Views;

public partial class StatusView : Window
{
    public StatusView()
    {
        InitializeComponent();
        Closing += OnClosing;

        LogList.ItemsView.CollectionChanged += (s, e) =>
        {
            if (e.Action != NotifyCollectionChangedAction.Add)
                return;
            Dispatcher.UIThread.Post(() =>
            {
                ScrollViewer sv = LogList.Scroll as ScrollViewer;
                sv?.ScrollToEnd();
            });
        };
    }

    private void OnClosing(object sender, CancelEventArgs e)
    {
        if (DataContext is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
