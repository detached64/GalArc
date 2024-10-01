using GalArc.Resource;
using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class PreferenceSettings : UserControl
    {
        public PreferenceSettings()
        {
            InitializeComponent();
        }

        private void PreferenceSettings_Load(object sender, EventArgs e)
        {
            this.combEncoding.Items.AddRange(Encodings.CodePages.Keys.ToArray());
            string encoding = Properties.Settings.Default.DefaultEncoding;
            if (this.combEncoding.Items.Contains(encoding))
            {
                this.combEncoding.Text = Properties.Settings.Default.DefaultEncoding;
            }
            else
            {
                this.combEncoding.Text = "Shift-JIS";
            }
        }

        private void combEncoding_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.DefaultEncoding = this.combEncoding.Text;
            Properties.Settings.Default.Save();
        }
    }
}
