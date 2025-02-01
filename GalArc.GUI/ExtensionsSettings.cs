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
        private static readonly Lazy<ExtensionsSettings> lazyInstance =
                new Lazy<ExtensionsSettings>(() => new ExtensionsSettings());

        public static ExtensionsSettings Instance => lazyInstance.Value;

        private ExtensionsSettings()
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
            this.chkbxEnableExtensions.Checked = BaseSettings.Default.IsExtensionsEnabled;
        }

        private void chkbxEnableExtensions_CheckedChanged(object sender, EventArgs e)
        {
            BaseSettings.Default.IsExtensionsEnabled = this.chkbxEnableExtensions.Checked;
            BaseSettings.Default.Save();
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
            List<Type> types = typeof(IExtension).Assembly.GetTypes()
                .Where(t => t.IsClass && typeof(IExtension).IsAssignableFrom(t))
                .ToList();
            if (types.Count == 0)
            {
                return;
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
