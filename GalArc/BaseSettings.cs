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
            get => (bool)this[nameof(IsDebugMode)];
            set => this[nameof(IsDebugMode)] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool ToSaveLog
        {
            get => (bool)this[nameof(ToSaveLog)];
            set => this[nameof(ToSaveLog)] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool ToAutoSaveState
        {
            get => (bool)this[nameof(ToAutoSaveState)];
            set => this[nameof(ToAutoSaveState)] = value;
        }

        #region Functions Enabled
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool IsExtensionsEnabled
        {
            get => (bool)this[nameof(IsExtensionsEnabled)];
            set => this[nameof(IsExtensionsEnabled)] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool IsDatabaseEnabled
        {
            get => (bool)this[nameof(IsDatabaseEnabled)];
            set => this[nameof(IsDatabaseEnabled)] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool IsSiglusKeyFinderEnabled
        {
            get => (bool)this[nameof(IsSiglusKeyFinderEnabled)];
            set => this[nameof(IsSiglusKeyFinderEnabled)] = value;
        }
        #endregion

        #region Paths
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string LogPath
        {
            get => (string)this[nameof(LogPath)];
            set => this[nameof(LogPath)] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string DatabasePath
        {
            get => (string)this[nameof(DatabasePath)];
            set => this[nameof(DatabasePath)] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string SiglusKeyFinderPath
        {
            get => (string)this[nameof(SiglusKeyFinderPath)];
            set => this[nameof(SiglusKeyFinderPath)] = value;
        }
        #endregion
    }
}
