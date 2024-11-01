using ArcFormats;
using GalArc.Logs;
using System;
using System.IO;
using GalArc.GUI.Properties;

namespace GalArc.GUI
{
    internal class ArchivePackager
    {
        internal static void InitUnpack(string inputFilePath, string outputFolderPath)
        {
            string[] selectedInfos = MainWindow.selectedNodeUnpack.FullPath.Split('/');
            string engineName = selectedInfos[0];
            string extension = selectedInfos[1];
            if (selectedInfos[1].Contains("."))     // fixed file name
            {
                if (!String.Equals(extension, Path.GetFileName(inputFilePath), StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Error(string.Format(Resources.logFileNameFailToMatch, extension));
                }
                extension = selectedInfos[1].Replace(".", string.Empty);
            }

            string fullTypeName = $"ArcFormats.{engineName}.{extension}";
            SettingsExporter.ExportSettingsToArcFormats();

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

            Worker worker = new Worker(outputFilePath, inputFolderPath, fullTypeName);
            worker.Pack();
        }
    }
}
