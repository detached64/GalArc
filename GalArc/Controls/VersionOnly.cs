using System;

namespace GalArc.Controls
{
    public partial class VersionOnly : OptionsTemplate
    {
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
            Version = this.combVersion.Text;
        }
    }
}
