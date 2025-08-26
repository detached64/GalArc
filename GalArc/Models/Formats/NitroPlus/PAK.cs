using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System;
using System.IO;

namespace GalArc.Models.Formats.NitroPlus;

internal class PAK : ArcFormat, IPackConfigurable
{
    public override string Name => "PAK";
    public override string Description => "Nitro+ PAK Archive";
    public override bool CanWrite => true;

    private NitroPlusPAKPackOptions _packOptions;
    public ArcOptions PackOptions => _packOptions ??= new NitroPlusPAKPackOptions();

    private class NitroPakEntry : PackedEntry
    {
        public uint PathLen { get; set; }
        public string RelativePath { get; set; }
    }

    public override void Unpack(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        fs.Position = 4;
        int fileCount = br.ReadInt32();
        ProgressManager.SetMax(fileCount);
        fs.Position += 4;
        int comSize = br.ReadInt32();
        fs.Position = 0x114;

        using MemoryStream ms = new(ZlibHelper.Decompress(br.ReadBytes(comSize)));
        using BinaryReader brEntry = new(ms);
        int dataOffset = 276 + comSize;
        fs.Position = dataOffset;

        while (ms.Position != ms.Length)
        {
            NitroPakEntry entry = new();
            entry.PathLen = brEntry.ReadUInt32();
            entry.RelativePath = ArcEncoding.Shift_JIS.GetString(brEntry.ReadBytes((int)entry.PathLen));
            entry.Path = Path.Combine(folderPath, entry.RelativePath);

            entry.Offset = brEntry.ReadUInt32() + (uint)dataOffset;
            entry.UnpackedSize = brEntry.ReadUInt32();
            entry.Size = brEntry.ReadUInt32();
            entry.IsPacked = brEntry.ReadUInt32() != 0;
            uint size = brEntry.ReadUInt32();
            if (entry.IsPacked)
            {
                entry.Size = size;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(entry.Path));

            byte[] data = br.ReadBytes((int)entry.Size);
            byte[] backup = new byte[data.Length];
            Buffer.BlockCopy(data, 0, backup, 0, data.Length);
            try
            {
                byte[] result = ZlibHelper.Decompress(data);
                File.WriteAllBytes(entry.Path, result);
                result = null;
            }
            catch
            {
                File.WriteAllBytes(entry.Path, backup);
            }
            backup = null;
            data = null;
            ProgressManager.Progress();
        }
    }

    public override void Pack(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        string[] fullPaths = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
        string[] relativePaths = Utility.GetRelativePaths(fullPaths, folderPath);

        bw.Write(2);
        int fileCount = fullPaths.Length;
        bw.Write(fileCount);
        ProgressManager.SetMax(fileCount);

        using (MemoryStream memoryStream = new())
        {
            using BinaryWriter bwIndex = new(memoryStream);
            uint offset = 0;
            foreach (string relativePath in relativePaths)
            {
                bwIndex.Write(ArcEncoding.Shift_JIS.GetByteCount(relativePath));
                bwIndex.Write(ArcEncoding.Shift_JIS.GetBytes(relativePath));
                bwIndex.Write(offset);
                uint fileSize = (uint)new FileInfo(Path.Combine(folderPath, relativePath)).Length;
                bwIndex.Write(fileSize);
                bwIndex.Write(fileSize);
                bwIndex.Write((long)0);
                offset += fileSize;
            }

            byte[] uncomIndex = memoryStream.ToArray();

            bw.Write(uncomIndex.Length);
            byte[] comIndex = ZlibHelper.Compress(uncomIndex);
            bw.Write(comIndex.Length);

            if (!string.IsNullOrEmpty(_packOptions.OriginalFilePath) && File.Exists(_packOptions.OriginalFilePath))
            {
                using FileStream fs = File.OpenRead(_packOptions.OriginalFilePath);
                using BinaryReader br = new(fs);
                fs.Position = 16;
                bw.Write(br.ReadBytes(260));
            }
            else
            {
                bw.Write(new byte[260]);
            }
            bw.Write(comIndex);
        }

        foreach (string fullPath in fullPaths)
        {
            byte[] data = File.ReadAllBytes(fullPath);
            bw.Write(data);
            data = null;
            ProgressManager.Progress();
        }
    }
}

internal partial class NitroPlusPAKPackOptions : ArcOptions
{
    [ObservableProperty]
    private string originalFilePath = string.Empty;
}
