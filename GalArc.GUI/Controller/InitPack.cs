using GalArc.GUI;
using GalArc.Resource;
using System;
using System.Linq;
using System.Text;

namespace GalArc.Controller
{
    internal class InitPack
    {
        internal static void initPack(string inputFolderPath, string outputFilePath, string version, string encodingString)
        {
            string engineName = PackWindow.Instance.pa_selEngine.Text;
            string extension = PackWindow.Instance.pa_combPackFormat.Text;
            if (!Array.Exists(Controller.UpdateContent.selectedEngineInfo_Pack.PackFormat.Split('/'), element => element == extension))
            {
                throw new Exception("Error:Not a supported format.");
            }
            Encoding encoding = Encoding.UTF8;
            if (!string.IsNullOrEmpty(encodingString))
            {
                encoding = Encoding.GetEncoding(Encodings.CodePages[encodingString]);
            }
            extension = InitUnpack.CleanExtension(extension);
            string fullTypeName = $"ArcFormats.{engineName}.{extension}";


            ArcFormats.Global global = new ArcFormats.Global(outputFilePath, inputFolderPath, fullTypeName, encoding, encoding, true, version);
            global.Pack();

        }
    }
}
