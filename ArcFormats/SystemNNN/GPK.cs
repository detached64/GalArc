using Log;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Utility;

namespace ArcFormats.SystemNNN
{
    public class GPK
    {
        private struct SystemNNN_gtb_entry
        {
            public uint size { get; set; }
            public uint offset { get; set; }
            public string filePath { get; set; }
        }

        public static void Unpack(string filePath, string folderPath, Encoding encoding)
        {
            //init
            string gpkPath;
            string gtbPath;

            gpkPath = filePath;
            gtbPath = Path.ChangeExtension(filePath, ".gtb");

            if (!File.Exists(gtbPath))
            {
                LogUtility.Error_NeedAnotherFile(".gtb", ".gpk");
            }

            //open&make dir
            FileStream fs1 = new FileStream(gtbPath, FileMode.Open, FileAccess.Read);
            BinaryReader br1 = new BinaryReader(fs1);
            uint filecount = br1.ReadUInt32();
            LogUtility.InitBar(filecount);

            FileStream fs2 = new FileStream(gpkPath, FileMode.Open, FileAccess.Read);
            BinaryReader br2 = new BinaryReader(fs2);

            Directory.CreateDirectory(folderPath);

            uint thisPos = 0;
            uint maxPos = 0;
            //process
            //i~n-1
            for (int i = 1; i < filecount; i++)
            {
                SystemNNN_gtb_entry entry = new SystemNNN_gtb_entry();

                //offset
                entry.offset = br1.ReadUInt32();

                fs1.Seek(4 * filecount - 4, SeekOrigin.Current);

                //size
                uint size1 = br1.ReadUInt32();
                uint size2 = br1.ReadUInt32();
                entry.size = size2 - size1;

                fs1.Seek(4 + 8 * filecount + entry.offset, SeekOrigin.Begin);

                entry.filePath = folderPath + "\\" + Encoding.UTF8.GetString(Utilities.ReadUntil_Ansi(br1, 0x00)) + ".dwq";
                thisPos = (uint)fs1.Position;
                maxPos = Math.Max(thisPos, maxPos);

                //get file content
                byte[] buffer = br2.ReadBytes((int)entry.size);

                //write file
                File.WriteAllBytes(entry.filePath, buffer);
                fs1.Seek(4 + 4 * i, SeekOrigin.Begin);

                LogUtility.UpdateBar();
            }

            uint offset = br1.ReadUInt32();
            uint gtbSize = (uint)new FileInfo(gtbPath).Length;
            uint gpkSize = (uint)new FileInfo(gpkPath).Length;
            fs1.Seek(8 * filecount, SeekOrigin.Begin);
            uint sizeWithoutLast = br1.ReadUInt32();
            fs1.Seek(offset + 4 + 8 * filecount, SeekOrigin.Begin);
            SystemNNN_gtb_entry last = new SystemNNN_gtb_entry();
            last.offset = gtbSize - (offset + 4 + 8 * filecount) - 1;
            last.filePath = folderPath + "\\" + Encoding.UTF8.GetString(Utilities.ReadUntil_Ansi(br1, 0x00)) + ".dwq";
            last.size = gpkSize - sizeWithoutLast;

            thisPos = (uint)fs1.Position;
            maxPos = Math.Max(thisPos, maxPos);
            byte[] buf = br2.ReadBytes((int)last.size);
            File.WriteAllBytes(last.filePath, buf);
            LogUtility.UpdateBar();
            if (maxPos == gtbSize)
            {
                LogUtility.Info("Valid gpk v1 archive detected.");
            }
            else
            {
                LogUtility.Info("Valid gpk v2 archive detected.");
            }

            fs1.Dispose();
            fs2.Dispose();
            br1.Dispose();
            br2.Dispose();
        }
        public static void Pack(string folderPath, string filePath, string version, Encoding encoding)
        {

            DirectoryInfo d = new DirectoryInfo(folderPath);
            uint filecount = (uint)Utilities.GetFileCount_All(folderPath);
            LogUtility.InitBar(3 * filecount);
            string gpkPath = filePath;
            string gtbPath = filePath.Contains(".gpk") ? gpkPath.Replace(".gpk", ".gtb") : gpkPath + ".gtb";


            FileStream fs1 = new FileStream(gtbPath, FileMode.Create, FileAccess.Write);
            FileStream fs2 = new FileStream(gpkPath, FileMode.Create, FileAccess.Write);
            BinaryWriter writer1 = new BinaryWriter(fs1);
            BinaryWriter writer2 = new BinaryWriter(fs2);

            uint sizeToNow = 0;
            uint offsetToNow = 0;
            writer1.Write(filecount);

            foreach (FileInfo fi in d.GetFiles())
            {
                if (Path.GetExtension(fi.FullName) != ".dwq")
                {
                    fs1.Dispose();
                    fs2.Dispose();
                    FileInfo deleteVpk = new FileInfo(gpkPath);
                    FileInfo deleteVtb = new FileInfo(gtbPath);
                    deleteVpk.Delete();
                    deleteVtb.Delete();
                    throw new Exception("Error:File extension " + fi.Extension +" not supported.");
                }
                writer1.Write(offsetToNow);
                offsetToNow = offsetToNow + (uint)Path.GetFileNameWithoutExtension(fi.FullName).Length + 1;

                byte[] buffer = File.ReadAllBytes(fi.FullName);
                writer2.Write(buffer);
                LogUtility.UpdateBar();
            }

            foreach (FileInfo fi in d.GetFiles())
            {
                writer1.Write(sizeToNow);
                sizeToNow += (uint)fi.Length;
                LogUtility.UpdateBar();
            }

            foreach (FileInfo fi in d.GetFiles())
            {
                writer1.Write(Encoding.ASCII.GetBytes(Path.GetFileNameWithoutExtension(fi.FullName)));
                writer1.Write('\0');
                LogUtility.UpdateBar();
            }

            if (version == "1")
            {
                //skip this
            }
            else
            {
                while (fs1.Position % 16 != 0)
                {
                    writer1.Write('\0');
                }
                ulong sizeToNowNew = 0;
                foreach (FileInfo fi in d.GetFiles())
                {
                    writer1.Write(sizeToNowNew);
                    sizeToNowNew += (ulong)fi.Length;
                }
                writer1.Write((ulong)0);
                writer1.Write(Encoding.ASCII.GetBytes("over2G!"));
                writer1.Write('\0');
            }

            fs1.Dispose();
            fs2.Dispose();
        }
    
    }
}
