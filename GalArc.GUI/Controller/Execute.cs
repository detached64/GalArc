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

            Global global = new Global(inputFilePath, outputFolderPath, fullTypeName);
            global.Unpack();
        }

        internal static void InitPack(string inputFolderPath, string outputFilePath)
        {
            string[] selectedInfos = MainWindow.selectedNodePack.FullPath.Split('/');
            string engineName = selectedInfos[0];
            string extension = selectedInfos[1];

            string fullTypeName = $"ArcFormats.{engineName}.{extension}";
            ExportSettings();

            Global global = new Global(outputFilePath, inputFolderPath, fullTypeName);
            global.Pack();
        }

        internal static void ExportSettings()
        {
            Global.Encoding = Encoding.GetEncoding(Encodings.CodePages[Settings.Default.DefaultEncoding]);
        }

    }
}
