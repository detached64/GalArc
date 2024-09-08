using Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility;

namespace ArcFormats.AdvHD
{
    public static class PNA
    {
        private struct AdvHD_pna_header
        {
            internal string magic { get; set; }
            internal uint unknown1 { get; set; }
            internal uint width { get; set; }
            internal uint height { get; set; }
            internal uint fileCount { get; set; }
        }

        private struct AdvHD_pna_entry
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

        public static void Unpack(string filePath, string folderPath, Encoding encoding)
        {
            AdvHD_pna_header header = new AdvHD_pna_header();
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            header.magic = Encoding.UTF8.GetString(br.ReadBytes(4));
            if (header.magic != "PNAP" || Path.GetExtension(filePath) != ".pna")
            {
                LogUtility.Error_NotValidArchive();
            }

            header.unknown1 = br.ReadUInt32();
            header.width = br.ReadUInt32();
            header.height = br.ReadUInt32();
            header.fileCount = br.ReadUInt32();

            Directory.CreateDirectory(folderPath);
            List<AdvHD_pna_entry> l = new List<AdvHD_pna_entry>();

            for (int i = 0; i < header.fileCount; i++)
            {
                AdvHD_pna_entry entry = new AdvHD_pna_entry();
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

                FileStream fw = new FileStream(folderPath + "\\" + Path.GetFileNameWithoutExtension(filePath) + "_" + l[i].fileNumber.ToString("000") + ".png", FileMode.Create, FileAccess.Write);
                byte[] buffer = br.ReadBytes((int)l[i].fileSize);
                fw.Write(buffer, 0, buffer.Length);
                fw.Dispose();
                validCount++;
            }
            fs.Dispose();
            br.Dispose();
        }

        public static void Pack(string folderPath, string filePath, string version, Encoding encoding)
        {
            string spath = folderPath + ".pna";
            string tpath = filePath;
            if (tpath == spath)
            {
                throw new Exception("Output file path is the same as the original file path.");
            }
            if (!File.Exists(spath))
            {
                LogUtility.Error_NeedOriginalFile(".pna");
            }

            File.Copy(spath, tpath, true);

            int fileCount = Utilities.GetFileCount_All(folderPath);

            FileStream fs = new FileStream(tpath, FileMode.Open, FileAccess.ReadWrite);
            BinaryWriter bw = new BinaryWriter(fs);
            BinaryReader br = new BinaryReader(fs);
            fs.Seek(16, SeekOrigin.Begin);
            uint fileCountWithInvalid = br.ReadUInt32();

            DirectoryInfo dir = new DirectoryInfo(folderPath);
            foreach (FileInfo fi in dir.GetFiles())
            {
                while (br.ReadUInt32() != 0)
                {
                    fs.Seek(-4 + 40, SeekOrigin.Current);
                }
                fs.Seek(-4 + 36, SeekOrigin.Current);
                bw.Write((uint)new FileInfo(fi.FullName).Length);
                //fs.Seek(4, SeekOrigin.Current);
            }
            fs.Seek(20 + 40 * fileCountWithInvalid, SeekOrigin.Begin);

            foreach (FileInfo fi in dir.GetFiles())
            {
                byte[] buffer = File.ReadAllBytes(fi.FullName);
                bw.Write(buffer);
            }

            fs.Dispose();
            bw.Dispose();
            br.Dispose();
        }
    }
}