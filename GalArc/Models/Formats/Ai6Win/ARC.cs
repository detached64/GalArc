using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.Infrastructure.Logging;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace GalArc.Models.Formats.Ai6Win;

internal class ARC : ArcFormat, IPackConfigurable
{
    public override string Name => "ARC";
    public override string Description => "Ai6Win Archive";
    public override bool CanWrite => true;

    private Ai6WinARCPackOptions _packOptions;
    public ArcOptions PackOptions => _packOptions ??= new Ai6WinARCPackOptions();

    private UserControl _packWidget;
    public UserControl PackWidget => _packWidget ??= new Ai6WinARCPackWidget()
    {
        DataContext = PackOptions
    };

    private class ArcEntry : PackedEntry
    {
        public string FullPath { get; set; }
    }

    public override void Unpack(string filePath, string folderPath)
    {
        List<Action> actions =
        [
            () => UnpackV3(filePath, folderPath),
            () => UnpackV2(filePath, folderPath),
            () => UnpackV1(filePath, folderPath),
        ];
        foreach (Action action in actions)
        {
            try
            {
                action();
                return;
            }
            catch
            { }
        }
        throw new InvalidArchiveException();
    }

    public override void Pack(string folderPath, string filePath)
    {
        switch (_packOptions.Version)
        {
            case 1:
                PackV1(folderPath, filePath);
                break;
            case 2:
                PackV2(folderPath, filePath);
                break;
            case 3:
                PackV3(folderPath, filePath);
                break;
        }
    }

    private static void UnpackV1(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        int fileCount = br.ReadInt32();

        List<ArcEntry> l = [];
        for (int i = 0; i < fileCount; i++)
        {
            ArcEntry entry = new();
            entry.Name = ArcEncoding.Shift_JIS.GetString(br.ReadBytes(32)).TrimEnd('\0');
            if (entry.Name.ContainsInvalidChars())
            {
                throw new Exception();
            }
            entry.FullPath = Path.Combine(folderPath, entry.Name);
            entry.Offset = br.ReadUInt32();
            entry.Size = br.ReadUInt32();
            entry.IsPacked = false;
            l.Add(entry);
        }

        ProgressManager.SetMax(fileCount);
        Logger.ShowVersion("arc", 1);

        ExtractData(l, br);
    }

    private static void UnpackV2(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        int fileCount = br.ReadInt32();

        List<ArcEntry> l = [];
        for (int i = 0; i < fileCount; i++)
        {
            ArcEntry entry = new();
            byte[] nameBuf = br.ReadBytes(260);
            int nameLen = Array.IndexOf<byte>(nameBuf, 0);
            if (nameLen == -1)
            {
                nameLen = nameBuf.Length;
            }

            byte key = (byte)(nameLen + 1);
            for (int j = 0; j < nameLen; j++)
            {
                nameBuf[j] -= key;
                key--;
            }
            entry.Name = ArcEncoding.Shift_JIS.GetString(nameBuf, 0, nameLen);
            if (entry.Name.ContainsInvalidChars())
            {
                throw new Exception();
            }
            entry.FullPath = Path.Combine(folderPath, entry.Name);
            entry.Size = BigEndian.Convert(br.ReadUInt32());
            entry.UnpackedSize = BigEndian.Convert(br.ReadUInt32());
            entry.Offset = BigEndian.Convert(br.ReadUInt32());
            entry.IsPacked = entry.Size != entry.UnpackedSize;
            l.Add(entry);
        }

        ProgressManager.SetMax(fileCount);
        Logger.ShowVersion("arc", 2);

        ExtractData(l, br);
    }

    private static void UnpackV3(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        int indexSize = br.ReadInt32();
        List<ArcEntry> l = [];

        while (fs.Position < 4 + indexSize)
        {
            int nameLen = br.ReadByte();
            ArcEntry entry = new();
            byte[] nameBuf = br.ReadBytes(nameLen);
            byte key = (byte)nameLen;
            for (int i = 0; i < nameBuf.Length; i++)
            {
                nameBuf[i] += key--;
            }
            entry.Name = ArcEncoding.Shift_JIS.GetString(nameBuf);
            if (entry.Name.ContainsInvalidChars())
            {
                throw new Exception();
            }
            entry.FullPath = Path.Combine(folderPath, entry.Name);
            entry.Size = BigEndian.Convert(br.ReadUInt32());
            entry.UnpackedSize = BigEndian.Convert(br.ReadUInt32());
            entry.Offset = BigEndian.Convert(br.ReadUInt32());
            entry.IsPacked = entry.Size != entry.UnpackedSize;
            l.Add(entry);
        }

        ProgressManager.SetMax(l.Count);
        Logger.ShowVersion("arc", 3);

        ExtractData(l, br);
    }

    private static void ExtractData(List<ArcEntry> l, BinaryReader br)
    {
        for (int i = 0; i < l.Count; i++)
        {
            byte[] data = br.ReadBytes((int)l[i].Size);
            if (l[i].IsPacked)
            {
                data = LzssHelper.Decompress(data);
            }
            File.WriteAllBytes(Path.Combine(l[i].FullPath), data);
            data = null;
            ProgressManager.Progress();
        }
    }

    private static void PackV1(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        FileInfo[] files = new DirectoryInfo(folderPath).GetFiles();
        int fileCount = files.Length;
        bw.Write(fileCount);
        uint baseOffset = 4 + (40 * (uint)fileCount);
        ProgressManager.SetMax(fileCount);
        foreach (FileInfo file in files)
        {
            bw.WritePaddedString(file.Name, 32);
            bw.Write(baseOffset);
            uint size = (uint)file.Length;
            bw.Write(size);
            baseOffset += size;
        }
        foreach (FileInfo file in files)
        {
            byte[] data = File.ReadAllBytes(file.FullName);
            bw.Write(data);
            data = null;
            ProgressManager.Progress();
        }
    }

    private void PackV2(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        FileInfo[] files = new DirectoryInfo(folderPath).GetFiles();
        int fileCount = files.Length;
        bw.Write(fileCount);
        uint dataOffset = 4 + (272 * (uint)fileCount);
        ProgressManager.SetMax(fileCount);
        foreach (FileInfo file in files)
        {
            byte[] nameBuffer = Utility.GetPaddedBytes(file.Name, 260);
            int nameLen = Array.IndexOf<byte>(nameBuffer, 0);
            if (nameLen == -1)
            {
                nameLen = nameBuffer.Length;
            }

            byte key = (byte)(nameLen + 1);
            for (int i = 0; i < nameLen; i++)
            {
                nameBuffer[i] += key--;
            }
            bw.Write(nameBuffer);
            uint unpackedSize = (uint)file.Length;
            byte[] data = File.ReadAllBytes(file.FullName);
            if (_packOptions.CompressContents)
            {
                data = LzssHelper.Compress(data);
            }
            uint packedSize = (uint)data.Length;
            bw.Write(BigEndian.Convert(packedSize));
            bw.Write(BigEndian.Convert(unpackedSize));
            bw.Write(BigEndian.Convert(dataOffset));
            long pos = fw.Position;
            fw.Position = dataOffset;
            bw.Write(data);
            fw.Position = pos;
            dataOffset += packedSize;
            data = null;
            ProgressManager.Progress();
        }
    }

    private void PackV3(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        FileInfo[] files = new DirectoryInfo(folderPath).GetFiles();
        int fileCount = files.Length;
        ProgressManager.SetMax(fileCount);
        uint dataOffset = (uint)(4 + Utility.GetNameLengthSum(files, ArcEncoding.Shift_JIS) + (13 * fileCount));
        bw.Write(dataOffset - 4);

        foreach (FileInfo file in files)
        {
            byte[] nameBuf = ArcEncoding.Shift_JIS.GetBytes(file.Name);
            byte key = (byte)nameBuf.Length;
            bw.Write(key);
            for (int i = 0; i < nameBuf.Length; i++)
            {
                nameBuf[i] -= key--;
            }
            bw.Write(nameBuf);
            uint unpackedSize = (uint)file.Length;
            byte[] data = File.ReadAllBytes(file.FullName);
            if (_packOptions.CompressContents)
            {
                data = LzssHelper.Compress(data);
            }
            uint packedSize = (uint)data.Length;
            bw.Write(BigEndian.Convert(packedSize));
            bw.Write(BigEndian.Convert(unpackedSize));
            bw.Write(BigEndian.Convert(dataOffset));
            long pos = fw.Position;
            fw.Position = dataOffset;
            bw.Write(data);
            fw.Position = pos;
            dataOffset += packedSize;
            data = null;
            ProgressManager.Progress();
        }
    }
}

internal partial class Ai6WinARCPackOptions : ArcOptions
{
    [ObservableProperty]
    private IReadOnlyList<int> versions = [1, 2, 3];
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCompressContentsVisible))]
    private int version = 3;
    [ObservableProperty]
    private bool compressContents = true;
    public bool IsCompressContentsVisible => Version != 1;
}
