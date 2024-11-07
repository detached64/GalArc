using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using Utility;
using Utility.Compression;
using Utility.Extensions;

namespace ArcFormats.NekoSDK
{
    public class DAT
    {
        private class Entry
        {
            public string Name { get; set; }
            public uint Offset { get; set; }
            public uint PackedSize { get; set; }
            public uint UnpackedSize { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            fs.Position = 0x88;
            int fileCount = br.ReadInt32() ^ 0xCACACA / 0x8c - 1;       // 0x8c bytes of 0x00 attached
            fs.Position = 0;
            var entries = new List<Entry>(fileCount);
            long pos = 0x80;
            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new Entry();
                entry.Name = br.ReadCString(ArcEncoding.Shift_JIS);     // sometimes the 0x80 namebuf contains invalid characters
                fs.Position = pos;                                      // example: 優遇接待#～孤島と6人のスク水っ娘たち～ アニメーション追加版 パケ版 - bgm.dat
                entry.UnpackedSize = br.ReadUInt32() ^ 0xCACACA;
                entry.PackedSize = br.ReadUInt32() ^ 0xCACACA;
                entry.Offset = br.ReadUInt32() ^ 0xCACACA;
                entries.Add(entry);
                pos += 0x8C;
            }
            Logger.InitBar(fileCount);
            Directory.CreateDirectory(folderPath);
            foreach (var entry in entries)
            {
                fs.Position = entry.Offset;
                byte[] data = br.ReadBytes((int)entry.PackedSize);
                data = Lzss.Decompress(data);
                string fileName = Path.Combine(folderPath, entry.Name);
                File.WriteAllBytes(fileName, data);
                Logger.UpdateBar();
                data = null;
            }
            fs.Dispose();
            br.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            FileInfo[] files = new DirectoryInfo(folderPath).GetFiles();
            Logger.InitBar(files.Length);
            uint dataOffset = (uint)(files.Length + 1) * 0x8C;
            foreach (FileInfo file in files)
            {
                bw.WritePaddedString(file.Name, 0x80);
                bw.Write((uint)file.Length ^ 0xCACACA);
                byte[] raw = File.ReadAllBytes(file.FullName);
                byte[] data = Lzss.Compress(raw);
                bw.Write((uint)data.Length ^ 0xCACACA);
                bw.Write(dataOffset ^ 0xCACACA);
                long pos = fw.Position;
                fw.Position = dataOffset;
                bw.Write(data);
                dataOffset += (uint)data.Length;
                fw.Position = pos;
                Logger.UpdateBar();
                raw = null;
                data = null;
            }
            fw.Dispose();
            bw.Dispose();
        }
    }
}
