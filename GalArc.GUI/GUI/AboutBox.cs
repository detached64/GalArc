using GalArc.Controller;
using GalArc.Properties;
using GalArc.Resource;
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

            Localize.SetLocalCulture(MainWindow.LocalCulture);
        }

        private void AboutBox_Load(object sender, EventArgs e)
        {
            if (MainWindow.Instance.TopMost)
            {
                this.TopMost = true;
            }
            this.lbCurrentVer.Text = string.Format(Resources.lbVersion, Resource.Version.CurrentVer);
            UpdateDataGridView(engines);
            UpdateLicense();
        }

        private void searchText_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.searchText.Text))
            {
                searchedEngines = engines.Where(engine => engine.EngineName.IndexOf(this.searchText.Text, StringComparison.OrdinalIgnoreCase) >= 0 || engine.UnpackFormat.IndexOf(this.searchText.Text, StringComparison.OrdinalIgnoreCase) >= 0 || engine.PackFormat.IndexOf(this.searchText.Text, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                UpdateDataGridView(searchedEngines);
            }
            else
            {
                UpdateDataGridView(engines);
            }
        }

        private void ab_lbSearch_SizeChanged(object sender, EventArgs e)
        {
            this.searchText.Location = new Point(this.lbSearch.Location.X + this.lbSearch.Width + delta, this.searchText.Location.Y);
        }

        private void ab_linkSite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.linkSite.LinkVisited = true;
            Process.Start(new ProcessStartInfo(programUrl) { UseShellExecute = true });
        }

        private void ab_linkIssue_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.linkIssue.LinkVisited = true;
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

        private void UpdateDataGridView(List<EngineInfo> engines)
        {
            // update the column names
            if (this.dataGridViewEngines.Columns.Count == 3)
            {
                this.dataGridViewEngines.Columns[0].HeaderText = Resources.columnEngineName;
                this.dataGridViewEngines.Columns[1].HeaderText = Resources.columnUnpackFormat;
                this.dataGridViewEngines.Columns[2].HeaderText = Resources.columnPackFormat;
            }
            else
            {
                this.dataGridViewEngines.Columns.Clear();
                this.dataGridViewEngines.Columns.Add("EngineName", Resources.columnEngineName);
                this.dataGridViewEngines.Columns.Add("UnpackFormat", Resources.columnUnpackFormat);
                this.dataGridViewEngines.Columns.Add("PackFormat", Resources.columnPackFormat);
            }

            // update the rows
            this.dataGridViewEngines.Rows.Clear();
            //int count = engines.Count;
            //DataGridViewRow[] rows = new DataGridViewRow[count];
            //for (int i = 0; i < count; i++)
            //{
            //    rows[i] = new DataGridViewRow();
            //    rows[i].CreateCells(AboutWindow.Instance.dataGridViewEngines);
            //    rows[i].Cells[0].Value = engines[i].EngineName;
            //    rows[i].Cells[1].Value = engines[i].UnpackFormat;
            //    rows[i].Cells[2].Value = engines[i].PackFormat;
            //}
            foreach (var engine in engines)
            {
                this.dataGridViewEngines.Rows.Add(engine.EngineName, engine.UnpackFormat, engine.PackFormat);
            }
        }

        private void UpdateLicense()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GalArc.License.txt"))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException("Resource not found.");
                }

                using (StreamReader reader = new StreamReader(stream))
                {
                    this.txtLicense.Text = reader.ReadToEnd();
                }
            }
        }

    }
}
