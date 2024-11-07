using ArcFormats.Templates;
using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Utility;
using Utility.Extensions;

namespace ArcFormats.Triangle
{
    public class CGF
    {
        public static UserControl PackExtraOptions = new VersionOnly("1");

        private class Entry
        {
            internal string Name { get; set; }
            internal uint Offset { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            int fileCount = br.ReadInt32();

            fs.Position = 20;
            uint offset1 = br.ReadUInt32();
            fs.Position = 32;
            uint offset2 = br.ReadUInt32();
            fs.Dispose();
            br.Dispose();

            if (offset1 == 4 + 20 * (uint)fileCount)
            {
                cgfV1_unpack(filePath, folderPath);
            }
            else if ((offset2 & ~0xc0000000) == 4 + 32 * (uint)fileCount)
            {
                Logger.Error("cgf v2 archive not implemented.");
            }
            else
            {
                Logger.ErrorInvalidArchive();
            }
        }

        public void Pack(string folderPath, string filePath)
        {
            if (Config.Version == "1")
            {
                cgfV1_pack(folderPath, filePath);
            }
            else if (Config.Version == "2")
            {

            }
        }

        private static void cgfV1_unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            int fileCount = br.ReadInt32();
            Directory.CreateDirectory(folderPath);
            Logger.InitBar(fileCount);
            List<Entry> entries = new List<Entry>();

            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new Entry();
                long pos = fs.Position;
                entry.Name = br.ReadCString(ArcEncoding.Shift_JIS);
                fs.Position = pos + 16;
                entry.Offset = br.ReadUInt32();
                entries.Add(entry);
            }

            for (int i = 0; i < fileCount - 1; i++)
            {
                byte[] buf = br.ReadBytes((int)(entries[i + 1].Offset - entries[i].Offset));
                File.WriteAllBytes(Path.Combine(folderPath, entries[i].Name), buf);
                buf = null;
                Logger.UpdateBar();
            }
            byte[] bufLast = br.ReadBytes((int)(fs.Length - entries[fileCount - 1].Offset));
            File.WriteAllBytes(Path.Combine(folderPath, entries[fileCount - 1].Name), bufLast);
            bufLast = null;
            Logger.UpdateBar();
            fs.Dispose();
            br.Dispose();
        }

        private static void cgfV1_pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            DirectoryInfo d = new DirectoryInfo(folderPath);
            FileInfo[] files = d.GetFiles();
            int fileCount = files.Length;
            bw.Write(fileCount);
            uint baseOffset = 4 + 20 * (uint)fileCount;
            Logger.InitBar(fileCount);
            foreach (FileInfo file in files)
            {
                bw.WritePaddedString(file.Name, 16);
                bw.Write(baseOffset);
                baseOffset += (uint)file.Length;
            }
            foreach (FileInfo file in files)
            {
                byte[] data = File.ReadAllBytes(file.FullName);
                bw.Write(data);
                data = null;
                Logger.UpdateBar();
            }
            fw.Dispose();
            bw.Dispose();
        }
    }
}
