using System.Configuration;
using System.Diagnostics;

namespace GalArc.Settings
{
    public class ResSettings : ApplicationSettingsBase
    {
        public static ResSettings Default { get; } = (ResSettings)Synchronized(new ResSettings());

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("932")]
        public int ArtemisPfsEncoding
        {
            get => (int)this[nameof(ArtemisPfsEncoding)];
            set => this[nameof(ArtemisPfsEncoding)] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("932")]
        public int EntisNoaEncoding
        {
            get => (int)this[nameof(EntisNoaEncoding)];
            set => this[nameof(EntisNoaEncoding)] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("932")]
        public int NexasPacEncoding
        {
            get => (int)this[nameof(NexasPacEncoding)];
            set => this[nameof(NexasPacEncoding)] = value;
        }
    }
}
