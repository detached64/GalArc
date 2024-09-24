using GalArc.GUI;
using GalArc.Properties;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace GalArc.Controller
{
    internal class Localize
    {
        /// <summary>
        /// Set the culture according to the system language.
        /// </summary>
        /// Four child windows are in the same thread; main window is in another thread.
        /// Use this method in OptionWindow and main only.
        /// 
        internal static string GetLocalCulture()
        {
            string LocalCulture;
            if (string.IsNullOrEmpty(Properties.Settings.Default.lastLang) || !Resource.Global.AutoSaveLanguage)
            {
                LocalCulture = CultureInfo.CurrentCulture.Name;
            }
            else
            {
                LocalCulture = Properties.Settings.Default.lastLang;
            }
            if (!Resource.Languages.languages.Values.ToArray().Contains(LocalCulture))
            {
                LocalCulture = "en-US";
            }
            return LocalCulture;
        }
        internal static void SetLocalCulture(string LocalCulture)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(LocalCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(LocalCulture);
        }

        internal static void GetStrings_main()
        {
            MainWindow.Instance.pages.TabPages[0].Text = Resources.main_unpackPage_Text;
            MainWindow.Instance.pages.TabPages[1].Text = Resources.main_packPage_Text;
        }

        internal static void GetStrings_unpack()
        {
            UnpackWindow.Instance.un_lbEngine.Text = Resources.un_lb_Engine;
            UnpackWindow.Instance.un_lbFormat.Text = Resources.un_lb_Format;
            UnpackWindow.Instance.un_lbInputFile.Text = Resources.un_lb_InputFile;
            UnpackWindow.Instance.un_lbOutputFolder.Text = Resources.un_lb_OutputFolder;
            UnpackWindow.Instance.un_lbEncoding.Text = Resources.un_lb_Encoding;

            UnpackWindow.Instance.un_chkbxMatch.Text = Resources.un_chkbx_Match;
            UnpackWindow.Instance.un_chkbxShowLog.Text = Resources.un_chkbx_Log;
            UnpackWindow.Instance.un_chkbxDecScr.Text = Resources.un_chkbx_DecScr;

            UnpackWindow.Instance.un_btnClear.Text = Resources.un_btn_Clear;
            UnpackWindow.Instance.btnUnpack.Text = Resources.un_btn_Unpack;

            UnpackWindow.Instance.un_gbOption.Text = Resources.un_gb_Option;
        }

        internal static void GetStrings_pack()
        {
            PackWindow.Instance.pa_lbEngine.Text = Resources.pa_lb_Engine;
            PackWindow.Instance.pa_lbFormat.Text = Resources.pa_lb_Format;
            PackWindow.Instance.pa_lbInputFolder.Text = Resources.pa_lb_InputFolder;
            PackWindow.Instance.pa_lbOutputFile.Text = Resources.pa_lb_OutputFile;
            PackWindow.Instance.pa_lbEncoding.Text = Resources.pa_lb_Encoding;
            PackWindow.Instance.pa_lbPackFormat.Text = Resources.pa_lb_PackFormat;
            PackWindow.Instance.pa_lbPackVersion.Text = Resources.pa_lb_Version;

            PackWindow.Instance.pa_chkbxMatch.Text = Resources.pa_chkbx_Match;
            PackWindow.Instance.pa_chkbxShowLog.Text = Resources.pa_chkbx_Log;

            PackWindow.Instance.pa_btnClear.Text = Resources.pa_btn_Clear;
            PackWindow.Instance.btnPack.Text = Resources.pa_btn_Pack;

            PackWindow.Instance.pa_gbOption.Text = Resources.pa_gb_Option;
        }

        internal static void GetStrings_option()
        {
            OptionDialog.Instance.op_lbLang.Text = Resources.op_lb_Lang;
            OptionDialog.Instance.op_chkbxOnTop.Text = Resources.op_chkbx_OnTop;
        }

        internal static void GetStrings_about()
        {
            if (AboutBox.Instance.dataGridViewEngines.Columns.Count == 3)
            {
                AboutBox.Instance.dataGridViewEngines.Columns[0].HeaderText = Resources.ab_col_EngineName;
                AboutBox.Instance.dataGridViewEngines.Columns[1].HeaderText = Resources.ab_col_UnpackFormat;
                AboutBox.Instance.dataGridViewEngines.Columns[2].HeaderText = Resources.ab_col_PackFormat;
            }
            else if (AboutBox.Instance.dataGridViewEngines.Columns.Count == 0)
            {
                AboutBox.Instance.dataGridViewEngines.Columns.Clear();
                AboutBox.Instance.dataGridViewEngines.Columns.Add("EngineName", Resources.ab_col_EngineName);
                AboutBox.Instance.dataGridViewEngines.Columns.Add("UnpackFormat", Resources.ab_col_UnpackFormat);
                AboutBox.Instance.dataGridViewEngines.Columns.Add("PackFormat", Resources.ab_col_PackFormat);
            }
            else
            {
                Log.LogUtility.Error("Error occurs in AboutWindow dataGridViewEngines.");
            }
            AboutBox.Instance.ab_lbSearch.Text = Resources.ab_lb_Search;
            AboutBox.Instance.ab_linkSite.Text = Resources.ab_link_Site;
            AboutBox.Instance.ab_linkIssue.Text = Resources.ab_link_Issue;
            AboutBox.Instance.lbCurrentVer.Text = $"Version {Resource.Global.CurrentVersion}";
        }

        internal static void RefreshStrings()
        {
            GetStrings_main();
            GetStrings_unpack();
            GetStrings_pack();
            Log.Controller.Localize.GetStrings_Log();
        }
    }
}
