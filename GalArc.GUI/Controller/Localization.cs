using GalArc.GUI;
using GalArc.Properties;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace GalArc.Controller
{
    internal class Localization
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
            main.Main.pages.TabPages[0].Text = Resources.main_unpackPage_Text;
            main.Main.pages.TabPages[1].Text = Resources.main_packPage_Text;
            main.Main.pages.TabPages[2].Text = Resources.main_optionPage_Text;
            main.Main.pages.TabPages[3].Text = Resources.main_aboutPage_Text;
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
            OptionWindow.Instance.op_lbLang.Text = Resources.op_lb_Lang;
            OptionWindow.Instance.op_chkbxOnTop.Text = Resources.op_chkbx_OnTop;
        }

        internal static void GetStrings_about()
        {
            if (AboutWindow.Instance.dataGridViewEngines.Columns.Count == 3)
            {
                AboutWindow.Instance.dataGridViewEngines.Columns[0].HeaderText = Resources.ab_col_EngineName;
                AboutWindow.Instance.dataGridViewEngines.Columns[1].HeaderText = Resources.ab_col_UnpackFormat;
                AboutWindow.Instance.dataGridViewEngines.Columns[2].HeaderText = Resources.ab_col_PackFormat;
            }
            else if (AboutWindow.Instance.dataGridViewEngines.Columns.Count == 0)
            {
                AboutWindow.Instance.dataGridViewEngines.Columns.Clear();
                AboutWindow.Instance.dataGridViewEngines.Columns.Add("EngineName", Resources.ab_col_EngineName);
                AboutWindow.Instance.dataGridViewEngines.Columns.Add("UnpackFormat", Resources.ab_col_UnpackFormat);
                AboutWindow.Instance.dataGridViewEngines.Columns.Add("PackFormat", Resources.ab_col_PackFormat);
            }
            else
            {
                Log.LogUtility.Error("Error occurs in AboutWindow dataGridViewEngines.");
            }
            AboutWindow.Instance.ab_lbSearch.Text = Resources.ab_lb_Search;
            AboutWindow.Instance.ab_linkSite.Text = Resources.ab_link_Site;
            AboutWindow.Instance.ab_linkIssue.Text = Resources.ab_link_Issue;
            AboutWindow.Instance.ab_lbCurrentVersion.Text = Resources.ab_lb_CurrentVersion;
            AboutWindow.Instance.ab_lbLatestVersion.Text = Resources.ab_lb_LatestVersion;
            if (!AboutWindow.Instance.ab_lbLatestVer.Text.Contains("."))         //the latest ver hasn't got.
            {
                AboutWindow.Instance.ab_lbLatestVer.Text = Resources.ab_lb_LatestVer;
            }

            AboutWindow.Instance.ab_gbAbout.Text = Resources.ab_gb_About;
            AboutWindow.Instance.ab_gbFormat.Text = Resources.ab_gb_Format;
            AboutWindow.Instance.ab_gbUpdate.Text = Resources.ab_gb_Update;

            AboutWindow.Instance.ab_btnCheckUpdate.Text = Resources.ab_btn_Check;
            AboutWindow.Instance.ab_btnDownload.Text = Resources.ab_btn_Download;
        }

        internal static void RefreshStrings()
        {
            GetStrings_main();
            GetStrings_unpack();
            GetStrings_pack();
            GetStrings_option();
            GetStrings_about();
            Log.Controller.Localization.GetStrings_Log();
        }
    }
}
