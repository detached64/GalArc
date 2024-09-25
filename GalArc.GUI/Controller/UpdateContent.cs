using GalArc.GUI;
using GalArc.Resource;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalArc.Controller
{
    internal class UpdateContent
    {
        internal static string[] EncodingList = Encodings.CodePages.Keys.ToArray();

        /// <summary>
        /// Add here only when multiple formats are supported and some of them need specific version.
        /// </summary>
        internal static Dictionary<string, string> versionPairs = new Dictionary<string, string>
        {
            { "AdvHD", "ARC" },
            { "SystemNNN", "GPK" },
            { "Triangle", "CGF" },
        };


        internal static void UpdatePackVersion()
        {
            PackWindow.Instance.pa_combVersion.Items.Clear();
            bool shouldEnable = !string.IsNullOrEmpty(MainWindow.selectedEngineInfo_Pack.PackVersion) &&
                (
                    !MainWindow.selectedEngineInfo_Pack.PackFormat.Contains("/") ||
                    ConfigurePackVersion()
                );
            if (shouldEnable)
            {
                PackWindow.Instance.pa_combVersion.Enabled = true;
                PackWindow.Instance.pa_combVersion.Items.AddRange(MainWindow.selectedEngineInfo_Pack.PackVersion.Split('/'));
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
            if (!versionPairs.ContainsKey(MainWindow.selectedEngineInfo_Pack.EngineName) || MainWindow.selectedNodePack.FullPath.Split('/')[1] == versionPairs[MainWindow.selectedEngineInfo_Pack.EngineName])
            {
                isVersionEnabled = true;
            }
            return isVersionEnabled;
        }

    }
}
