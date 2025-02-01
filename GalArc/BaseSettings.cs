using System;
using System.Configuration;
using System.Diagnostics;

namespace GalArc
{
    public class BaseSettings : ApplicationSettingsBase
    {
        public static BaseSettings Default { get; } = (BaseSettings)Synchronized(new BaseSettings());

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("False")]
        public bool IsDebugMode
        {
            get => (bool)this["IsDebugMode"];
            set => this["IsDebugMode"] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool ToSaveLog
        {
            get => (bool)this["ToSaveLog"];
            set => this["ToSaveLog"] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool ToAutoSaveState
        {
            get => (bool)this["ToAutoSaveState"];
            set => this["ToAutoSaveState"] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool IsExtensionsEnabled
        {
            get => (bool)this["IsExtensionsEnabled"];
            set => this["IsExtensionsEnabled"] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool IsGARbroDBEnabled
        {
            get => (bool)this["IsGARbroDBEnabled"];
            set => this["IsGARbroDBEnabled"] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string LogPath
        {
            get => (string)this["LogPath"];
            set => this["LogPath"] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string DatabasePath
        {
            get => (string)this["DatabasePath"];
            set => this["DatabasePath"] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string GARbroDBPath
        {
            get => (string)this["GARbroDBPath"];
            set => this["GARbroDBPath"] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string SiglusKeyFinderPath
        {
            get => (string)this["SiglusKeyFinderPath"];
            set => this["SiglusKeyFinderPath"] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool IsSiglusKeyFinderEnabled
        {
            get => (bool)this["IsSiglusKeyFinderEnabled"];
            set => this["IsSiglusKeyFinderEnabled"] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool IsDatabaseEnabled
        {
            get => (bool)this["IsDatabaseEnabled"];
            set => this["IsDatabaseEnabled"] = value;
        }
    }
}
