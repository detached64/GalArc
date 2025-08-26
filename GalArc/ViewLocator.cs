using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GalArc;

internal sealed partial class ViewLocator : IDataTemplate
{
    public bool Match(object data)
    {
        return data is ObservableObject;
    }
}
