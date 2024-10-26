using ArcFormats;
using GalArc.Properties;
using GalArc.Resource;
using System;
using System.Text;

namespace GalArc.Controller
{
    internal class Execute
    {
        internal static void InitUnpack(string inputFilePath, string outputFolderPath)
        {
            string[] selectedInfos = MainWindow.selectedNodeUnpack.FullPath.Split('/');
            string engineName = selectedInfos[0];
            string extension = selectedInfos[1];

            string fullTypeName = $"ArcFormats.{engineName}.{extension}";
            ExportSettings();

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


    }
}
