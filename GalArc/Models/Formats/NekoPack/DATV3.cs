using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.I18n;
using GalArc.Infrastructure.Logging;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Database.Commons;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace GalArc.Models.Formats.NekoPack;

internal class DATV3 : ArcFormat, IPackConfigurable
{
    public override string Name => "DAT";
    public override string Description => "NEKOPACK Archive v3";
    public override bool CanWrite => true;

    private NekoPackDATV3PackOptions _packOptions;
    public ArcOptions PackOptions => _packOptions ??= new NekoPackDATV3PackOptions();

    private const string Magic = "NEKOPACK";

    static DATV3()
    {
        ReadNameList();
    }

    private static readonly List<string> KnownDirNames =
    [
        "image/actor", "image/back", "image/mask", "image/visual", "image/actor/big",
        "image/face", "image/actor/b", "image/actor/bb", "image/actor/s", "image/actor/ss",
        "sound/bgm", "sound/env", "sound/se", "sound/bgv", "voice", "script", "system", "count",
    ];

    private static readonly List<string> KnownFileNames = [];

    private class NekoPackDir
    {
        public uint NameHash { get; set; }
        public int FileCount { get; set; }
        public List<NekoPackEntry> Entries { get; set; } = [];
    }

    private class NekoPackEntry : Entry
    {
        public uint NameHash { get; set; }
        public uint Parity { get; set; }
    }

    public override void Unpack(string input, string output)
    {
        using FileStream fs = File.OpenRead(input);
        using BinaryReader br = new(fs);
        if (!string.Equals(Encoding.ASCII.GetString(br.ReadBytes(8)), Magic))
        {
            throw new InvalidArchiveException();
        }
        uint seed = br.ReadUInt32();
        uint order = br.ReadUInt32();
        uint parity = br.ReadUInt32();
        uint indexSize = br.ReadUInt32();
        if (ComputeParity(seed, indexSize) != parity)
        {
            Logger.Info("Index parity check failed. The archive may be corrupted.");
        }
        Logger.Info(MsgStrings.Seed, $"{seed:X8}");
        ushort[] key = GetKey(parity);
        byte[] index = br.ReadBytes((int)indexSize);
        Decrypt(index, key);
        List<NekoPackDir> dirs = [];
        int fileCount = 0;
        using (MemoryStream ms = new(index))
        using (BinaryReader indexReader = new(ms))
        {
            while (ms.Position < ms.Length)
            {
                NekoPackDir dir = new()
                {
                    NameHash = indexReader.ReadUInt32(),
                    FileCount = indexReader.ReadInt32()
                };
                for (int i = 0; i < dir.FileCount; i++)
                {
                    dir.Entries.Add(new NekoPackEntry()
                    {
                        NameHash = indexReader.ReadUInt32(),
                        Size = indexReader.ReadUInt32()
                    });
                }
                dirs.Add(dir);
                fileCount += dir.FileCount;
            }
        }
        ProgressManager.SetMax(fileCount);
        Dictionary<uint, string> dirNames = GetNamesMap(KnownDirNames, seed);
        Dictionary<uint, string> fileNames = GetNamesMap(KnownFileNames, seed);
        foreach (NekoPackDir dir in dirs)
        {
            if (!dirNames.TryGetValue(dir.NameHash, out string dirName))
            {
                dirName = dir.NameHash.ToString("X8");
            }
            Directory.CreateDirectory(Path.Combine(output, dirName));
            foreach (NekoPackEntry entry in dir.Entries)
            {
                entry.Parity = br.ReadUInt32();
                uint size = br.ReadUInt32();
                if (entry.Size != size)
                {
                    throw new InvalidArchiveException($"File size mismatch for entry {entry.NameHash:X8} in directory {dirName}.");
                }
                byte[] data = br.ReadBytes((int)entry.Size);
                if (entry.Parity != 0)
                {
                    if (ComputeParity(seed, entry.Size) != entry.Parity)
                    {
                        Logger.Info($"File parity check failed for {dirName}/{entry.NameHash:X8}. The file may be corrupted.");
                    }
                    ushort[] fileKey = GetKey(entry.Parity);
                    if (data.Length % 2 == 0)
                    {
                        Decrypt(data, fileKey);
                    }
                    else
                    {
                        byte[] oddData = new byte[data.Length + 1];
                        Buffer.BlockCopy(data, 0, oddData, 0, data.Length);
                        oddData[^1] = 0;
                        Decrypt(oddData, fileKey);
                        Buffer.BlockCopy(oddData, 0, data, 0, data.Length);
                    }
                }
                if (fileNames.TryGetValue(entry.NameHash, out string fileName))
                {
                    Logger.Debug(MsgStrings.FileNameRecovered, fileName);
                }
                else
                {
                    fileName = entry.NameHash.ToString("X8");
                }
                string filePath = Path.Combine(output, dirName, fileName);
                File.WriteAllBytes(filePath, data);
                ProgressManager.Progress();
            }
        }
    }

    public override void Pack(string input, string output)
    {
        uint seed = uint.TryParse(_packOptions.SeedString, NumberStyles.HexNumber, null, out uint s) ? s : 0;
        using FileStream fw = File.Create(output);
        using BinaryWriter bw = new(fw);
        bw.Write(Encoding.ASCII.GetBytes(Magic));
        bw.Write(seed);
        bw.Write(0xFFFFFFFF);
        FileInfo[] files = new DirectoryInfo(input).GetFiles("*", SearchOption.AllDirectories);
        int fileCount = files.Length;
        IGrouping<string, FileInfo>[] groups = [.. files.GroupBy(f => Path.GetRelativePath(input, f.DirectoryName).Replace('\\', '/'))];
        int dirCount = groups.Length;
        uint indexSize = (uint)(8 * (fileCount + dirCount));
        uint dataOffset = 24 + indexSize;
        uint parity = ComputeParity(seed, indexSize);
        bw.Write(parity);
        bw.Write(indexSize);
        fw.Position = dataOffset;
        ProgressManager.SetMax(fileCount);
        List<NekoPackDir> dirs = new(groups.Length);
        foreach (IGrouping<string, FileInfo> group in groups)
        {
            NekoPackDir dir = new()
            {
                NameHash = TryGetNameHash(group.Key, seed),
                FileCount = group.Count(),
                Entries = new List<NekoPackEntry>(group.Count())
            };
            foreach (FileInfo file in group)
            {
                dir.Entries.Add(new NekoPackEntry()
                {
                    Path = file.FullName,
                    NameHash = TryGetNameHash(file.Name, seed),
                    Size = (uint)file.Length,
                    Parity = 0
                });
            }
            dirs.Add(dir);
        }
        foreach (NekoPackDir dir in dirs)
        {
            dir.Entries.Sort((a, b) => a.NameHash.CompareTo(b.NameHash));
        }
        dirs.Sort((a, b) => a.NameHash.CompareTo(b.NameHash));
        using MemoryStream indexStream = new();
        using BinaryWriter indexWriter = new(indexStream);
        foreach (NekoPackDir dir in dirs)
        {
            indexWriter.Write(dir.NameHash);
            indexWriter.Write(dir.FileCount);
            foreach (NekoPackEntry entry in dir.Entries)
            {
                indexWriter.Write(entry.NameHash);
                indexWriter.Write(entry.Size);
                bw.Write(entry.Parity);
                bw.Write(entry.Size);
                using FileStream file = File.OpenRead(entry.Path);
                file.CopyTo(fw);
                ProgressManager.Progress();
            }
        }
        fw.Position = 24;
        ushort[] key = GetKey(parity);
        byte[] indexData = indexStream.ToArray();
        Encrypt(indexData, key);
        bw.Write(indexData);
    }

    private static uint ComputeParity(uint a1, uint a2)
    {
        uint v1 = (a2 ^ ((a2 ^ ((a2 ^ ((a2 ^ a1) + 0x5D588B65)) - 0x359D3E2A)) - 0x70E44324)) + 0x6C078965;
        uint v2 = ((a2 ^ ((a2 ^ a1) + 0x5D588B65)) - 0x359D3E2A) >> 27;
        return Binary.RotL(v1, (int)v2);
    }

    private static ushort[] GetKey(uint parity)
    {
        uint tmp = parity ^ (parity + 0x5D588B65u);
        uint tmp2 = tmp ^ (parity - 0x359D3E2Au);
        uint key0 = tmp2 ^ (tmp - 0x70E44324u);
        uint key1 = key0 ^ (tmp2 + 0x6C078965u);
        return [(ushort)(key0 & 0xFFFF), (ushort)(key0 >> 16), (ushort)(key1 & 0xFFFF), (ushort)(key1 >> 16)];
    }

    private static unsafe void Decrypt(byte[] data, ushort[] key)
    {
        fixed (byte* b = data)
        {
            ushort* w = (ushort*)b;
            for (int i = 0; i < data.Length / 2; i++)
            {
                w[i] ^= key[i % 4];
                key[i % 4] += w[i];
            }
        }
    }

    private static unsafe void Encrypt(byte[] data, ushort[] key)
    {
        fixed (byte* b = data)
        {
            ushort* w = (ushort*)b;
            for (int i = 0; i < data.Length / 2; i++)
            {
                ushort tmp = w[i];
                w[i] ^= key[i % 4];
                key[i % 4] += tmp;
            }
        }
    }

    private static uint GetNameHash(byte[] data, int length, uint seed)
    {
        uint hash = seed;
        for (int i = 0; i < length; ++i)
        {
            byte c = data[i];
            hash = 81 * (ShiftMap[c] ^ hash);
        }
        return hash;
    }

    private static uint GetNameHash(string name, uint seed)
    {
        byte[] buffer = new byte[0x100];
        int length = ArcEncoding.Shift_JIS.GetBytes(name, 0, name.Length, buffer, 0);
        return GetNameHash(buffer, length, seed);
    }

    private static uint TryGetNameHash(string name, uint seed)
    {
        return uint.TryParse(name, NumberStyles.HexNumber, null, out uint hash) ? hash : GetNameHash(name, seed);
    }

    private static Dictionary<uint, string> GetNamesMap(List<string> names, uint seed)
    {
        Dictionary<uint, string> map = new(names.Count);
        foreach (string name in names)
        {
            uint hash = GetNameHash(name, seed);
            if (!map.TryAdd(hash, name) && !map[hash].Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                Logger.Info($"Hash collision detected between \"{map[hash]}\" and \"{name}\". Hash is {hash:X8}.");
            }
        }
        return map;
    }

    private static void ReadNameList()
    {
        DatabaseManager.LoadList("NekoPack", KnownFileNames.Add);
    }

    private static readonly byte[] ShiftMap =
    [
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x38, 0x2F, 0x33, 0x3C, 0x40, 0x3B, 0x2A, 0x2E, 0x31, 0x30, 0x26, 0x44, 0x35, 0x28, 0x3E, 0x12,
        0x02, 0x22, 0x06, 0x20, 0x1A, 0x1C, 0x0F, 0x11, 0x18, 0x17, 0x42, 0x2B, 0x3A, 0x37, 0x34, 0x0C,
        0x41, 0x08, 0x1D, 0x07, 0x15, 0x21, 0x05, 0x1E, 0x0A, 0x14, 0x0E, 0x10, 0x09, 0x27, 0x1F, 0x0B,
        0x23, 0x16, 0x0D, 0x01, 0x25, 0x04, 0x1B, 0x03, 0x13, 0x24, 0x19, 0x2D, 0x12, 0x29, 0x32, 0x3F,
        0x3D, 0x08, 0x1D, 0x07, 0x15, 0x21, 0x05, 0x1E, 0x0A, 0x14, 0x0E, 0x10, 0x09, 0x27, 0x1F, 0x0B,
        0x23, 0x16, 0x0D, 0x01, 0x25, 0x04, 0x1B, 0x03, 0x13, 0x24, 0x19, 0x2C, 0x39, 0x43, 0x36, 0x00,
        0x4B, 0xA9, 0xA7, 0xAF, 0x50, 0x52, 0x91, 0x9F, 0x47, 0x6B, 0x96, 0xAB, 0x87, 0xB5, 0x9B, 0xBB,
        0x99, 0xA4, 0xBF, 0x5C, 0xC6, 0x9C, 0xC2, 0xC4, 0xB6, 0x4F, 0xB8, 0xC1, 0x85, 0xA8, 0x51, 0x7E,
        0x5F, 0x82, 0x73, 0xC7, 0x90, 0x4E, 0x45, 0xA5, 0x7A, 0x63, 0x70, 0xB3, 0x79, 0x83, 0x60, 0x55,
        0x5B, 0x5E, 0x68, 0xBA, 0x53, 0xA1, 0x67, 0x97, 0xAC, 0x71, 0x81, 0x59, 0x64, 0x7C, 0x9D, 0xBD,
        0x9D, 0xBD, 0x95, 0xA0, 0xB2, 0xC0, 0x6F, 0x6A, 0x54, 0xB9, 0x6D, 0x88, 0x77, 0x48, 0x5D, 0x72,
        0x49, 0x93, 0x57, 0x65, 0xBE, 0x4A, 0x80, 0xA2, 0x5A, 0x98, 0xA6, 0x62, 0x7F, 0x84, 0x75, 0xBC,
        0xAD, 0xB1, 0x6E, 0x76, 0x8B, 0x9E, 0x8C, 0x61, 0x69, 0x8D, 0xB4, 0x78, 0xAA, 0xAE, 0x8F, 0xC3,
        0x58, 0xC5, 0x74, 0xB7, 0x8E, 0x7D, 0x89, 0x8A, 0x56, 0x4D, 0x86, 0x94, 0x9A, 0x4C, 0x92, 0xB0,
    ];
}

internal partial class NekoPackDATV3PackOptions : ArcOptions
{
    [ObservableProperty]
    private string seedString = "00000000";
}
