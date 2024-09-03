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
        internal static Encoding GBK = Encoding.GetEncoding(936);

        internal static Encoding Shift_JIS = Encoding.GetEncoding(932);

        internal static void initPack(string inputFolderPath, string outputFilePath, string version, string encodingString)
        {
            string engineName = PackWindow.Instance.pa_selEngine.Text;
            string extension = PackWindow.Instance.pa_combPackFormat.Text;
            if (!Array.Exists(Controller.UpdateContent.selectedEngineInfo_Pack.PackFormat.Split('/'), element => element == extension))
            {
                throw new Exception("Error:Not a supported format.");
            }
            extension = CleanExtension(extension);
            string fullTypeName = $"ArcFormats.{engineName}.{extension}";

            Assembly assembly = Assembly.Load("ArcFormats");
            Type ArcPack = assembly.GetType(fullTypeName);

            MethodInfo packMethod = ArcPack?.GetMethod("Pack", BindingFlags.Static | BindingFlags.Public);
            if (ArcPack != null && packMethod != null)
            {
                Encoding encoding;
                switch (encodingString)
                {
                    case "GBK":
                        encoding = GBK;
                        break;
                    case "Shift_JIS":
                        encoding = Shift_JIS;
                        break;
                    case "UTF-8":
                    default:
                        encoding = Encoding.UTF8;
                        break;
                }
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
        /// <summary>
        /// Used for multiple extensions that share the same format.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        private static string CleanExtension(string extension)
        {
            if (extension == "RGSSAD" || extension == "RGSS2A" || extension == "RGSS3A")
            {
                return "RGSS";
            }
            return extension;
        }

    }
}
