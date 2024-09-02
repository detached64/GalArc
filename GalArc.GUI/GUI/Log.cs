using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class Log : Form
    {
        public static Log Instance;
        public Log()
        {
            Instance = this;
            InitializeComponent();
        }

        private void Log_Load(object sender, EventArgs e)
        {
            this.Location = new Point(
                (Screen.PrimaryScreen.WorkingArea.Width - this.Width) / 2,
                (Screen.PrimaryScreen.WorkingArea.Height + main.Main.Height) / 2 - 10);
        }

        private void Log_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void log_btnHide_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
