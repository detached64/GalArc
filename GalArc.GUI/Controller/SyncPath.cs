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
            if (UnpackWindow.Instance.un_selEngine.Text == "NextonLikeC")
            {
                UnpackWindow.Instance.un_FolderPath.Text = Path.GetDirectoryName(UnpackWindow.Instance.un_FilePath.Text) + "\\" + Path.GetFileNameWithoutExtension(UnpackWindow.Instance.un_FilePath.Text) + "_unpacked";
            }
            else
            {
                UnpackWindow.Instance.un_FolderPath.Text = Path.GetDirectoryName(UnpackWindow.Instance.un_FilePath.Text) + "\\" + Path.GetFileNameWithoutExtension(UnpackWindow.Instance.un_FilePath.Text);
            }
        }

        public static void pa_filePathSync()
        {
            PackWindow.Instance.pa_FilePath.Text = PackWindow.Instance.pa_FolderPath.Text + "." + PackWindow.Instance.pa_combPackFormat.Text.ToLower() + ".new";
        }
    }
}
