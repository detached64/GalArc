using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GalArc.Models.Formats.AVGEngine;

internal class GXP : ArcFormat
{
    public override string Name => "GXP";
    public override string Description => "AVG Engine GXP Archive";
    public override bool CanWrite => true;

    private static readonly byte[] KnownKey = [0x40, 0x21, 0x28, 0x38, 0xA6, 0x6E, 0x43, 0xA5, 0x40, 0x21, 0x28, 0x38, 0xA6, 0x43, 0xA5, 0x64, 0x3E, 0x65, 0x24, 0x20, 0x46, 0x6E, 0x74,];

    public override void Unpack(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        if (br.ReadUInt32() != 0x505847)
            throw new InvalidArchiveException();
        fs.Position = 24;
        int fileCount = br.ReadInt32();
        fs.Position = 40;
        long baseOffset = br.ReadInt64();
        List<Entry> entries = new(fileCount);
        for (int i = 0; i < fileCount; i++)
        {
            uint entry_size = BitConverter.ToUInt32(Decrypt(br.ReadBytes(4)));
            fs.Position -= 4;
            byte[] entry_data = Decrypt(br.ReadBytes((int)entry_size));
            Entry entry = new();
            entry.Size = (uint)BitConverter.ToInt64(entry_data, 4);
            int nameLength = BitConverter.ToInt32(entry_data, 12) * 2;
            entry.Offset = (uint)(BitConverter.ToInt64(entry_data, 24) + baseOffset);
            entry.Name = Encoding.Unicode.GetString(entry_data, 32, nameLength);
            entry.Path = Path.Combine(folderPath, entry.Name);
            entries.Add(entry);
        }
        ProgressManager.SetMax(fileCount);
        foreach (Entry entry in entries)
        {
            fs.Position = entry.Offset;
            byte[] data = Decrypt(br.ReadBytes((int)entry.Size));
            Directory.CreateDirectory(Path.GetDirectoryName(entry.Path));
            File.WriteAllBytes(entry.Path, data);
            ProgressManager.Progress();
        }
    }

    public override void Pack(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        bw.Write(0x505847);
        bw.Write(0);
        bw.Write(0x10203040);
        bw.Write(1);
        bw.Write(0);
        bw.Write(1);
        FileInfo[] files = new DirectoryInfo(folderPath).GetFiles("*", SearchOption.AllDirectories);
        bw.Write(files.Length);
        bw.Write(new byte[20]);
        long offset = 0;
        foreach (FileInfo file in files)
        {
            using MemoryStream ms = new();
            using BinaryWriter mw = new(ms);
            mw.Write(0);
            mw.Write(file.Length);
            byte[] name = Encoding.Unicode.GetBytes(Path.GetRelativePath(folderPath, file.FullName).Replace('\\', '/'));
            mw.Write(name.Length / 2);
            mw.Write(new byte[8]);
            mw.Write(offset);
            offset += file.Length;
            mw.Write(name);
            mw.Write(new byte[8]);
            int length = (int)ms.Position;
            ms.Position = 0;
            mw.Write(length);
            bw.Write(Decrypt(ms.ToArray()));
        }
        long dataOffset = fw.Position;
        ProgressManager.SetMax(files.Length);
        foreach (FileInfo file in files)
        {
            byte[] data = File.ReadAllBytes(file.FullName);
            bw.Write(Decrypt(data));
            ProgressManager.Progress();
        }
        long totalSize = fw.Position;
        fw.Position = 28;
        bw.Write((int)dataOffset - 0x30);
        bw.Write(totalSize - dataOffset);
        bw.Write(dataOffset);
    }

    private static byte[] Decrypt(byte[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] ^= (byte)(i ^ KnownKey[i % KnownKey.Length]);
        }
        return data;
    }
}
