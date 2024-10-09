using Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utility;

namespace ArcFormats.InnocentGrey
{
    public class DAT
    {
        private struct Header
        {
            public string magic { get; set; }       //"PACKDAT."
            public uint fileCount { get; set; }
            public uint fileCount1 { get; set; }
        }

        private struct Entry
        {
            public string fileName { get; set; }
            public uint offset { get; set; }
            public uint fileType { get; set; }
            public uint fileSize_uncompressed { get; set; }
            public uint fileSize_compressed { get; set; }
            public bool isCompressed { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            if (Encoding.ASCII.GetString(br.ReadBytes(7)) != "PACKDAT" || br.ReadByte() != '\x2e')
            {
                LogUtility.ErrorInvalidArchive();
            }
            Header header = new Header();
            header.fileCount = br.ReadUInt32();
            header.fileCount1 = br.ReadUInt32();
            if (header.fileCount != header.fileCount1)
            {
                LogUtility.ErrorInvalidArchive();
            }

            LogUtility.InitBar(header.fileCount);
            List<Entry> entries = new List<Entry>();
            Directory.CreateDirectory(folderPath);
            for (int i = 0; i < header.fileCount; i++)
            {
                Entry entry = new Entry();
                entry.fileName = Encoding.ASCII.GetString(br.ReadBytes(32)).TrimEnd('\0');
                entry.offset = br.ReadUInt32();
                entry.fileType = br.ReadUInt32();
                entry.fileSize_uncompressed = br.ReadUInt32();
                entry.fileSize_compressed = br.ReadUInt32();
                entry.isCompressed = entry.fileSize_compressed != entry.fileSize_uncompressed;

                if (entry.isCompressed)     //skip compressed data for now
                {
                    throw new NotImplementedException("Compressed data detected.Temporarily not supported.");
                }
                entries.Add(entry);
            }

            for (int i = 0; i < header.fileCount; i++)
            {
                fs.Position = entries[i].offset;//have to seek manually,or it will read the wrong data
                byte[] data = br.ReadBytes((int)entries[i].fileSize_uncompressed);
                byte key = (byte)(Path.GetExtension(entries[i].fileName) == ".s" ? 0xFF : 0);
                data = Xor.xor(data, key);
                File.WriteAllBytes(Path.Combine(folderPath, entries[i].fileName), data);
                LogUtility.UpdateBar();
            }
            br.Dispose();
            fs.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            DirectoryInfo d = new DirectoryInfo(folderPath);
            Header header = new Header();
            header.magic = "PACKDAT";
            header.fileCount = (uint)d.GetFiles("*.*", SearchOption.TopDirectoryOnly).Count();
            header.fileCount1 = header.fileCount;
            LogUtility.InitBar(header.fileCount);
            bw.Write(Encoding.ASCII.GetBytes(header.magic));
            bw.Write('\x2e');
            bw.Write(header.fileCount);
            bw.Write(header.fileCount1);
            uint dataOffset = 16 + header.fileCount * 48;

            foreach (FileInfo file in d.GetFiles("*.*", SearchOption.TopDirectoryOnly))
            {
                bw.Write(Encoding.ASCII.GetBytes(file.Name.PadRight(32, '\0')));
                bw.Write(dataOffset);
                dataOffset += (uint)file.Length;
                bw.Write(0x20000000);
                bw.Write((uint)file.Length);
                bw.Write((uint)file.Length);
            }

            foreach (FileInfo file in d.GetFiles("*.*", SearchOption.TopDirectoryOnly))
            {
                byte[] data = File.ReadAllBytes(file.FullName);
                byte key = (byte)(Path.GetExtension(file.Name) == ".s" ? 0xFF : 0);
                data = Xor.xor(data, key);
                bw.Write(data);
                LogUtility.UpdateBar();
            }
            bw.Dispose();
            fw.Dispose();
        }
    }
}