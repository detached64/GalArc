using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.Infrastructure.Logging;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GalArc.Models.Formats.BiShop;

internal class BSA : ArcFormat, IPackConfigurable
{
    public override string Name => "BSA";
    public override string Description => "BiShop Archive";
    public override bool CanWrite => true;

    private BiShopBSAPackOptions _packOptions;
    public ArcOptions PackOptions => _packOptions ??= new BiShopBSAPackOptions();

    private readonly byte[] Magic = Utility.HexStringToByteArray("4253417263000000");

    private readonly List<string> path = [];

    private int realCount;

    private string rootDir = string.Empty;

    private int fileCount;

    private class BsaEntry : Entry
    {
        internal uint NameOffset { get; set; }
        internal uint DataOffset { get; set; }
    }

    public override void Unpack(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);

        if (!br.ReadBytes(8).SequenceEqual(Magic))
        {
            throw new InvalidArchiveException();
        }
        ushort version = br.ReadUInt16();
        fs.Dispose();
        br.Dispose();
        if (version > 1)
        {
            Logger.ShowVersion("bsa", 2);
            UnpackV2(filePath, folderPath);
        }
        else if (version == 1)
        {
            Logger.ShowVersion("bsa", 1);
            UnpackV1(filePath, folderPath);
        }
        else
        {
            throw new InvalidVersionException(InvalidVersionType.Unknown);
        }
    }

    private static void UnpackV1(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        br.ReadBytes(10);
        ushort fileCount = br.ReadUInt16();
        uint indexOffset = br.ReadUInt32();
        ProgressManager.SetMax(fileCount);

        fs.Seek(indexOffset, SeekOrigin.Begin);
        for (int i = 0; i < fileCount; i++)
        {
            string path = Path.Combine(folderPath, ArcEncoding.Shift_JIS.GetString(br.ReadBytes(32)).TrimEnd('\0'));
            uint dataOffset = br.ReadUInt32();
            uint dataSize = br.ReadUInt32();
            long pos = fs.Position;
            fs.Position = dataOffset;
            File.WriteAllBytes(path, br.ReadBytes((int)dataSize));
            fs.Position = pos;
            ProgressManager.Progress();
        }
    }

    private void UnpackV2(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        br.ReadBytes(10);
        ushort fileCount = br.ReadUInt16();
        uint indexOffset = br.ReadUInt32();
        uint nameOffset = indexOffset + ((uint)fileCount * 12);
        ProgressManager.SetMax(fileCount);

        fs.Seek(indexOffset, SeekOrigin.Begin);
        path.Clear();
        realCount = 0;
        path.Add(folderPath);

        for (int i = 0; i < fileCount; i++)
        {
            BsaEntry entry = new();
            entry.NameOffset = br.ReadUInt32() + nameOffset;
            entry.DataOffset = br.ReadUInt32();
            entry.Size = br.ReadUInt32();

            long pos = fs.Position;
            fs.Position = entry.NameOffset;
            string name = br.ReadCString();
            if (name[0] == '>')
            {
                path.Add(name[1..]);
            }
            else if (name[0] == '<')
            {
                path.RemoveAt(path.Count - 1);
            }
            else
            {
                fs.Position = entry.DataOffset;
                string path = Path.Combine(Path.Combine([.. this.path]), name);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, br.ReadBytes((int)entry.Size));
                realCount++;
            }
            fs.Position = pos;
            ProgressManager.Progress();
        }
        Logger.Debug(realCount.ToString() + " among them are actually files.");
    }

    public override void Pack(string folderPath, string filePath)
    {
        switch (_packOptions.Version)
        {
            case "1":
                PackV1(folderPath, filePath);
                break;
            case "2":
                PackV2(folderPath, filePath);
                break;
        }
    }

    private void PackV1(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        // header
        bw.Write(Magic);
        bw.Write((ushort)1);
        DirectoryInfo d = new(folderPath);
        FileInfo[] files = d.GetFiles();
        int fileCount = files.Length;
        ProgressManager.SetMax(fileCount);
        bw.Write((ushort)fileCount);
        bw.Write(0);
        // data
        using MemoryStream ms = new();
        using BinaryWriter bwIndex = new(ms);
        foreach (FileInfo file in files)
        {
            bwIndex.WritePaddedString(file.Name, 32);
            bwIndex.Write((uint)fw.Position);
            bwIndex.Write((uint)file.Length);
            byte[] data = File.ReadAllBytes(file.FullName);
            bw.Write(data);
            data = null;
            ProgressManager.Progress();
        }
        // entry
        uint indexOffset = (uint)fw.Position;
        fw.Position = 12;
        bw.Write(indexOffset);
        fw.Position = indexOffset;
        ms.WriteTo(fw);
    }

    private void PackV2(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        // init
        using MemoryStream index = new();
        using MemoryStream names = new();
        using BinaryWriter bwIndex = new(index);
        using BinaryWriter bwNames = new(names);
        rootDir = folderPath;
        this.fileCount = 0;
        // header
        bw.Write(Magic);
        bw.Write((ushort)3);
        int fileCount = Utility.GetFileCount(folderPath);
        ProgressManager.SetMax(fileCount);
        bw.Write((ushort)fileCount);
        bw.Write(0);
        // others
        Write(bw, folderPath, bwIndex, bwNames);
        uint indexOffset = (uint)fw.Position;
        fw.Position = 10;
        bw.Write((ushort)this.fileCount);
        bw.Write(indexOffset);
        fw.Position = fw.Length;
        index.WriteTo(fw);
        names.WriteTo(fw);
    }

    private void Write(BinaryWriter bw, string path, BinaryWriter bwIndex, BinaryWriter bwNames)
    {
        // enter folder
        if (path != rootDir)
        {
            bwIndex.Write((uint)bwNames.BaseStream.Position);
            bwIndex.Write((long)0);

            bwNames.Write(ArcEncoding.Shift_JIS.GetBytes(">" + Path.GetFileName(path)));
            bwNames.Write('\0');

            fileCount++;
        }

        foreach (string file in Directory.GetFiles(path))
        {
            bwIndex.Write((uint)bwNames.BaseStream.Position);
            bwIndex.Write((uint)bw.BaseStream.Position);
            bwIndex.Write((uint)new FileInfo(file).Length);

            bwNames.Write(ArcEncoding.Shift_JIS.GetBytes(Path.GetFileName(file)));
            bwNames.Write('\0');

            byte[] data = File.ReadAllBytes(file);
            bw.Write(data);
            data = null;

            fileCount++;
            ProgressManager.Progress();
        }

        foreach (string dir in Directory.GetDirectories(path))
        {
            Write(bw, dir, bwIndex, bwNames);
        }

        // leave folder
        if (path != rootDir)
        {
            bwIndex.Write((uint)bwNames.BaseStream.Position);
            bwIndex.Write((long)0);

            bwNames.Write(Encoding.ASCII.GetBytes("<\0"));

            fileCount++;
        }
    }
}

internal partial class BiShopBSAPackOptions : ArcOptions
{
    [ObservableProperty]
    private IReadOnlyList<string> versions = ["1", "2"];
    [ObservableProperty]
    private string version = "2";
}
