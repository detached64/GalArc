using System.Collections.ObjectModel;

namespace GalArc.Infrastructure.Settings;

internal sealed class SettingItem
{
    public string Title { get; set; }
    public SettingOptions SettingViewModel { get; set; }
    public ObservableCollection<SettingItem> Children { get; set; }
}
