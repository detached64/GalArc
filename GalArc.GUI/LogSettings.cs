using GalArc.Controls;
using GalArc.Logs;
using System;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class LogSettings : SettingsTemplate
    {
        private static readonly Lazy<LogSettings> lazyInstance =
            new Lazy<LogSettings>(() => new LogSettings());

        public static LogSettings Instance => lazyInstance.Value;

        private LogSettings()
        {
            InitializeComponent();
        }

        private void LogSettings_Load(object sender, EventArgs e)
        {
            this.chkbxDebug.Checked = BaseSettings.Default.IsDebugMode;
            this.chkbxSaveLog.Checked = BaseSettings.Default.ToSaveLog;
            this.txtLogPath.Text = Logger.Path;
            this.panel.Enabled = this.chkbxSaveLog.Checked;
        }

        private void chkbxDebug_CheckedChanged(object sender, EventArgs e)
        {
            BaseSettings.Default.IsDebugMode = this.chkbxDebug.Checked;
            BaseSettings.Default.Save();
        }

        private void chkbxSaveLog_CheckedChanged(object sender, EventArgs e)
        {
            BaseSettings.Default.ToSaveLog = this.chkbxSaveLog.Checked;
            BaseSettings.Default.Save();
            this.panel.Enabled = this.chkbxSaveLog.Checked;
        }

        private void btSelInput_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.InitialDirectory = Environment.CurrentDirectory;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Logger.Path = dialog.FileName;
                    this.txtLogPath.Text = Logger.Path;
                }
            }
        }

        private void txtLogPath_TextChanged(object sender, EventArgs e)
        {
            Logger.Path = this.txtLogPath.Text;
        }
    }
}
