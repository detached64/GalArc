using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace GalArc.Models.Formats.Triangle;

internal class CGF : ArcFormat, IPackConfigurable
{
    public override string Name => "CGF";
    public override string Description => "Triangle CGF Archive";
    public override bool CanWrite => true;

    private TriangleCGFPackOptions _packOptions;
    public ArcOptions PackOptions => _packOptions ??= new TriangleCGFPackOptions();

    public override void Unpack(string filePath, string folderPath)
    {
        FileStream fs = File.OpenRead(filePath);
        BinaryReader br = new(fs);
        int fileCount = br.ReadInt32();

        fs.Position = 20;
        uint offset1 = br.ReadUInt32();
        fs.Position = 32;
        uint offset2 = br.ReadUInt32();
        fs.Dispose();
        br.Dispose();

        if (offset1 == 4 + (20 * (uint)fileCount))
        {
            UnpackV1(filePath, folderPath);
        }
        else if ((offset2 & ~0xc0000000) == 4 + (32 * (uint)fileCount))
        {
            throw new InvalidVersionException(InvalidVersionType.NotSupported);
        }
        else
        {
            throw new InvalidArchiveException();
        }
    }

    public override void Pack(string folderPath, string filePath)
    {
        switch (_packOptions.Version)
        {
            case 1:
                PackV1(folderPath, filePath);
                break;
            case 2:
                throw new NotImplementedException();
        }
    }

    private static void UnpackV1(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        int fileCount = br.ReadInt32();
        ProgressManager.SetMax(fileCount);
        List<Entry> entries = [];

        for (int i = 0; i < fileCount; i++)
        {
            Entry entry = new();
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
            ProgressManager.Progress();
        }
        byte[] bufLast = br.ReadBytes((int)(fs.Length - entries[fileCount - 1].Offset));
        File.WriteAllBytes(Path.Combine(folderPath, entries[fileCount - 1].Name), bufLast);
        bufLast = null;
        ProgressManager.Progress();
    }

    private static void PackV1(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        DirectoryInfo d = new(folderPath);
        FileInfo[] files = d.GetFiles();
        int fileCount = files.Length;
        bw.Write(fileCount);
        uint baseOffset = 4 + (20 * (uint)fileCount);
        ProgressManager.SetMax(fileCount);
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
            ProgressManager.Progress();
        }
    }
}

internal partial class TriangleCGFPackOptions : ArcOptions
{
    [ObservableProperty]
    private int version = 1;
    [ObservableProperty]
    private IReadOnlyList<int> versions = [1];
}