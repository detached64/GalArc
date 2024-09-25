using GalArc.GUI;
using GalArc.Resource;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GalArc.Controller
{
    internal class Execute
    {
        internal static void InitUnpack(string inputFilePath, string outputFolderPath)
        {
            string[] selectedInfos = MainWindow.selectedNodeUnpack.FullPath.Split('/');
            string engineName = selectedInfos[0];
            string extension = selectedInfos[1];
            extension = CleanExtension1(extension, engineName);
            //if (!Array.Exists(MainWindow.selectedEngineInfo_Unpack.UnpackFormat.Split('/'), element => element == extension))
            //{
            //    throw new Exception("Error:Not a supported format.");
            //}
            extension = CleanExtension2(extension, engineName);
            string fullTypeName = $"ArcFormats.{engineName}.{extension}";

            //ArcFormats.Global global = new ArcFormats.Global(inputFilePath, outputFolderPath, fullTypeName);
            //global.Unpack();
        }

        internal static void InitPack(string inputFolderPath, string outputFilePath, string version, string encodingString)
        {
            string[] selectedInfos = MainWindow.selectedNodePack.Text.Split('/');
            string engineName = selectedInfos[0];
            string extension = selectedInfos[1];

            Encoding encoding = Encoding.UTF8;
            if (!string.IsNullOrEmpty(encodingString))
            {
                encoding = Encoding.GetEncoding(Encodings.CodePages[encodingString]);
            }
            string fullTypeName = $"ArcFormats.{engineName}.{extension}";

            ArcFormats.Global global = new ArcFormats.Global(outputFilePath, inputFolderPath, fullTypeName, encoding, encoding, true, version);
            global.Pack();
        }

        /// <summary>
        /// Used for multiple extensions which are not all on the listbox , but they share the same method.
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="engineName"></param>
        /// <returns></returns>
        internal static string CleanExtension1(string extension, string engineName = null)
        {
            if (engineName == "Artemis" && Regex.IsMatch(extension, @"^\d{3}$"))
            {
                return "PFS";
            }
            return extension;
        }

        /// <summary>
        /// Used for multiple extensions which are all on the listbox , and they share the same method.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        internal static string CleanExtension2(string extension, string engineName = null)
        {
            if (extension == "RGSSAD" || extension == "RGSS2A" || extension == "RGSS3A")
            {
                return "RGSS";
            }
            if (engineName == "PJADV" && extension == "PAK")
            {
                return "DAT";
            }
            return extension;
        }

    }
}
