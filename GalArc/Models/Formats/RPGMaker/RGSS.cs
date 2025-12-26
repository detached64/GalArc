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

namespace GalArc.Models.Formats.RPGMaker;

internal class RGSSAD : ArcFormat, IPackConfigurable
{
    public override string Name => "RGSSAD";
    public override string Description => "RPG Maker RGSS Archive";
    public override bool CanWrite => true;

    private RPGMakerRGSSPackOptions _packOptions;
    public ArcOptions PackOptions => _packOptions ??= new RPGMakerRGSSPackOptions();

    public override void Unpack(string filePath, string folderPath)
    {
        int version;
        using (FileStream fs = File.OpenRead(filePath))
        {
            using BinaryReader br = new(fs);
            if (br.ReadUInt32() != 0x53534752) // "RGSS"
            {
                throw new InvalidArchiveException();
            }
            fs.Position = 7;
            version = br.ReadByte();
        }

        switch (version)
        {
            case 1:
                Logger.ShowVersion(Path.GetExtension(filePath).Trim('.'), version);
                UnpackV1(filePath, folderPath);
                break;
            case 3:
                Logger.ShowVersion(Path.GetExtension(filePath).Trim('.'), version);
                UnpackV3(filePath, folderPath);
                break;
            default:
                throw new InvalidVersionException(InvalidVersionType.Unknown);
        }
    }

    public override void Pack(string folderPath, string filePath)
    {
        switch (_packOptions.Version)
        {
            case 1:
                PackV1(folderPath, filePath);
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
        KeyGen keygen = new(0xDEADCAFE);
        fs.Position = 8;
        int fileCount = 0;
        while (fs.Position < fs.Length)
        {
            uint nameLen = br.ReadUInt32() ^ keygen.Compute();
            byte[] nameBytes = br.ReadBytes((int)nameLen);
            string name = Encoding.UTF8.GetString(DecryptName(nameBytes, keygen));
            string fullPath = Path.Combine(folderPath, name);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            uint size = br.ReadUInt32() ^ keygen.Compute();
            byte[] data = br.ReadBytes((int)size);
            DecryptData(data, new KeyGen(keygen.GetCurrent()));
            File.WriteAllBytes(fullPath, data);
            data = null;
            fileCount++;
        }
        ProgressManager.SetMax(fileCount);
        ProgressManager.Finish();
    }

    private static void UnpackV3(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        fs.Position = 8;
        uint seed = br.ReadUInt32();
        Logger.Debug(MsgStrings.Seed, $"{seed:X8}");
        uint key = (seed * 9) + 3;
        int fileCount = 0;
        bool isFirst = true;
        long maxIndex = 13;
        while (fs.Position < maxIndex)
        {
            uint dataOffset = br.ReadUInt32() ^ key;
            if (isFirst)
            {
                maxIndex = dataOffset - 16;
                isFirst = false;
            }
            uint fileSize = br.ReadUInt32() ^ key;
            uint thisKey = br.ReadUInt32() ^ key;
            uint nameLen = br.ReadUInt32() ^ key;
            string fullPath = Path.Combine(folderPath, Encoding.UTF8.GetString(DecryptName(br.ReadBytes((int)nameLen), key)));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            long pos = fs.Position;
            fs.Position = dataOffset;
            byte[] data = br.ReadBytes((int)fileSize);
            DecryptData(data, new KeyGen(thisKey));
            File.WriteAllBytes(fullPath, data);
            data = null;
            fs.Position = pos;
            fileCount++;
        }
        ProgressManager.SetMax(fileCount);
        ProgressManager.Finish();
    }

    private static void PackV1(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        bw.Write(Encoding.ASCII.GetBytes("RGSSAD\0"));
        bw.Write((byte)1);
        DirectoryInfo d = new(folderPath);
        FileInfo[] files = d.GetFiles("*", SearchOption.AllDirectories);

        ProgressManager.SetMax(files.Length);
        KeyGen keygen = new(0xDEADCAFE);
        foreach (FileInfo file in files)
        {
            byte[] relativePath = Encoding.UTF8.GetBytes(file.FullName[(folderPath.Length + 1)..]);
            bw.Write((uint)relativePath.Length ^ keygen.Compute());
            bw.Write(DecryptName(relativePath, keygen));
            byte[] data = File.ReadAllBytes(file.FullName);
            bw.Write((uint)data.Length ^ keygen.Compute());
            DecryptData(data, new KeyGen(keygen.GetCurrent()));
            bw.Write(data);
            data = null;
            ProgressManager.Progress();
        }
    }

    private void PackV3(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        bw.Write(Encoding.ASCII.GetBytes("RGSSAD\0"));
        bw.Write((byte)3);
        uint seed = Convert.ToUInt32(_packOptions.Seed, 16);
        bw.Write(seed);
        uint key = (9 * seed) + 3;
        string[] files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
        uint fileCount = (uint)files.Length;
        uint baseOffset = (16 * fileCount) + 12 + 16;

        foreach (string file in files)
        {
            baseOffset += (uint)Encoding.UTF8.GetByteCount(file[(folderPath.Length + 1)..]);
        }

        ProgressManager.SetMax(files.Length);
        foreach (string file in files)
        {
            byte[] relativePath = Encoding.UTF8.GetBytes(file[(folderPath.Length + 1)..]);
            byte[] data = File.ReadAllBytes(file);
            const uint thisKey = 0;
            bw.Write(baseOffset ^ key);
            bw.Write((uint)data.Length ^ key);
            bw.Write(thisKey ^ key);
            bw.Write((uint)relativePath.Length ^ key);
            bw.Write(DecryptName(relativePath, key));
            long pos = fw.Position;
            fw.Position = baseOffset;
            DecryptData(data, new KeyGen(thisKey));
            bw.Write(data);
            baseOffset += (uint)data.Length;
            data = null;
            fw.Position = pos;
            ProgressManager.Progress();
        }
        bw.Write(key);
        bw.Write(0);
        bw.Write((long)0);
    }

    private static byte[] DecryptName(byte[] data, KeyGen keygen)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] ^= (byte)keygen.Compute();
        }
        return data;
    }

    private static byte[] DecryptName(byte[] data, uint key)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] ^= (byte)(key >> (i << 3));
        }
        return data;
    }

    private static void DecryptData(byte[] data, KeyGen keygen)
    {
        uint key = keygen.Compute();
        for (int i = 0; i < data.Length;)
        {
            data[i] ^= (byte)(key >> (i << 3));
            i++;
            if ((i & 3) == 0)
            {
                key = keygen.Compute();
            }
        }
    }

    private class KeyGen(uint seed)
    {
        public uint Compute()
        {
            uint result = seed;
            seed = (seed * 7) + 3;
            return result;
        }

        public uint GetCurrent()
        {
            return seed;
        }
    }
}

internal class RGSS2A : RGSSAD
{
    public override string Name => "RGSS2A";
}

internal class RGSS3A : RGSSAD
{
    public override string Name => "RGSS3A";
}

internal partial class RPGMakerRGSSPackOptions : ArcOptions
{
    [ObservableProperty]
    private int version = 3;
    [ObservableProperty]
    private IReadOnlyList<int> versions = [1, 3];
    [ObservableProperty]
    private string seed = "0";
}
