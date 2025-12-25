using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.I18n;
using GalArc.Infrastructure.Logging;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GalArc.Models.Formats.SystemNNN;

internal class GPK : ArcFormat, IPackConfigurable
{
    public override string Name => "GPK";
    public override string Description => "SystemNNN GPK Archive";
    public override bool CanWrite => true;

    private SystemNNNGPKPackOptions _packOptions;
    public ArcOptions PackOptions => _packOptions ??= new SystemNNNGPKPackOptions();

    public override void Unpack(string filePath, string folderPath)
    {
        string gpkPath;
        string gtbPath;

        gpkPath = filePath;
        gtbPath = Path.ChangeExtension(filePath, ".gtb");

        if (!File.Exists(gtbPath))
        {
            Logger.Error(MsgStrings.ErrorSpecifiedFileNotFound, Path.GetFileName(gtbPath));
        }

        using FileStream fs1 = File.OpenRead(gtbPath);
        using BinaryReader br1 = new(fs1);
        int filecount = br1.ReadInt32();
        ProgressManager.SetMax(filecount);

        using FileStream fs2 = File.OpenRead(gpkPath);
        using BinaryReader br2 = new(fs2);

        uint thisPos;
        uint maxPos = 0;
        for (int i = 1; i < filecount; i++)
        {
            Entry entry = new();

            entry.Offset = br1.ReadUInt32();

            fs1.Seek((4 * filecount) - 4, SeekOrigin.Current);

            uint size1 = br1.ReadUInt32();
            uint size2 = br1.ReadUInt32();
            entry.Size = size2 - size1;

            fs1.Seek(4 + (8 * filecount) + entry.Offset, SeekOrigin.Begin);

            entry.Path = Path.Combine(folderPath, $"{br1.ReadCString(Encoding.UTF8)}.dwq");
            thisPos = (uint)fs1.Position;
            maxPos = Math.Max(thisPos, maxPos);

            byte[] buffer = br2.ReadBytes((int)entry.Size);

            File.WriteAllBytes(entry.Path, buffer);
            buffer = null;
            fs1.Seek(4 + (4 * i), SeekOrigin.Begin);

            ProgressManager.Progress();
        }

        uint offset = br1.ReadUInt32();
        uint gtbSize = (uint)new FileInfo(gtbPath).Length;
        uint gpkSize = (uint)new FileInfo(gpkPath).Length;
        fs1.Seek(8 * filecount, SeekOrigin.Begin);
        uint sizeWithoutLast = br1.ReadUInt32();
        fs1.Seek(offset + 4 + (8 * filecount), SeekOrigin.Begin);
        Entry last = new();
        last.Offset = (uint)(gtbSize - (offset + 4 + (8 * filecount)) - 1);
        last.Path = Path.Combine(folderPath, br1.ReadCString(Encoding.UTF8) + ".dwq");
        last.Size = gpkSize - sizeWithoutLast;

        thisPos = (uint)fs1.Position;
        maxPos = Math.Max(thisPos, maxPos);
        byte[] buf = br2.ReadBytes((int)last.Size);
        File.WriteAllBytes(last.Path, buf);
        buf = null;
        ProgressManager.Progress();
        if (maxPos == gtbSize)
        {
            Logger.ShowVersion("gpk", 1);
        }
        else
        {
            Logger.ShowVersion("gpk", 2);
        }
    }

    public override void Pack(string folderPath, string filePath)
    {
        DirectoryInfo d = new(folderPath);
        FileInfo[] files = d.GetFiles("*.dwq");
        int fileCount = files.Length;
        ProgressManager.SetMax(fileCount);

        string gpkPath = filePath;
        string gtbPath = filePath.Contains(".gpk") ? gpkPath.Replace(".gpk", ".gtb") : gpkPath + ".gtb";

        using FileStream fs1 = File.Create(gtbPath);
        using FileStream fs2 = File.Create(gpkPath);
        using BinaryWriter writer1 = new(fs1);
        using BinaryWriter writer2 = new(fs2);

        uint sizeToNow = 0;
        uint offsetToNow = 0;
        writer1.Write(fileCount);

        foreach (FileInfo file in files)
        {
            writer1.Write(offsetToNow);
            offsetToNow = offsetToNow + (uint)Path.GetFileNameWithoutExtension(file.FullName).Length + 1;

            byte[] buffer = File.ReadAllBytes(file.FullName);
            writer2.Write(buffer);
            buffer = null;
        }

        foreach (FileInfo file in files)
        {
            writer1.Write(sizeToNow);
            sizeToNow += (uint)file.Length;
        }

        foreach (FileInfo file in files)
        {
            writer1.Write(Encoding.ASCII.GetBytes(Path.GetFileNameWithoutExtension(file.FullName)));
            writer1.Write('\0');
            ProgressManager.Progress();
        }

        if (_packOptions.Version != 1)
        {
            while (fs1.Position % 16 != 0)
            {
                writer1.Write('\0');
            }
            long sizeToNowNew = 0;
            foreach (FileInfo file in files)
            {
                writer1.Write(sizeToNowNew);
                sizeToNowNew += file.Length;
            }
            writer1.Write((ulong)0);
            writer1.Write(Encoding.ASCII.GetBytes("over2G!\0"));
        }
    }
}

internal partial class SystemNNNGPKPackOptions : ArcOptions
{
    [ObservableProperty]
    private int version = 2;
    [ObservableProperty]
    private IReadOnlyList<int> versions = [1, 2];
}
