using GalArc.GUI;
using Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            extension = Controller.InitUnpack.CleanExtension(extension);
            string fullTypeName = $"ArcFormats.{engineName}.{extension}";

            Assembly assembly = Assembly.Load("ArcFormats");
            Type ArcPack = assembly.GetType(fullTypeName);

            MethodInfo packMethod = ArcPack?.GetMethod("Pack", BindingFlags.Static | BindingFlags.Public);
            if (ArcPack != null && packMethod != null)
            {
                Encoding encoding = Encoding.GetEncoding(Resource.Encoding.CodePages[encodingString]);
                List<object> list = new List<object>
                {
                    inputFolderPath,
                    outputFilePath,
                    version,
                    encoding
                };
                object[] parameters = list.ToArray();
                LogUtility.InitPack(inputFolderPath, outputFilePath);
                packMethod.Invoke(null, parameters);
                LogUtility.FinishPack();
            }
            else
            {
                throw new Exception("Error:Specified format not found.");
            }
        }
    }
}
