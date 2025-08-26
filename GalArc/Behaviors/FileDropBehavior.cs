using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Windows.Input;

namespace GalArc.Behaviors;

internal static class FileDropBehavior
{
    public static readonly AttachedProperty<ICommand> DropCommandProperty =
        AvaloniaProperty.RegisterAttached<Control, ICommand>("DropCommand", typeof(FileDropBehavior));

    public static ICommand GetDropCommand(Control element)
    {
        return element.GetValue(DropCommandProperty);
    }

    public static void SetDropCommand(Control element, ICommand value)
    {
        element.SetValue(DropCommandProperty, value);
    }

    static FileDropBehavior()
    {
        DragDrop.DropEvent.AddClassHandler<Control>(OnDrop);
        DragDrop.DragOverEvent.AddClassHandler<Control>(OnDragOver);
    }

    private static void OnDragOver(object sender, DragEventArgs e)
    {
        e.DragEffects = e.Data.Contains(DataFormats.Files) ? DragDropEffects.Copy : DragDropEffects.None;
    }

    private static void OnDrop(object sender, DragEventArgs e)
    {
        if (sender is Control control)
        {
            ICommand command = GetDropCommand(control);
            if (command != null && e.Data.Contains(DataFormats.Files))
            {
                IEnumerable<IStorageItem> fileNames = e.Data.GetFiles();
                if (fileNames != null && command.CanExecute(fileNames))
                {
                    command.Execute(fileNames);
                }
            }
        }
    }
}
