using GalArc.GUI;
using Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GalArc.Controller
{
    internal class InitUnpack
    {
        internal static Encoding GBK = Encoding.GetEncoding(936);

        internal static Encoding Shift_JIS = Encoding.GetEncoding(932);

        internal static void initUnpack(string inputFilePath, string outputFolderPath, string encodingString)
        {
            string engineName = UnpackWindow.Instance.un_selEngine.Text;
            string extension = Path.GetExtension(inputFilePath).Replace(".", string.Empty).ToUpper();
            if (!Array.Exists(Controller.UpdateContent.selectedEngineInfo_Unpack.UnpackFormat.Split('/'), element => element == extension))
            {
                throw new Exception("Error:Not a supported format.");
            }
            extension = CleanExtension(extension);
            string fullTypeName = $"ArcFormats.{engineName}.{extension}";

            Assembly assembly = Assembly.Load("ArcFormats");
            Type ArcUnpack = assembly.GetType(fullTypeName);

            MethodInfo unpackMethod = ArcUnpack?.GetMethod("Unpack", BindingFlags.Static | BindingFlags.Public);
            if (ArcUnpack != null && unpackMethod != null)
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
                    inputFilePath,
                    outputFolderPath,
                    encoding
                };
                object[] parameters = list.ToArray();
                LogUtility.InitUnpack(inputFilePath, outputFolderPath);
                unpackMethod.Invoke(null, parameters);
                LogUtility.FinishUnpack();
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
