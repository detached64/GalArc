using GalArc.Controls;
using System;
using System.Drawing;

namespace ArcFormats.NitroPlus
{
    public partial class PackPAKOptions : OptionsTemplate
    {
        public static PackPAKOptions Instance { get; } = new PackPAKOptions();

        internal static string OriginalFilePath = string.Empty;

        public PackPAKOptions()
        {
            InitializeComponent();
        }

        private void lbOriginalArc_SizeChanged(object sender, EventArgs e)
        {
            this.txtOriginalFilePath.Location = new Point(this.lbOriginalArc.Location.X + this.lbOriginalArc.Width + 6, this.txtOriginalFilePath.Location.Y);
            this.btSelect.Location = new Point(this.txtOriginalFilePath.Location.X + this.txtOriginalFilePath.Width + 6, this.btSelect.Location.Y);
        }

        private void btSelect_Click(object sender, EventArgs e)
        {
            this.txtOriginalFilePath.Text = ChooseFile() ?? this.txtOriginalFilePath.Text;
            OriginalFilePath = this.txtOriginalFilePath.Text;
        }
    }
}
