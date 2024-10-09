using Log;
using System;
using System.IO;
using System.Text;
using Utility;

namespace ArcFormats.SystemNNN
{
    public class VPK
    {
        private struct SystemNNN_vtb_entry
        {
            public int size { get; set; }
            public string filePath { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            //init
            string vpkPath;
            string vtbPath;
            vpkPath = filePath;
            vtbPath = Path.ChangeExtension(filePath, ".vtb");

            if (!File.Exists(vtbPath))
            {
                LogUtility.ErrorNeedAnotherFile(".vtb", ".vpk");
            }

            int vtbSize = (int)new FileInfo(vtbPath).Length;
            int filecount = (vtbSize / 12) - 1;//file size = 12*file count + 12
            LogUtility.InitBar(filecount);

            //open&make dir
            FileStream fs1 = new FileStream(vtbPath, FileMode.Open, FileAccess.Read);
            BinaryReader br1 = new BinaryReader(fs1);
            FileStream fs2 = new FileStream(vpkPath, FileMode.Open, FileAccess.Read);
            BinaryReader br2 = new BinaryReader(fs2);
            Directory.CreateDirectory(folderPath);
            //1~n-1
            for (int i = 1; i < filecount; i++)
            {
                SystemNNN_vtb_entry entry = new SystemNNN_vtb_entry();
                entry.filePath = folderPath + "\\" + Encoding.UTF8.GetString(br1.ReadBytes(8)) + ".vaw";
                int size1 = br1.ReadInt32();
                fs1.Seek(8, SeekOrigin.Current);
                int size2 = br1.ReadInt32();
                entry.size = size2 - size1;
                byte[] buffer = br2.ReadBytes(entry.size);
                File.WriteAllBytes(entry.filePath, buffer);
                fs1.Seek(12 * i, SeekOrigin.Begin);
                LogUtility.UpdateBar();
            }
            //the last
            SystemNNN_vtb_entry last = new SystemNNN_vtb_entry();
            last.filePath = folderPath + "\\" + Encoding.UTF8.GetString(br1.ReadBytes(8)) + ".vaw";
            int vpksizeBefore = br1.ReadInt32();
            fs1.Seek(8, SeekOrigin.Current);//reserve

            int vpksize = br1.ReadInt32();
            last.size = vpksize - vpksizeBefore;
            byte[] buf = br2.ReadBytes(last.size);
            File.WriteAllBytes(last.filePath, buf);
            LogUtility.UpdateBar();

            fs1.Dispose();
            fs2.Dispose();
            br1.Dispose();
            br2.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            //init
            int sizeToNow = 0;
            DirectoryInfo d = new DirectoryInfo(folderPath);
            int filecount = Utilities.GetFileCount_All(folderPath);
            string vpkPath = filePath;
            string vtbPath = vpkPath.Contains(".vpk") ? vpkPath.Replace(".vpk", ".vtb") : vpkPath + ".vtb";
            FileStream fs1 = new FileStream(vtbPath, FileMode.Create, FileAccess.Write);
            FileStream fs2 = new FileStream(vpkPath, FileMode.Create, FileAccess.Write);
            BinaryWriter writer1 = new BinaryWriter(fs1);
            BinaryWriter writer2 = new BinaryWriter(fs2);
            LogUtility.InitBar(filecount);

            foreach (FileInfo fi in d.GetFiles())
            {
                if (Path.GetExtension(fi.FullName) != ".vaw" || Path.GetFileNameWithoutExtension(fi.FullName).Length != 8)
                {
                    fs1.Close();
                    fs2.Close();
                    FileInfo deleteVpk = new FileInfo(vpkPath);
                    FileInfo deleteVtb = new FileInfo(vtbPath);
                    deleteVpk.Delete();
                    deleteVtb.Delete();
                    throw new Exception("Error:File extension " + fi.Extension + " not supported.");
                }
                writer1.Write(Encoding.UTF8.GetBytes(Path.GetFileNameWithoutExtension(fi.FullName)));
                writer1.Write(sizeToNow);
                sizeToNow += (int)fi.Length;

                byte[] buffer = File.ReadAllBytes(fi.FullName);
                writer2.Write(buffer);
                LogUtility.UpdateBar();
            }
            writer1.Write(0);
            writer1.Write(0);
            writer1.Write(sizeToNow);

            fs1.Dispose();
            fs2.Dispose();
        }
    }
}