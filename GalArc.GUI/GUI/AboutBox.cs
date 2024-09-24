using GalArc.Controller;
using GalArc.Resource;
using Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class AboutBox : Form
    {
        private List<EngineInfo> engines = EngineInfos.engineInfos;

        private List<EngineInfo> searchedEngines = new List<EngineInfo>();

        public static AboutBox Instance;

        private const int delta = 6;

        private const string programUrl = "https://github.com/detached64/GalArc";

        private const string issueUrl = "https://github.com/detached64/GalArc/issues";

        public AboutBox()
        {
            Instance = this;
            InitializeComponent();

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            Type type = this.dataGridViewEngines.GetType();
            PropertyInfo pi = type.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(this.dataGridViewEngines, true, null);

            this.dataGridViewEngines.RowPrePaint += new DataGridViewRowPrePaintEventHandler(dataGridViewEngines_RowPrePaint);

            Controller.Localize.SetLocalCulture(MainWindow.LocalCulture);
            Controller.Localize.GetStrings_about();
        }

        private void AboutBox_Load(object sender, EventArgs e)
        {
            Controller.UpdateContent.UpdateDataGridView(engines);
            Controller.UpdateContent.UpdateLicense();
        }

        private void searchText_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.searchText.Text))
            {
                searchedEngines = engines.Where(engine => engine.EngineName.IndexOf(this.searchText.Text, StringComparison.OrdinalIgnoreCase) >= 0 || engine.UnpackFormat.IndexOf(this.searchText.Text, StringComparison.OrdinalIgnoreCase) >= 0 || engine.PackFormat.IndexOf(this.searchText.Text, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                Controller.UpdateContent.UpdateDataGridView(searchedEngines);
            }
            else
            {
                Controller.UpdateContent.UpdateDataGridView(engines);
            }
        }

        private void ab_lbSearch_SizeChanged(object sender, EventArgs e)
        {
            this.searchText.Location = new System.Drawing.Point(this.ab_lbSearch.Location.X + this.ab_lbSearch.Width + delta, this.searchText.Location.Y);
        }

        private void ab_linkSite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.ab_linkSite.LinkVisited = true;
            Process.Start(new ProcessStartInfo(programUrl) { UseShellExecute = true });
        }

        private void ab_linkIssue_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.ab_linkIssue.LinkVisited = true;
            Process.Start(new ProcessStartInfo(issueUrl) { UseShellExecute = true });
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
    }
}
