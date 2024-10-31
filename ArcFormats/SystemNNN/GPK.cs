using ArcFormats.Templates;
using GalArc.Logs;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Utility.Extensions;

namespace ArcFormats.SystemNNN
{
    public class GPK
    {
        public static UserControl PackExtraOptions = new VersionOnly("1/2");

        private class Entry
        {
            public uint Size { get; set; }
            public uint Offset { get; set; }
            public string Path { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            //init
            string gpkPath;
            string gtbPath;

            gpkPath = filePath;
            gtbPath = Path.ChangeExtension(filePath, ".gtb");

            if (!File.Exists(gtbPath))
            {
                Logger.ErrorNeedAnotherFile(Path.GetFileName(gtbPath));
            }

            //open&make dir
            FileStream fs1 = File.OpenRead(gtbPath);
            BinaryReader br1 = new BinaryReader(fs1);
            uint filecount = br1.ReadUInt32();
            Logger.InitBar(filecount);

            FileStream fs2 = File.OpenRead(gpkPath);
            BinaryReader br2 = new BinaryReader(fs2);

            Directory.CreateDirectory(folderPath);

            uint thisPos = 0;
            uint maxPos = 0;
            //process
            //i~n-1
            for (int i = 1; i < filecount; i++)
            {
                Entry entry = new Entry();

                //offset
                entry.Offset = br1.ReadUInt32();

                fs1.Seek(4 * filecount - 4, SeekOrigin.Current);

                //size
                uint size1 = br1.ReadUInt32();
                uint size2 = br1.ReadUInt32();
                entry.Size = size2 - size1;

                fs1.Seek(4 + 8 * filecount + entry.Offset, SeekOrigin.Begin);

                entry.Path = Path.Combine(folderPath, br1.ReadCString(Encoding.UTF8) + ".dwq");
                thisPos = (uint)fs1.Position;
                maxPos = Math.Max(thisPos, maxPos);

                //get file content
                byte[] buffer = br2.ReadBytes((int)entry.Size);

                //write file
                File.WriteAllBytes(entry.Path, buffer);
                buffer = null;
                fs1.Seek(4 + 4 * i, SeekOrigin.Begin);

                Logger.UpdateBar();
            }

            uint offset = br1.ReadUInt32();
            uint gtbSize = (uint)new FileInfo(gtbPath).Length;
            uint gpkSize = (uint)new FileInfo(gpkPath).Length;
            fs1.Seek(8 * filecount, SeekOrigin.Begin);
            uint sizeWithoutLast = br1.ReadUInt32();
            fs1.Seek(offset + 4 + 8 * filecount, SeekOrigin.Begin);
            Entry last = new Entry();
            last.Offset = gtbSize - (offset + 4 + 8 * filecount) - 1;
            last.Path = Path.Combine(folderPath, br1.ReadCString(Encoding.UTF8) + ".dwq");
            last.Size = gpkSize - sizeWithoutLast;

            thisPos = (uint)fs1.Position;
            maxPos = Math.Max(thisPos, maxPos);
            byte[] buf = br2.ReadBytes((int)last.Size);
            File.WriteAllBytes(last.Path, buf);
            buf = null;
            Logger.UpdateBar();
            if (maxPos == gtbSize)
            {
                Logger.ShowVersion("gpk", 1);
            }
            else
            {
                Logger.ShowVersion("gpk", 2);
            }

            fs1.Dispose();
            fs2.Dispose();
            br1.Dispose();
            br2.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            DirectoryInfo d = new DirectoryInfo(folderPath);
            FileInfo[] files = d.GetFiles("*.dwq");
            int filecount = files.Length;
            Logger.InitBar(filecount);
            LogWindow.Instance.bar.Maximum = 3 * filecount;

            string gpkPath = filePath;
            string gtbPath = filePath.Contains(".gpk") ? gpkPath.Replace(".gpk", ".gtb") : gpkPath + ".gtb";

            FileStream fs1 = File.Create(gtbPath);
            FileStream fs2 = File.Create(gpkPath);
            BinaryWriter writer1 = new BinaryWriter(fs1);
            BinaryWriter writer2 = new BinaryWriter(fs2);

            uint sizeToNow = 0;
            uint offsetToNow = 0;
            writer1.Write(filecount);

            foreach (FileInfo file in files)
            {
                writer1.Write(offsetToNow);
                offsetToNow = offsetToNow + (uint)Path.GetFileNameWithoutExtension(file.FullName).Length + 1;

                byte[] buffer = File.ReadAllBytes(file.FullName);
                writer2.Write(buffer);
                buffer = null;
                Logger.UpdateBar();
            }

            foreach (FileInfo file in files)
            {
                writer1.Write(sizeToNow);
                sizeToNow += (uint)file.Length;
                Logger.UpdateBar();
            }

            foreach (FileInfo file in files)
            {
                writer1.Write(Encoding.ASCII.GetBytes(Path.GetFileNameWithoutExtension(file.FullName)));
                writer1.Write('\0');
                Logger.UpdateBar();
            }

            if (Config.Version == "1")
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
                foreach (FileInfo file in files)
                {
                    writer1.Write(sizeToNowNew);
                    sizeToNowNew += (ulong)file.Length;
                }
                writer1.Write((ulong)0);
                writer1.Write(Encoding.ASCII.GetBytes("over2G!\0"));
            }

            fs1.Dispose();
            fs2.Dispose();
        }
    }
}