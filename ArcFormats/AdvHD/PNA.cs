using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility.Exceptions;

namespace ArcFormats.AdvHD
{
    public class PNA : ArchiveFormat
    {
        private class Header
        {
            internal string Magic { get; set; }
            internal uint Unknown1 { get; set; }
            internal uint Width { get; set; }
            internal uint Height { get; set; }
            internal uint FileCount { get; set; }
        }

        private class Entry
        {
            internal uint FileType { get; set; }
            internal uint FileNumber { get; set; }
            internal uint OffsetX { get; set; }
            internal uint OffsetY { get; set; }
            internal uint Width { get; set; }
            internal uint Height { get; set; }
            internal uint Add1 { get; set; }
            internal uint Add2 { get; set; }
            internal uint Remark3 { get; set; }
            internal uint FileSize { get; set; }
        }

        public override void Unpack(string filePath, string folderPath)
        {
            Header header = new Header();
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            header.Magic = Encoding.UTF8.GetString(br.ReadBytes(4));
            if (header.Magic != "PNAP")
            {
                throw new InvalidArchiveException();
            }

            header.Unknown1 = br.ReadUInt32();
            header.Width = br.ReadUInt32();
            header.Height = br.ReadUInt32();
            header.FileCount = br.ReadUInt32();

            Directory.CreateDirectory(folderPath);
            List<Entry> entries = new List<Entry>((int)header.FileCount);

            for (int i = 0; i < header.FileCount; i++)
            {
                Entry entry = new Entry();
                entry.FileType = br.ReadUInt32();
                entry.FileNumber = header.FileCount - br.ReadUInt32();
                entry.OffsetX = br.ReadUInt32();
                entry.OffsetY = br.ReadUInt32();
                entry.Width = br.ReadUInt32();
                entry.Height = br.ReadUInt32();
                entry.Add1 = br.ReadUInt32();
                entry.Add2 = br.ReadUInt32();
                entry.Remark3 = br.ReadUInt32();
                entry.FileSize = br.ReadUInt32();
                entries.Add(entry);
            }
            int validCount = 0;
            for (int i = 0; i < header.FileCount; i++)
            {
                if (entries[i].FileType == 1 || entries[i].FileType == 2)
                {
                    continue;
                }
                byte[] buffer = br.ReadBytes((int)entries[i].FileSize);
                File.WriteAllBytes(Path.Combine(folderPath, Path.GetFileNameWithoutExtension(filePath) + "_" + entries[i].FileNumber.ToString("000") + ".png"), buffer);
                buffer = null;
                validCount++;
            }
            fs.Dispose();
            br.Dispose();
        }

        public override void Pack(string folderPath, string filePath)
        {
            string spath = folderPath + ".pna";
            string tpath = filePath;
            if (tpath == spath)
            {
                throw new Exception("Output file path is the same as the original file path.");
            }
            if (!File.Exists(spath))
            {
                Logger.ErrorNeedOriginalFile(Path.GetFileName(spath));
            }

            File.Copy(spath, tpath, true);

            string[] files = Directory.GetFiles(folderPath);
            int fileCount = files.Length;

            FileStream fs = new FileStream(tpath, FileMode.Open, FileAccess.ReadWrite);
            BinaryWriter bw = new BinaryWriter(fs);
            BinaryReader br = new BinaryReader(fs);
            fs.Seek(16, SeekOrigin.Begin);
            uint fileCountWithInvalid = br.ReadUInt32();
            foreach (string file in files)
            {
                while (br.ReadUInt32() != 0)
                {
                    fs.Seek(-4 + 40, SeekOrigin.Current);
                }
                fs.Seek(-4 + 36, SeekOrigin.Current);
                bw.Write((uint)new FileInfo(file).Length);
                //fs.Seek(4, SeekOrigin.Current);
            }
            fs.Seek(20 + 40 * fileCountWithInvalid, SeekOrigin.Begin);

            foreach (string file in files)
            {
                byte[] buffer = File.ReadAllBytes(file);
                bw.Write(buffer);
                buffer = null;
            }

            fs.Dispose();
            bw.Dispose();
            br.Dispose();
        }
    }
}