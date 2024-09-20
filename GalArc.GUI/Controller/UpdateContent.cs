using GalArc.GUI;
using GalArc.Resource;
using Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GalArc.Controller
{
    internal class UpdateContent
    {
        internal static EngineInfo selectedEngineInfo_Unpack;
        internal static EngineInfo selectedEngineInfo_Pack;

        internal static string[] EncodingList = Resource.Encodings.CodePages.Keys.ToArray();

        internal static Dictionary<string, string> versionPairs = new Dictionary<string, string>
        {
            { "AdvHD", "ARC" },
            { "SystemNNN", "GPK" },
            { "Triangle", "CGF" },
        };
        /// <summary>
        /// Initialize the content of combobox.
        /// </summary>
        internal static void InitCombobox_Engines()
        {
            foreach (var engine in EngineInfos.engineInfos)
            {
                if (!string.IsNullOrEmpty(engine.UnpackFormat))
                {
                    UnpackWindow.Instance.un_selEngine.Items.Add(engine.EngineName);
                }
                if (!string.IsNullOrEmpty(engine.PackFormat))
                {
                    PackWindow.Instance.pa_selEngine.Items.Add(engine.EngineName);
                }
            }
        }

        /// <summary>
        /// Update the content of listbox accordingly when specified combobox is updated.
        /// </summary>
        internal static void UpdateUnpackListbox()
        {
            UnpackWindow.Instance.un_ShowFormat.Items.Clear();
            List<EngineInfo> searchedInfo = EngineInfos.engineInfos.Where(engine => string.CompareOrdinal(engine.EngineName, UnpackWindow.Instance.un_selEngine.Text) == 0).ToList();
            if (searchedInfo.Count != 1)
            {
                LogUtility.Error("Error occurs while update format listbox in unpackwindow.");
                return;
            }
            foreach (var engine in searchedInfo)
            {
                UnpackWindow.Instance.un_ShowFormat.Items.AddRange(engine.UnpackFormat.Split('/'));
                selectedEngineInfo_Unpack = engine;
            }
        }

        internal static void UpdatePackListbox()
        {
            PackWindow.Instance.pa_ShowFormat.Items.Clear();
            List<EngineInfo> searchedInfo = EngineInfos.engineInfos.Where(engine => string.CompareOrdinal(engine.EngineName, PackWindow.Instance.pa_selEngine.Text) == 0).ToList();
            if (searchedInfo.Count != 1)
            {
                LogUtility.Error("Error occurs while update format listbox in unpackwindow.");
                return;
            }
            foreach (var engine in searchedInfo)
            {
                PackWindow.Instance.pa_ShowFormat.Items.AddRange(engine.PackFormat.Split('/'));
                selectedEngineInfo_Pack = engine;
            }
        }

        internal static void InitEncoding()
        {
            UnpackWindow.Instance.un_combEncoding.Items.AddRange(EncodingList);
            PackWindow.Instance.pa_combEncoding.Items.AddRange(EncodingList);
        }

        internal static void UpdateUnpackEncoding()
        {
            UnpackWindow.Instance.un_combEncoding.Items.Clear();
            if (selectedEngineInfo_Unpack.isUnpackEncodingEnabled)
            {
                UnpackWindow.Instance.un_combEncoding.Enabled = true;
                UnpackWindow.Instance.un_combEncoding.Items.AddRange(EncodingList);
                UnpackWindow.Instance.un_combEncoding.SelectedIndex = 0;
            }
            else
            {
                UnpackWindow.Instance.un_combEncoding.Enabled = false;
            }
        }

        internal static void UpdatePackEncoding()
        {
            PackWindow.Instance.pa_combEncoding.Items.Clear();
            if (selectedEngineInfo_Pack.isPackEncodingEnabled)
            {
                PackWindow.Instance.pa_combEncoding.Enabled = true;
                PackWindow.Instance.pa_combEncoding.Items.AddRange(EncodingList);
                PackWindow.Instance.pa_combEncoding.SelectedIndex = 0;
            }
            else
            {
                PackWindow.Instance.pa_combEncoding.Enabled = false;
            }
        }

        internal static void UpdatePackFormat()
        {
            PackWindow.Instance.pa_combPackFormat.Items.Clear();
            PackWindow.Instance.pa_combPackFormat.Items.AddRange(selectedEngineInfo_Pack.PackFormat.Split('/'));
            PackWindow.Instance.pa_combPackFormat.SelectedIndex = 0;
        }

        internal static void UpdatePackVersion()
        {
            PackWindow.Instance.pa_combVersion.Items.Clear();
            if (!string.IsNullOrEmpty(selectedEngineInfo_Pack.PackVersion) && !selectedEngineInfo_Pack.PackFormat.Contains("/") || ConfigurePackVersion())
            {
                PackWindow.Instance.pa_combVersion.Enabled = true;
                PackWindow.Instance.pa_combVersion.Items.AddRange(selectedEngineInfo_Pack.PackVersion.Split('/'));
                PackWindow.Instance.pa_combVersion.SelectedIndex = 0;
            }
            else
            {
                PackWindow.Instance.pa_combVersion.Enabled = false;
            }
        }

        /// <summary>
        /// To remove the content of version combobox when selected format doesn't need specific version.
        /// </summary>
        internal static bool ConfigurePackVersion()
        {
            bool isVersionEnabled = false;
            if (versionPairs.ContainsKey(PackWindow.Instance.pa_selEngine.Text) && PackWindow.Instance.pa_combPackFormat.Text == versionPairs[PackWindow.Instance.pa_selEngine.Text])
            {
                isVersionEnabled = true;
            }
            return isVersionEnabled;
        }

        internal static void InitCombobox_Languages()
        {
            OptionWindow.Instance.op_cbLang.Items.AddRange(Resource.Languages.languages.Keys.ToArray());
        }

        internal static void UpdateDataGridView(List<EngineInfo> engines)
        {
            AboutWindow.Instance.dataGridViewEngines.Rows.Clear();
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
                AboutWindow.Instance.dataGridViewEngines.Rows.Add(engine.EngineName, engine.UnpackFormat, engine.PackFormat);
            }
        }

        internal static void UpdateDecScr()
        {
            if (selectedEngineInfo_Unpack.isDecryptScriptEnabled)
            {
                UnpackWindow.Instance.un_chkbxDecScr.Enabled = true;
            }
            else
            {
                UnpackWindow.Instance.un_chkbxDecScr.Enabled = false;
            }
        }
    }
}
