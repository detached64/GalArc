using Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility;

namespace ArcFormats.AdvHD
{
    public class PNA
    {
        private class Header
        {
            internal string magic { get; set; }
            internal uint unknown1 { get; set; }
            internal uint width { get; set; }
            internal uint height { get; set; }
            internal uint fileCount { get; set; }
        }

        private class Entry
        {
            internal uint fileType { get; set; }
            internal uint fileNumber { get; set; }
            internal uint offsetX { get; set; }
            internal uint offsetY { get; set; }
            internal uint width { get; set; }
            internal uint height { get; set; }
            internal uint add1 { get; set; }
            internal uint add2 { get; set; }
            internal uint remark3 { get; set; }
            internal uint fileSize { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            Header header = new Header();
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            header.magic = Encoding.UTF8.GetString(br.ReadBytes(4));
            if (header.magic != "PNAP" || Path.GetExtension(filePath) != ".pna")
            {
                LogUtility.ErrorInvalidArchive();
            }

            header.unknown1 = br.ReadUInt32();
            header.width = br.ReadUInt32();
            header.height = br.ReadUInt32();
            header.fileCount = br.ReadUInt32();

            Directory.CreateDirectory(folderPath);
            List<Entry> l = new List<Entry>();

            for (int i = 0; i < header.fileCount; i++)
            {
                Entry entry = new Entry();
                entry.fileType = br.ReadUInt32();
                entry.fileNumber = header.fileCount - br.ReadUInt32();
                entry.offsetX = br.ReadUInt32();
                entry.offsetY = br.ReadUInt32();
                entry.width = br.ReadUInt32();
                entry.height = br.ReadUInt32();
                entry.add1 = br.ReadUInt32();
                entry.add2 = br.ReadUInt32();
                entry.remark3 = br.ReadUInt32();
                entry.fileSize = br.ReadUInt32();
                l.Add(entry);
            }
            int validCount = 0;
            for (int i = 0; i < header.fileCount; i++)
            {
                if (l[i].fileType == 1 || l[i].fileType == 2)
                {
                    continue;
                }
                byte[] buffer = br.ReadBytes((int)l[i].fileSize);
                File.WriteAllBytes(Path.Combine(folderPath, Path.GetFileNameWithoutExtension(filePath) + "_" + l[i].fileNumber.ToString("000") + ".png"), buffer);
                validCount++;
            }
            fs.Dispose();
            br.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            string spath = folderPath + ".pna";
            string tpath = filePath;
            if (tpath == spath)
            {
                throw new Exception("Output file path is the same as the original file path.");
            }
            if (!File.Exists(spath))
            {
                LogUtility.ErrorNeedOriginalFile(".pna");
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
            }

            fs.Dispose();
            bw.Dispose();
            br.Dispose();
        }
    }
}