using System.Configuration;
using System.Diagnostics;

namespace GalArc.Settings
{
    public class GUISettings : ApplicationSettingsBase
    {
        public static GUISettings Default { get; } = (GUISettings)Synchronized(new GUISettings());

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool MatchPath
        {
            get => (bool)this[nameof(MatchPath)];
            set => this[nameof(MatchPath)] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string LastLanguage
        {
            get => (string)this[nameof(LastLanguage)];
            set => this[nameof(LastLanguage)] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("False")]
        public bool IsTopMost
        {
            get => (bool)this[nameof(IsTopMost)];
            set => this[nameof(IsTopMost)] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("False")]
        public bool IsUnpackMode
        {
            get => (bool)this[nameof(IsUnpackMode)];
            set => this[nameof(IsUnpackMode)] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("False")]
        public bool IsPackMode
        {
            get => (bool)this[nameof(IsPackMode)];
            set => this[nameof(IsPackMode)] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string DefaultEncoding
        {
            get => (string)this[nameof(DefaultEncoding)];
            set => this[nameof(DefaultEncoding)] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("0")]
        public int UnpackSelectedNode0
        {
            get => (int)this[nameof(UnpackSelectedNode0)];
            set => this[nameof(UnpackSelectedNode0)] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("0")]
        public int UnpackSelectedNode1
        {
            get => (int)this[nameof(UnpackSelectedNode1)];
            set => this[nameof(UnpackSelectedNode1)] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("0")]
        public int PackSelectedNode0
        {
            get => (int)this[nameof(PackSelectedNode0)];
            set => this[nameof(PackSelectedNode0)] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("0")]
        public int PackSelectedNode1
        {
            get => (int)this[nameof(PackSelectedNode1)];
            set => this[nameof(PackSelectedNode1)] = value;
        }
    }
}
