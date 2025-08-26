using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.I18n;
using GalArc.Infrastructure.Logging;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GalArc.Models.Formats.InnocentGrey;

internal class IGA : ArcFormat, IUnpackConfigurable, IPackConfigurable
{
    public override string Name => "IGA";
    public override string Description => "InnocentGrey/Noesis Archive";
    public override bool CanWrite => true;

    protected InnocentGreyIGAUnpackOptions _unpackOptions;
    public ArcOptions UnpackOptions => _unpackOptions ??= new InnocentGreyIGAUnpackOptions();

    protected InnocentGreyIGAPackOptions _packOptions;
    public ArcOptions PackOptions => _packOptions ??= new InnocentGreyIGAPackOptions();

    private const string Magic = "IGA0";

    private class InnoIgaEntry : Entry
    {
        public uint NameOffset { get; set; }
        public uint DataOffset { get; set; }
        public uint NameLen { get; set; }
    }

    public override void Unpack(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        List<InnoIgaEntry> entries = [];
        List<InnoIgaEntry> entriesUpdate = [];
        if (Encoding.ASCII.GetString(br.ReadBytes(4)) != Magic)
        {
            throw new InvalidArchiveException();
        }

        fs.Position = 16;
        uint indexSize = VarInt.UnpackUint(br);

        long endPos = fs.Position + indexSize;
        while (fs.Position < endPos)
        {
            InnoIgaEntry entry = new();
            entry.NameOffset = VarInt.UnpackUint(br);
            entry.DataOffset = VarInt.UnpackUint(br);
            entry.Size = VarInt.UnpackUint(br);
            entries.Add(entry);
        }

        ProgressManager.SetMax(entries.Count);
        uint nameIndexSize = VarInt.UnpackUint(br);
        long endName = fs.Position + nameIndexSize;

        for (int i = 0; i < entries.Count; i++)
        {
            InnoIgaEntry entry = entries[i];
            uint thisNameLen = i + 1 < entries.Count ? entries[i + 1].NameOffset - entries[i].NameOffset : nameIndexSize - entries[i].NameOffset;

            entry.Name = VarInt.UnpackString(br, thisNameLen);
            entry.DataOffset += (uint)endName;
            entriesUpdate.Add(entry);
        }

        foreach (InnoIgaEntry entry in entriesUpdate)
        {
            fs.Position = entry.DataOffset;
            byte[] buffer = new byte[entry.Size];
            br.Read(buffer, 0, (int)entry.Size);
            int key = _unpackOptions.DecryptScripts && Path.GetExtension(entry.Name) == ".s" ? 0xFF : 0;
            if (key != 0)
            {
                Logger.DebugFormat(MsgStrings.Decrypting, entry.Name);
            }
            for (uint j = 0; j < entry.Size; j++)
            {
                buffer[j] ^= (byte)((j + 2) ^ key);
            }
            File.WriteAllBytes(Path.Combine(folderPath, entry.Name), buffer);
            buffer = null;
            ProgressManager.Progress();
        }
    }

    public override void Pack(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        bw.Write(Encoding.ASCII.GetBytes(Magic));
        bw.Write(0);    // don't know accurate value , set to 0
        bw.Write(2);
        bw.Write(2);
        List<InnoIgaEntry> l = [];

        DirectoryInfo dir = new(folderPath);
        FileInfo[] files = dir.GetFiles();
        uint nameOffset = 0;
        uint dataOffset = 0;
        foreach (FileInfo file in files)
        {
            InnoIgaEntry entry = new();
            entry.Name = file.Name;
            entry.NameLen = (uint)entry.Name.Length;
            entry.NameOffset = nameOffset;
            entry.DataOffset = dataOffset;
            entry.Size = (uint)file.Length;
            l.Add(entry);
            nameOffset += entry.NameLen;
            dataOffset += entry.Size;
        }
        int fileCount = files.Length;
        ProgressManager.SetMax(fileCount);

        using (MemoryStream msEntry = new())
        {
            using BinaryWriter bwEntry = new(msEntry);
            using MemoryStream msFileName = new();
            using BinaryWriter bwFileName = new(msFileName);
            foreach (InnoIgaEntry i in l)
            {
                bwEntry.Write(VarInt.PackUint(i.NameOffset));
                bwEntry.Write(VarInt.PackUint(i.DataOffset));
                bwEntry.Write(VarInt.PackUint(i.Size));
                bwFileName.Write(VarInt.PackString(i.Name));
            }
            bw.Write(VarInt.PackUint((uint)msEntry.Length));
            msEntry.WriteTo(fw);

            bw.Write(VarInt.PackUint((uint)msFileName.Length));
            msFileName.WriteTo(fw);
        }

        foreach (InnoIgaEntry entry in l)
        {
            byte[] buffer = File.ReadAllBytes(Path.Combine(folderPath, entry.Name));
            int key = _packOptions.EncryptScripts && Path.GetExtension(entry.Name) == ".s" ? 0xFF : 0;
            if (key != 0)
            {
                Logger.DebugFormat(MsgStrings.Encrypting, entry.Name);
            }
            for (uint j = 0; j < entry.Size; j++)
            {
                buffer[j] ^= (byte)((j + 2) ^ key);
            }
            bw.Write(buffer);
            buffer = null;
            ProgressManager.Progress();
        }
    }

    protected static class VarInt
    {
        public static uint UnpackUint(BinaryReader br)
        {
            uint value = 0;
            while ((value & 1) == 0)
            {
                value = value << 7 | br.ReadByte();
            }
            return value >> 1;
        }

        public static string UnpackString(BinaryReader br, uint length)
        {
            byte[] bytes = new byte[length];
            for (uint i = 0; i < length; ++i)
            {
                bytes[i] = (byte)UnpackUint(br);
            }
            return Encoding.GetEncoding(932).GetString(bytes);
        }

        public static byte[] PackUint(uint a)
        {
            List<byte> result = [];
            uint v = a;

            if (v == 0)
            {
                result.Add(0x01);
                return [.. result];
            }

            v = (v << 1) + 1;
            byte curByte = (byte)(v & 0xFF);
            while ((v & 0xFFFFFFFFFFFFFFFE) != 0)
            {
                result.Add(curByte);
                v >>= 7;
                curByte = (byte)(v & 0xFE);
            }

            result.Reverse();
            return [.. result];
        }

        public static byte[] PackString(string s)
        {
            byte[] bytes = Encoding.GetEncoding(932).GetBytes(s);
            List<byte> rst = [];
            foreach (byte b in bytes)
            {
                rst.AddRange(PackUint(b));
            }
            return [.. rst];
        }
    }
}

internal partial class InnocentGreyIGAUnpackOptions : ArcOptions
{
    [ObservableProperty]
    private bool decryptScripts = true;
}

internal partial class InnocentGreyIGAPackOptions : ArcOptions
{
    [ObservableProperty]
    private bool encryptScripts = true;
}
