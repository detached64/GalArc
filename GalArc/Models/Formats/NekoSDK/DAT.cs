using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System.Collections.Generic;
using System.IO;

namespace GalArc.Models.Formats.NekoSDK;

internal class DAT : ArcFormat
{
    public override string Name => "DAT";
    public override string Description => "NekoSDK DAT Archive";
    public override bool CanWrite => true;

    public override void Unpack(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        fs.Position = 0x88;
        int fileCount = br.ReadInt32() ^ (0xCACACA / 0x8c) - 1;       // 0x8c bytes of 0x00 attached
        fs.Position = 0;
        List<PackedEntry> entries = new(fileCount);
        long pos = 0x80;
        for (int i = 0; i < fileCount; i++)
        {
            PackedEntry entry = new();
            entry.Name = br.ReadCString();                          // sometimes the 0x80 namebuf contains invalid characters
            fs.Position = pos;                                      // example: 優遇接待#～孤島と6人のスク水っ娘たち～ アニメーション追加版 パケ版 - bgm.dat
            entry.UnpackedSize = br.ReadUInt32() ^ 0xCACACA;
            entry.Size = br.ReadUInt32() ^ 0xCACACA;
            entry.Offset = br.ReadUInt32() ^ 0xCACACA;
            entries.Add(entry);
            pos += 0x8C;
        }
        ProgressManager.SetMax(fileCount);
        foreach (PackedEntry entry in entries)
        {
            fs.Position = entry.Offset;
            byte[] data = br.ReadBytes((int)entry.Size);
            data = LzssHelper.Decompress(data);
            string fileName = Path.Combine(folderPath, entry.Name);
            File.WriteAllBytes(fileName, data);
            ProgressManager.Progress();
            data = null;
        }
    }

    public override void Pack(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        FileInfo[] files = new DirectoryInfo(folderPath).GetFiles();
        ProgressManager.SetMax(files.Length);
        uint dataOffset = (uint)(files.Length + 1) * 0x8C;
        foreach (FileInfo file in files)
        {
            bw.WritePaddedString(file.Name, 0x80);
            bw.Write((uint)file.Length ^ 0xCACACA);
            byte[] raw = File.ReadAllBytes(file.FullName);
            byte[] data = LzssHelper.Compress(raw);
            bw.Write((uint)data.Length ^ 0xCACACA);
            bw.Write(dataOffset ^ 0xCACACA);
            long pos = fw.Position;
            fw.Position = dataOffset;
            bw.Write(data);
            dataOffset += (uint)data.Length;
            fw.Position = pos;
            ProgressManager.Progress();
            raw = null;
            data = null;
        }
    }
}
