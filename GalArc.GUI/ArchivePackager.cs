using ArcFormats;
using GalArc.Extensions;
using GalArc.Extensions.GARbroDB;
using System;

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
            SettingsExporter.ExportSettingsToArcFormats();
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
            SettingsExporter.ExportSettingsToArcFormats();
            LoadSchemes();

            Worker worker = new Worker(outputFilePath, inputFolderPath, fullTypeName);
            worker.Pack();
        }

        internal static void LoadSchemes()
        {
            if (ExtensionsConfig.IsEnabled)
            {
                Deserializer.LoadScheme();
            }
        }
    }
}
