using System;
using System.Windows.Forms;

namespace ArcFormats.Templates
{
    public partial class VersionOnly : UserControl
    {
        public VersionOnly()
        {
            InitializeComponent();
        }

        public VersionOnly(string versions)
        {
            InitializeComponent();
            this.combVersion.Items.AddRange(versions.Split('/'));
            if (this.combVersion.Items.Count > 0)
            {
                this.combVersion.SelectedIndex = 0;
            }
        }

        private void combVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            Config.Version = this.combVersion.Text;
        }
    }
}
