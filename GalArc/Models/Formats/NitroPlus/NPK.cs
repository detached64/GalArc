using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.I18n;
using GalArc.Infrastructure.Logging;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Database.Commons;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ZstdSharp;

namespace GalArc.Models.Formats.NitroPlus;

internal class NPK : ArcFormat, IUnpackConfigurable, IPackConfigurable
{
    public override string Name => "NPK";
    public override string Description => "NitroPlus NPK Archive";
    public override bool CanWrite => false;

    private NitroPlusNPKUnpackOptions _unpackOptions;
    public ArcOptions UnpackOptions => _unpackOptions ??= new NitroPlusNPKUnpackOptions();

    private NitroPlusNPKPackOptions _packOptions;
    public ArcOptions PackOptions => _packOptions ??= new NitroPlusNPKPackOptions();

    private static readonly string[] NoCompressionExts = ["png", "ogg", "jpg", "mpg"];

    private sealed class NpkEntry : PackedEntry
    {
        public bool HasSeg { get; set; }
        public ushort NameLen { get; set; }
        public byte[] SHA256 { get; set; }
        public int SegCount { get; set; }
        public List<NpkSegInfo> Segs { get; set; }
    }

    private sealed class NpkSegInfo
    {
        public long Offset { get; set; }
        public uint AlignedSize { get; set; }
        public uint RealSize { get; set; }
        public uint DecompressedSize { get; set; }
    }

    public override void Unpack(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        int version = br.ReadUInt32() switch
        {
            0x334b504e => 3,
            0x324b504e => 2,
            _ => throw new InvalidVersionException(InvalidVersionType.NotSupported, _packOptions.MajorVersion)
        };
        int minor_version = br.ReadInt32();
        Logger.Info(MsgStrings.MajorVersion, version);
        Logger.Info(MsgStrings.MinorVersion, minor_version);
        byte[] IV = br.ReadBytes(16);
        int fileCount = br.ReadInt32();
        uint entryTableSize = br.ReadUInt32();
        List<NpkEntry> entries = new(fileCount);
        using Aes aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = _unpackOptions.SelectedKey;
        aes.IV = IV;
        using (SubStream encryptedTableStream = new(fs, fs.Position, entryTableSize))
        {
            using CryptoStream cryptoStream = new(encryptedTableStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            //using FileStream table_bin = File.Create(Path.ChangeExtension(filePath, ".table.bin"));
            //cryptoStream.CopyTo(table_bin);
            using MemoryStream tableStream = new();
            cryptoStream.CopyTo(tableStream);
            tableStream.Position = 0;
            using BinaryReader tableReader = new(tableStream);
            for (int i = 0; i < fileCount; i++)
            {
                NpkEntry entry = new();
                entry.HasSeg = tableReader.ReadByte() == 0;
                entry.NameLen = tableReader.ReadUInt16();
                entry.Name = _unpackOptions.Encoding.GetString(tableReader.ReadBytes(entry.NameLen));
                entry.UnpackedSize = tableReader.ReadUInt32();
                entry.SHA256 = tableReader.ReadBytes(32);
                entry.SegCount = tableReader.ReadInt32();
                entry.Segs = new(entry.SegCount);
                for (int j = 0; j < entry.SegCount; j++)
                {
                    NpkSegInfo segInfo = new();
                    segInfo.Offset = tableReader.ReadInt64();
                    segInfo.AlignedSize = tableReader.ReadUInt32();
                    segInfo.RealSize = tableReader.ReadUInt32();
                    segInfo.DecompressedSize = tableReader.ReadUInt32();
                    entry.Segs.Add(segInfo);
                }
                entries.Add(entry);
            }
        }
        ProgressManager.SetMax(fileCount);
        foreach (NpkEntry entry in entries)
        {
            string path = Path.Combine(folderPath, entry.Name);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using FileStream fw = File.Create(path);
            foreach (NpkSegInfo seg in entry.Segs)
            {
                using SubStream segStream = new(fs, seg.Offset, seg.AlignedSize);
                using CryptoStream segCryptoStream = new(segStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using MemoryStream memoryStream = new();
                segCryptoStream.CopyTo(memoryStream);
                memoryStream.Position = 0;
                if (seg.DecompressedSize > seg.RealSize)
                {
                    switch (version)
                    {
                        case 3:
                            using (DecompressionStream dstream = new(memoryStream))
                                dstream.CopyTo(fw);
                            break;
                        case 2:
                            using (DeflateStream dstream = new(memoryStream, CompressionMode.Decompress))
                                dstream.CopyTo(fw);
                            break;
                    }
                }
                else
                {
                    memoryStream.CopyTo(fw);
                }
            }
            using SHA256 sha256 = SHA256.Create();
            fw.Position = 0;
            byte[] hash = sha256.ComputeHash(fw);
            if (!hash.SequenceEqual(entry.SHA256))
            {
                Logger.Info($"SHA256 mismatch for file {entry.Name}");
            }
            ProgressManager.Progress();
        }
    }

    public override void Pack(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        switch (_packOptions.MajorVersion)
        {
            case 3:
                bw.Write(0x334b504e);
                break;
            case 2:
                bw.Write(0x324b504e);
                break;
            default:
                throw new InvalidVersionException(InvalidVersionType.NotSupported, _packOptions.MajorVersion);
        }
        bw.Write(_packOptions.MinorVersion);
        Random random = new();
        byte[] IV = new byte[16];
        random.NextBytes(IV);
        bw.Write(IV);
        string[] files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
        int fileCount = files.Length;
        bw.Write(fileCount);
        ProgressManager.SetMax(fileCount);
        List<NpkEntry> entries = new(fileCount);
        using Aes aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = _packOptions.SelectedKey;
        aes.IV = IV;
        foreach (string file in files)
        {
            NpkEntry entry = new();
            entry.Path = file;
            using FileStream content = File.OpenRead(file);
            entry.UnpackedSize = (uint)content.Length;
            using (SHA256 sha256 = SHA256.Create())
            {
                entry.SHA256 = sha256.ComputeHash(content);
                content.Position = 0;
            }
            bool useSeg = _packOptions.UseSeg || entry.UnpackedSize > uint.MaxValue;
            entry.Name = Path.GetRelativePath(folderPath, file);
            entry.NameLen = (ushort)_packOptions.Encoding.GetByteCount(entry.Name);
            long length = content.Length;
            if (useSeg)
            {
                entry.SegCount = (int)(1 + (entry.UnpackedSize / 0x10000));
                entry.HasSeg = entry.SegCount > 1;
                entry.Segs = new(entry.SegCount);
                for (int i = 0; i < entry.SegCount; i++)
                {
                    entry.Segs.Add(new NpkSegInfo()
                    {
                        DecompressedSize = (uint)Math.Min(0x10000, length),
                    });
                }
            }
            else
            {
                entry.SegCount = 1;
                entry.HasSeg = false;
                entry.Segs = [
                    new NpkSegInfo()
                    {
                        DecompressedSize = (uint)length,
                    }
                ];
            }
            entries.Add(entry);
        }
        long rawTableSize = 0;
        foreach (NpkEntry entry in entries)
        {
            rawTableSize += 1 + 2 + entry.NameLen + 4 + 32 + 4 + (entry.SegCount * (8 + 4 + 4 + 4));
        }
        byte[] fakeTable = new byte[rawTableSize];
        using MemoryStream fakeTableStream = new(fakeTable);
        using CryptoStream cryptoStream = new(fakeTableStream, aes.CreateEncryptor(), CryptoStreamMode.Read);
        using MemoryStream encrypedFakeTableStream = new();
        cryptoStream.CopyTo(encrypedFakeTableStream);
        uint tableSize = (uint)encrypedFakeTableStream.Length;
        bw.Write(tableSize);
        long tablePos = fw.Position;
        bw.Write(new byte[tableSize]);
        foreach (NpkEntry entry in entries)
        {
            using FileStream content = File.OpenRead(entry.Path);
            long readPos = 0;
            foreach (NpkSegInfo seg in entry.Segs)
            {
                using SubStream rawStream = new(content, readPos, seg.DecompressedSize);
                readPos += rawStream.Length;
                Stream compressedStream = new MemoryStream();
                if (!NoCompressionExts.Contains(Path.GetExtension(entry.Path).TrimStart('.').ToLower()))
                {
                    switch (_packOptions.MajorVersion)
                    {
                        case 3:
                            using (CompressionStream cStream = new(compressedStream))
                            {
                                rawStream.CopyTo(cStream);
                            }
                            break;
                        case 2:
                            using (DeflateStream dStream = new(compressedStream, CompressionLevel.Optimal, true))
                            {
                                rawStream.CopyTo(dStream);
                            }
                            break;
                        default:
                            throw new InvalidVersionException(InvalidVersionType.NotSupported, _packOptions.MajorVersion);
                    }
                    if (compressedStream.Length > rawStream.Length)
                    {
                        compressedStream = rawStream;
                    }
                }
                else
                {
                    compressedStream = rawStream;
                }
                compressedStream.Position = 0;
                using CryptoStream encryptedStream = new(compressedStream, aes.CreateEncryptor(), CryptoStreamMode.Read);
                using MemoryStream alignedStream = new();
                encryptedStream.CopyTo(alignedStream);
                seg.RealSize = (uint)compressedStream.Length;
                seg.AlignedSize = (uint)alignedStream.Length;
                Logger.Info($"Real size: {seg.RealSize}, Aligned size: {seg.AlignedSize}, Decompressed size: {seg.DecompressedSize}");
                seg.Offset = fw.Position;
                alignedStream.Position = 0;
                alignedStream.CopyTo(fw);
            }
            ProgressManager.Progress();
        }
        fw.Position = tablePos;
        using MemoryStream tableStream = new((int)rawTableSize);
        using BinaryWriter tableWriter = new(tableStream);
        foreach (NpkEntry entry in entries)
        {
            tableWriter.Write(entry.HasSeg ? (byte)0 : (byte)1);
            tableWriter.Write(entry.NameLen);
            tableWriter.Write(_packOptions.Encoding.GetBytes(entry.Name));
            tableWriter.Write(entry.UnpackedSize);
            tableWriter.Write(entry.SHA256);
            tableWriter.Write(entry.SegCount);
            foreach (NpkSegInfo seg in entry.Segs)
            {
                tableWriter.Write(seg.Offset);
                tableWriter.Write(seg.AlignedSize);
                tableWriter.Write(seg.RealSize);
                tableWriter.Write(seg.DecompressedSize);
            }
        }
        using CryptoStream finalCryptoStream = new(fw, aes.CreateEncryptor(), CryptoStreamMode.Write);
        tableStream.Position = 0;
        tableStream.CopyTo(finalCryptoStream);
    }
}

internal partial class NitroPlusNPKUnpackOptions : ArcOptions
{
    public readonly NpkScheme Scheme;
    public NitroPlusNPKUnpackOptions()
    {
        if (Design.IsDesignMode)
            return;
        Scheme = DatabaseManager.LoadScheme(DatabaseSerializationContext.Default.NpkScheme);
        if (Scheme?.KnownSchemes != null)
        {
            KnownSchemes = Scheme.KnownSchemes;
            SelectedKey = KnownSchemes.Values.FirstOrDefault();
        }
    }

    [ObservableProperty]
    private IReadOnlyDictionary<string, byte[]> knownSchemes;
    [ObservableProperty]
    private byte[] selectedKey;
    [ObservableProperty]
    private IReadOnlyList<Encoding> encodings = ArcEncoding.SupportedEncodings;
    [ObservableProperty]
    private Encoding encoding = Encoding.UTF8;
}

internal partial class NitroPlusNPKPackOptions : ArcOptions
{
    public readonly NpkScheme Scheme;
    public NitroPlusNPKPackOptions()
    {
        if (Design.IsDesignMode)
            return;
        Scheme = DatabaseManager.LoadScheme(DatabaseSerializationContext.Default.NpkScheme);
        if (Scheme?.KnownSchemes != null)
        {
            KnownSchemes = Scheme.KnownSchemes;
            SelectedKey = KnownSchemes.Values.FirstOrDefault();
        }
    }

    [ObservableProperty]
    private IReadOnlyDictionary<string, byte[]> knownSchemes;
    [ObservableProperty]
    private byte[] selectedKey;
    [ObservableProperty]
    private IReadOnlyList<Encoding> encodings = ArcEncoding.SupportedEncodings;
    [ObservableProperty]
    private Encoding encoding = Encoding.UTF8;
    [ObservableProperty]
    private IReadOnlyList<int> majorVersions = [2, 3];
    [ObservableProperty]
    private int majorVersion = 3;
    [ObservableProperty]
    private IReadOnlyList<int> minorVersions = [1, 2];
    [ObservableProperty]
    private int minorVersion = 2;
    [ObservableProperty]
    private bool useSeg = true;
}
