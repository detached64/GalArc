using GalArc.Common;
using GalArc.Settings;
using GalArc.Strings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class AboutBox : Form
    {
        private const string ProgramUrl = "https://github.com/detached64/GalArc";

        private const string IssueUrl = "https://github.com/detached64/GalArc/issues";

        public AboutBox()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            Type type = this.dataGridViewEngines.GetType();
            PropertyInfo pi = type.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(this.dataGridViewEngines, true, null);
        }

        private void AboutBox_Load(object sender, EventArgs e)
        {
            if (GUISettings.Default.IsTopMost)
            {
                this.TopMost = true;
            }
            this.lbCopyright.Text = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
            this.lbCurrentVer.Text = string.Format(GUIStrings.lbVersion, Updater.CurrentVersion) + (Updater.CurrentVersion.Revision == 0 ? string.Empty : " beta");
            UpdateDataGridView(EngineInfo.Infos);
            UpdateLicense();
            this.dataGridViewEngines.ClearSelection();
        }

        private void txtSearchText_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtSearchText.Text))
            {
                List<EngineInfo> searched = EngineInfo.Infos
                    .Where(engine => engine.EngineName.IndexOf(this.txtSearchText.Text, StringComparison.OrdinalIgnoreCase) >= 0 || engine.UnpackFormat.IndexOf(this.txtSearchText.Text, StringComparison.OrdinalIgnoreCase) >= 0 || engine.PackFormat.IndexOf(this.txtSearchText.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
                UpdateDataGridView(searched);
            }
            else
            {
                UpdateDataGridView(EngineInfo.Infos);
            }
        }

        private void lbSearch_SizeChanged(object sender, EventArgs e)
        {
            this.txtSearchText.Location = new Point(this.lbSearch.Location.X + this.lbSearch.Width + 6, this.txtSearchText.Location.Y);
        }

        private void linkSite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.linkSite.LinkVisited = true;
            Process.Start(ProgramUrl);
        }

        private void linkIssue_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.linkIssue.LinkVisited = true;
            Process.Start(IssueUrl);
        }

        private void dataGridViewEngines_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex % 2 == 0)
            {
                this.dataGridViewEngines.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(242, 245, 249);
            }
            else
            {
                this.dataGridViewEngines.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
            }
        }

        private void UpdateDataGridView(List<EngineInfo> engines)
        {
            // update the column names
            if (this.dataGridViewEngines.Columns.Count == 3)
            {
                this.dataGridViewEngines.Columns[0].HeaderText = GUIStrings.columnEngineName;
                this.dataGridViewEngines.Columns[1].HeaderText = GUIStrings.columnUnpackFormat;
                this.dataGridViewEngines.Columns[2].HeaderText = GUIStrings.columnPackFormat;
            }
            else
            {
                this.dataGridViewEngines.Columns.Clear();
                this.dataGridViewEngines.Columns.Add("EngineName", GUIStrings.columnEngineName);
                this.dataGridViewEngines.Columns.Add("UnpackFormat", GUIStrings.columnUnpackFormat);
                this.dataGridViewEngines.Columns.Add("PackFormat", GUIStrings.columnPackFormat);
            }

            // update the rows
            this.dataGridViewEngines.Rows.Clear();
            foreach (var engine in engines)
            {
                this.dataGridViewEngines.Rows.Add(engine.EngineName, engine.UnpackFormat, engine.PackFormat);
            }
        }

        private void UpdateLicense()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GalArc.GUI.License.txt"))
            {
                if (stream == null)
                {
                    this.txtLicense.Text = "License file not found.";
                    return;
                }

                using (StreamReader reader = new StreamReader(stream))
                {
                    this.txtLicense.Text = reader.ReadToEnd();
                }
            }
        }
    }
}
