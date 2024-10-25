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

using Log.Properties;
using System;
using System.Drawing;
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
        private static int m_main_Width;
        private static int m_main_Height;

        private static readonly int HeightDelta = 12;

        public LogWindow(int w, int h)
        {
            Instance = this;
            InitializeComponent();
            //Control.CheckForIllegalCrossThreadCalls = false;

            m_Width = this.Size.Width;
            m_Height = this.Size.Height;

            m_main_Width = w;
            m_main_Height = h;
        }

        private void LogWindow_Load(object sender, EventArgs e)
        {
            if (Settings.Default.AutoSaveState)
            {
                this.log_chkbxDebug.Checked = Settings.Default.chkbxDebug;
            }

            if (Settings.Default.AutoSaveState)
            {
                this.log_chkbxSave.Checked = Settings.Default.chkbxSave;
            }
        }

        private void log_btnClear_Click(object sender, EventArgs e)
        {
            this.log_txtLog.Clear();
        }

        public void ChangePosition(int main_X, int main_Y)
        {
            this.Location = new Point(main_X, main_Y + m_main_Height - HeightDelta);
            m_main_X = main_X;
            m_main_Y = main_Y;
        }

        private void log_btnResize_Click(object sender, EventArgs e)
        {
            this.Size = new Size(Width = m_Width, Height = m_Height);
            ChangePosition(m_main_X, m_main_Y);
        }

        private void log_chkbxDebug_CheckedChanged(object sender, EventArgs e)
        {
            if (Settings.Default.AutoSaveState)
            {
                Settings.Default.chkbxDebug = this.log_chkbxDebug.Checked;
                Settings.Default.Save();
            }
        }

        private void log_chkbxSave_CheckedChanged(object sender, EventArgs e)
        {
            if (Settings.Default.AutoSaveState)
            {
                Settings.Default.chkbxSave = this.log_chkbxSave.Checked;
                Settings.Default.Save();
            }
        }

        public static void ChangeLocalSettings(bool autoSaveState)
        {
            Settings.Default.AutoSaveState = autoSaveState;
            Settings.Default.Save();
        }

    }
}
