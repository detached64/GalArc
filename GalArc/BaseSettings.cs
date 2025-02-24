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
        public bool SaveLog
        {
            get => (bool)this[nameof(SaveLog)];
            set => this[nameof(SaveLog)] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool SaveControlState
        {
            get => (bool)this[nameof(SaveControlState)];
            set => this[nameof(SaveControlState)] = value;
        }

        #region Functions Enabled
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool IsDatabaseEnabled
        {
            get => (bool)this[nameof(IsDatabaseEnabled)];
            set => this[nameof(IsDatabaseEnabled)] = value;
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
        #endregion
    }
}
