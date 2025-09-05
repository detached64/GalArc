using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Database.Commons;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ZstdSharp;

namespace GalArc.Models.Formats.NitroPlus;

internal class NPK : ArcFormat, IUnpackConfigurable
{
    public override string Name => "NPK";
    public override string Description => "NitroPlus NPK Archive";

    private NitroPlusNPKUnpackOptions _unpackOptions;
    public ArcOptions UnpackOptions => _unpackOptions ??= new NitroPlusNPKUnpackOptions();

    private sealed class NpkEntry : PackedEntry
    {
        public bool HasSeg { get; set; }
        public byte[] SHA256 { get; set; }
        public int SegCount { get; set; }
        public List<NpkSegInfo> NpkSegs { get; set; }
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
            _ => throw new InvalidArchiveException(),
        };
        int minor_version = br.ReadInt32();
        byte[] IV = br.ReadBytes(16);
        int fileCount = br.ReadInt32();
        uint entryTableSize = br.ReadUInt32();
        using SubStream entryTableStream = new(fs, fs.Position, entryTableSize);
        Aes aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = _unpackOptions.SelectedKey;
        aes.IV = IV;
        using CryptoStream cryptoStream = new(entryTableStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
        List<NpkEntry> entries = new(fileCount);
        using (MemoryStream tableStream = new())
        {
            cryptoStream.CopyTo(tableStream);
            tableStream.Position = 0;
            using BinaryReader tableReader = new(tableStream);
            for (int i = 0; i < fileCount; i++)
            {
                NpkEntry entry = new();
                entry.HasSeg = tableReader.ReadByte() == 0;
                int nameLen = tableReader.ReadUInt16();
                entry.Name = _unpackOptions.Encoding.GetString(tableReader.ReadBytes(nameLen));
                entry.UnpackedSize = tableReader.ReadUInt32();
                entry.SHA256 = tableReader.ReadBytes(32);
                entry.SegCount = tableReader.ReadInt32();
                entry.NpkSegs = new(entry.SegCount);
                for (int j = 0; j < entry.SegCount; j++)
                {
                    NpkSegInfo segInfo = new();
                    segInfo.Offset = tableReader.ReadInt64();
                    segInfo.AlignedSize = tableReader.ReadUInt32();
                    segInfo.RealSize = tableReader.ReadUInt32();
                    segInfo.DecompressedSize = tableReader.ReadUInt32();
                    entry.NpkSegs.Add(segInfo);
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
            foreach (NpkSegInfo seg in entry.NpkSegs)
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
            ProgressManager.Progress();
        }
    }
}

internal partial class NitroPlusNPKUnpackOptions : ArcOptions
{
    public readonly NpkScheme Scheme;
    public NitroPlusNPKUnpackOptions()
    {
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
