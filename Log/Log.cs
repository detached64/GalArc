// File: Log/Log.cs
// Date: 2024/08/27
// Description: Log窗体
//
// Copyright (C) 2024 detached64
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Log
{
    public partial class LogWindow : Form
    {
        public static LogWindow Instance;

        private static int m_Width;
        private static int m_Height;

        private static int m_main_X;
        private static int m_main_Y;

        private static int Width_delta = 0;

        public event EventHandler LogFormHidden;
        public LogWindow()
        {
            Instance = this;
            InitializeComponent();
            LoadState();
            m_Width = this.Size.Width;
            m_Height = this.Size.Height;
        }

        private void Log_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            OnLogFormHidden(EventArgs.Empty);
        }

        private void log_btnHide_Click(object sender, EventArgs e)
        {
            this.Hide();
            OnLogFormHidden(EventArgs.Empty);
        }

        private void log_btnClear_Click(object sender, EventArgs e)
        {
            this.log_txtLog.Clear();
        }

        public void ChangePosition(int main_X, int main_Y)
        {
            Width_delta = this.Location.X - m_main_X;
            this.Location = new Point(main_X + Width_delta, main_Y + 788 - 12);
            m_main_X = main_X;
            m_main_Y = main_Y;
            //this.Location.Y = main_Y + main.Main.Location.Y - delta
            //delta = 12
        }
        private void ChangePositionWithoutDelta(int main_X, int main_Y)
        {
            this.Location = new Point(main_X, main_Y + 788 - 12);
            m_main_X = main_X;
            m_main_Y = main_Y;
            Width_delta = 0;
        }

        private void log_btnResize_Click(object sender, EventArgs e)
        {
            this.Size = new Size(Width = m_Width, Height = m_Height);
            ChangePositionWithoutDelta(m_main_X, m_main_Y);
        }

        private void log_chkbxDebug_CheckedChanged(object sender, EventArgs e)
        {
            if (Resource.Global.AutoSaveLogVerbose)
            {
                Properties.Settings.Default.log_chkbxVerbose = this.log_chkbxDebug.Checked;
                Properties.Settings.Default.Save();
            }
        }

        private void log_chkbxSave_CheckedChanged(object sender, EventArgs e)
        {
            if (Resource.Global.AutoSaveLogSave)
            {
                Properties.Settings.Default.log_chkbxSave = this.log_chkbxSave.Checked;
                Properties.Settings.Default.Save();
            }
        }

        private void LoadState()
        {
            if (Resource.Global.AutoSaveLogVerbose)
            {
                this.log_chkbxDebug.Checked = Properties.Settings.Default.log_chkbxVerbose;
            }

            if (Resource.Global.AutoSaveLogSave)
            {
                this.log_chkbxSave.Checked = Properties.Settings.Default.log_chkbxSave;
            }
        }

        internal virtual void OnLogFormHidden(EventArgs e)
        {
            LogFormHidden?.Invoke(this, e);
        }
    }
}
