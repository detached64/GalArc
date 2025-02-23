using GalArc.Controls;
using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using Utility.Extensions;

namespace ArcFormats.Triangle
{
    public class CGF : ArchiveFormat
    {
        private static readonly Lazy<OptionsTemplate> _lazyUnpackOptions = new Lazy<OptionsTemplate>(() => new VersionOnly("1"));
        public static OptionsTemplate PackExtraOptions => _lazyUnpackOptions.Value;

        public override void Unpack(string filePath, string folderPath)
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
                UnpackV1(filePath, folderPath);
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

        public override void Pack(string folderPath, string filePath)
        {
            switch (PackExtraOptions.Version)
            {
                case "1":
                    PackV1(folderPath, filePath);
                    break;
                case "2":
                    throw new NotImplementedException();
            }
        }

        private void UnpackV1(string filePath, string folderPath)
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
                entry.Name = br.ReadCString();
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

        private void PackV1(string folderPath, string filePath)
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
