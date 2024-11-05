using GalArc.Extensions;
using GalArc.GUI.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace GalArc.GUI
{
    public partial class ExtensionsSettings : UserControl
    {
        private static ExtensionsSettings instance;

        public static ExtensionsSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ExtensionsSettings();
                }
                return instance;
            }
        }

        public ExtensionsSettings()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            Type type = this.dataGridViewInfos.GetType();
            PropertyInfo pi = type.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(this.dataGridViewInfos, true, null);

            InitDataGridView();
            LoadExtensionsInfo();
        }

        private void ExtensionsSettings_Load(object sender, EventArgs e)
        {
            this.chkbxEnableExtensions.Checked = Settings.Default.EnableExtensions;
            ExtensionsConfig.IsEnabled = Settings.Default.EnableExtensions;
        }

        private void chkbxEnableExtensions_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.EnableExtensions = this.chkbxEnableExtensions.Checked;
            Settings.Default.Save();
            ExtensionsConfig.IsEnabled = this.chkbxEnableExtensions.Checked;
        }

        private void InitDataGridView()
        {
            dataGridViewInfos.Columns.Add("ExtensionName", Resources.columnExtName);
            dataGridViewInfos.Columns.Add("Description", Resources.columnExtDescription);
            dataGridViewInfos.Columns.Add("OriginalAuthor", Resources.columnExtOriginalAuthor);
            dataGridViewInfos.Columns.Add("OriginalWebsite", Resources.columnExtOriginalLink);
            dataGridViewInfos.Columns.Add("ExtensionWebsite", Resources.columnExtLink);
        }

        private void LoadExtensionsInfo()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                List<Type> types = assembly.GetTypes()
                    .Where(t => t.GetCustomAttributes(typeof(ExtensionAttribute), false).Any())
                    .ToList();
                if (types.Count == 0)
                {
                    continue;
                }
                int count = 0;
                foreach (Type type in types)
                {
                    object instance = Activator.CreateInstance(type);
                    if (!type.Name.EndsWith("Config"))
                    {
                        continue;
                    }
                    this.dataGridViewInfos.Rows.Add
                    (
                        type.Name.Remove(type.Name.Length - 6),
                        type.GetProperty("Description").GetValue(instance) ?? "--",
                        type.GetProperty("OriginalAuthor").GetValue(instance) ?? "--"
                    );
                    this.dataGridViewInfos.Rows[count].Cells[3] = new DataGridViewLinkCell { Value = type.GetProperty("OriginalWebsite").GetValue(instance).ToString() };
                    this.dataGridViewInfos.Rows[count].Cells[4] = new DataGridViewLinkCell { Value = type.GetProperty("ExtensionWebsite").GetValue(instance).ToString() };
                    count++;
                    instance = null;
                }
                types = null;
            }
        }

        private void dataGridViewInfos_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }
            if (e.ColumnIndex == 3 || e.ColumnIndex == 4)
            {
                string url = dataGridViewInfos.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                Process.Start(url);
            }
        }
    }
}
