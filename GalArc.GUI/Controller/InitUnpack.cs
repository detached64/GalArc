using GalArc.GUI;
using GalArc.Resource;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace GalArc.Controller
{
    internal class InitUnpack
    {
        internal static void initUnpack(string inputFilePath, string outputFolderPath, string encodingString)
        {
            string engineName = UnpackWindow.Instance.un_selEngine.Text;
            string extension = Path.GetExtension(inputFilePath).Replace(".", string.Empty).ToUpper();
            if (!Array.Exists(Controller.UpdateContent.selectedEngineInfo_Unpack.UnpackFormat.Split('/'), element => element == extension))
            {
                throw new Exception("Error:Not a supported format.");
            }
            Encoding encoding = Encoding.UTF8;
            if (!string.IsNullOrEmpty(encodingString))
            {
                encoding = Encoding.GetEncoding(Encodings.CodePages[encodingString]);
            }
            extension = CleanExtension(extension);
            string fullTypeName = $"ArcFormats.{engineName}.{extension}";

            ArcFormats.Global global = new ArcFormats.Global(inputFilePath, outputFolderPath, fullTypeName, encoding, encoding, true, null);
            global.Unpack();
        }

        /// <summary>
        /// Used for multiple extensions that share the same format.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        internal static string CleanExtension(string extension)
        {
            if (extension == "RGSSAD" || extension == "RGSS2A" || extension == "RGSS3A")
            {
                return "RGSS";
            }
            return extension;
        }
    }
}
