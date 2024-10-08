using System;
using System.Drawing;
using System.Windows.Forms;

namespace ArcFormats.NitroPlus
{
    public partial class PackPAKOptions : UserControl
    {
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

        private static string ChooseFile()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(openFileDialog.FileName))
                {
                    return openFileDialog.FileName;
                }
                return null;
            }
        }
    }
}
