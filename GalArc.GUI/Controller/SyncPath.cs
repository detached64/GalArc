using GalArc.GUI;
using System;
using System.IO;
using System.Linq;

namespace GalArc.Controller
{
    internal class SyncPath
    {
        public static void un_folderPathSync()
        {
            string folderPath = Path.Combine(Path.GetDirectoryName(UnpackWindow.Instance.un_FilePath.Text), Path.GetFileNameWithoutExtension(UnpackWindow.Instance.un_FilePath.Text));
            if (File.Exists(folderPath))
            {
                UnpackWindow.Instance.un_FolderPath.Text = UnpackWindow.Instance.un_FilePath.Text.Replace('.', '_') + "_unpacked";
            }
            else
            {
                UnpackWindow.Instance.un_FolderPath.Text = folderPath;
            }
        }

        public static void pa_filePathSync()
        {
            string filePath = PackWindow.Instance.pa_FolderPath.Text + "." + PackWindow.Instance.pa_combPackFormat.Text.ToLower();
            if (File.Exists(filePath))
            {
                PackWindow.Instance.pa_FilePath.Text = filePath + ".new";
            }
            else
            {
                PackWindow.Instance.pa_FilePath.Text = filePath;
            }
        }
    }
}
