using GalArc.Extensions;
using GalArc.GUI.Properties;
using System;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class ExtensionsSettings : UserControl
    {
        private static ExtensionsSettings instance;

        public static ExtensionsSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ExtensionsSettings();
                }
                return instance;
            }
        }

        public ExtensionsSettings()
        {
            InitializeComponent();
        }

        private void ExtensionsSettings_Load(object sender, EventArgs e)
        {
            chkbxEnableExtensions.Checked = Settings.Default.EnableExtensions;
            ExtensionsConfig.IsEnabled = Settings.Default.EnableExtensions;
        }

        private void chkbxEnableExtensions_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableExtensions = chkbxEnableExtensions.Checked;
            Settings.Default.Save();
            ExtensionsConfig.IsEnabled = chkbxEnableExtensions.Checked;
        }

    }
}
