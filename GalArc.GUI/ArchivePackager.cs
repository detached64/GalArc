using ArcFormats;
using GalArc.Common;
using GalArc.GUI.Properties;
using System;
using System.Text;

namespace GalArc.GUI
{
    internal class ArchivePackager
    {
        internal static void InitUnpack(string inputFilePath, string outputFolderPath)
        {
            string[] selectedInfos = MainWindow.selectedNodeUnpack.FullPath.Split('/');
            string engineName = selectedInfos[0];
            string extension = selectedInfos[1];

            string fullTypeName = $"ArcFormats.{engineName}.{extension}";
            ExportSettings();
            LoadSchemes();

            Worker worker = new Worker(inputFilePath, outputFolderPath, fullTypeName);
            worker.UnpackOne();
        }

        internal static void InitPack(string inputFolderPath, string outputFilePath)
        {
            string[] selectedInfos = MainWindow.selectedNodePack.FullPath.Split('/');
            string engineName = selectedInfos[0];
            string extension = selectedInfos[1];

            string fullTypeName = $"ArcFormats.{engineName}.{extension}";
            ExportSettings();
            LoadSchemes();

            Worker worker = new Worker(outputFilePath, inputFolderPath, fullTypeName);
            worker.Pack();
        }

        internal static void ExportSettings()
        {
            if (!string.IsNullOrEmpty(Settings.Default.DefaultEncoding))
            {
                Config.Encoding = Encoding.GetEncoding(Encodings.CodePages[Settings.Default.DefaultEncoding]);
            }
            else
            {
                Config.Encoding = Encoding.UTF8;
            }
        }

        internal static void LoadSchemes()
        {
            if (Extensions.ExtensionsConfig.IsEnabled)
            {
                Extensions.GARbroDB.Deserializer.LoadScheme();
            }
        }
    }
}
