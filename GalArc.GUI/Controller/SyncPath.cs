using System;
using System.IO;

namespace GalArc.Controller
{
    internal class SyncPath
    {
        public static string UnpackPathSync(string input)
        {
            string folderPath = Path.Combine(Path.GetDirectoryName(input), Path.GetFileNameWithoutExtension(input));
            if (File.Exists(folderPath))
            {
                return input.Replace('.', '_') + "_unpacked";
            }
            else
            {
                return folderPath;
            }
        }

        public static string PackPathSync(string input, string ext)
        {
            string filePath = input + "." + ext.ToLower();
            if (File.Exists(filePath))
            {
                return filePath + ".new";
            }
            else
            {
                return filePath;
            }
        }
    }
}
