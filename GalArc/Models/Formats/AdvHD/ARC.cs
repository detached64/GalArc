using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.I18n;
using GalArc.Infrastructure.Logging;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GalArc.Models.Formats.AdvHD;

internal class ARC : ArcFormat, IUnpackConfigurable, IPackConfigurable
{
    public override string Name => "ARC";
    public override string Description => "AdvHD Archive";
    public override bool CanWrite => true;

    private AdvHDARCUnpackOptions _unpackOptions;
    public ArcOptions UnpackOptions => _unpackOptions ??= new AdvHDARCUnpackOptions();

    private AdvHDARCPackOptions _packOptions;
    public ArcOptions PackOptions => _packOptions ??= new AdvHDARCPackOptions();

    private readonly string[] EncryptedFileExtV1 = ["wsc", "scr"];
    private readonly string[] EncryptedFileExtV2 = ["ws2", "json"];

    private class HeaderV1
    {
        public int TypeCount { get; set; }
        public int FileCountAll { get; set; }
    }

    private class TypeHeaderV1
    {
        public string Extension { get; set; }
        public int FileCount { get; set; }
        public long IndexOffset { get; set; }
    }

    private void UnpackV1(string filePath, string folderPath)
    {
        HeaderV1 header = new();
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        header.FileCountAll = 0;
        header.TypeCount = br.ReadInt32();
        List<TypeHeaderV1> typeHeaders = [];

        for (int i = 0; i < header.TypeCount; i++)
        {
            TypeHeaderV1 typeHeader = new();
            typeHeader.Extension = Encoding.ASCII.GetString(br.ReadBytes(3));
            br.ReadByte();
            typeHeader.FileCount = br.ReadInt32();
            typeHeader.IndexOffset = br.ReadUInt32();
            typeHeaders.Add(typeHeader);
            header.FileCountAll += typeHeader.FileCount;
        }
        ProgressManager.SetMax(header.FileCountAll);

        for (int i = 0; i < header.TypeCount; i++)
        {
            for (int j = 0; j < typeHeaders[i].FileCount; j++)
            {
                Entry index = new();
                index.Name = Encoding.ASCII.GetString(br.ReadBytes(13)).Replace("\0", string.Empty) + "." + typeHeaders[i].Extension;
                index.Size = br.ReadUInt32();
                index.Offset = br.ReadUInt32();
                long pos = fs.Position;
                index.Path = Path.Combine(folderPath, index.Name);
                fs.Seek(index.Offset, SeekOrigin.Begin);
                byte[] buffer = br.ReadBytes((int)index.Size);
                if (_unpackOptions.DecryptScripts && IsScriptFile(Path.GetExtension(index.Path), 1))
                {
                    Logger.DebugFormat(MsgStrings.Decrypting, index.Name);
                    DecryptScript(buffer);
                }
                File.WriteAllBytes(index.Path, buffer);
                buffer = null;
                fs.Seek(pos, SeekOrigin.Begin);
                ProgressManager.Progress();
            }
        }
    }

    private void PackV1(string folderPath, string filePath)
    {
        HeaderV1 header = new();
        header.FileCountAll = Utility.GetFileCount(folderPath);
        ProgressManager.SetMax(header.FileCountAll);
        string[] exts = Utility.GetFileExtensions(folderPath);
        Array.Sort(exts, StringComparer.Ordinal);
        int extCount = exts.Length;

        header.TypeCount = extCount;
        const int length = 13;
        DirectoryInfo d = new(folderPath);
        int[] type_fileCount = new int[extCount];

        using FileStream fw = File.Create(filePath);
        using MemoryStream mshead = new();
        using MemoryStream mstype = new();
        using MemoryStream msentry = new();
        using MemoryStream msdata = new();
        using BinaryWriter bwhead = new(mshead);
        using BinaryWriter bwtype = new(mstype);
        using BinaryWriter bwentry = new(msentry);
        using BinaryWriter bwdata = new(msdata);

        bwhead.Write(header.TypeCount);
        uint pos = (uint)((12 * extCount) + 4);

        for (int i = 0; i < extCount; i++)
        {
            foreach (FileInfo file in d.GetFiles($"*{exts[i]}"))
            {
                type_fileCount[i]++;
                bwentry.Write(Encoding.ASCII.GetBytes(Path.GetFileNameWithoutExtension(file.Name).PadRight(length, '\0')));
                bwentry.Write((uint)file.Length);
                bwentry.Write((uint)(4 + (12 * header.TypeCount) + (21 * header.FileCountAll) + msdata.Length));
                byte[] buffer = File.ReadAllBytes(file.FullName);
                if (_packOptions.EncryptScripts && IsScriptFile(exts[i], 1))
                {
                    Logger.DebugFormat(MsgStrings.Encrypting, file.Name);
                    EncryptScript(buffer);
                }
                bwdata.Write(buffer);
                buffer = null;
                ProgressManager.Progress();
            }
            bwtype.Write(Encoding.ASCII.GetBytes(exts[i]));
            bwtype.Write((byte)0);
            bwtype.Write((uint)type_fileCount[i]);
            bwtype.Write(pos);
            pos = (uint)((12 * extCount) + 4 + msentry.Length);
        }
        mshead.WriteTo(fw);
        mstype.WriteTo(fw);
        msentry.WriteTo(fw);
        msdata.WriteTo(fw);
    }

    private class HeaderV2
    {
        public int FileCount { get; set; }
        public uint EntrySize { get; set; }
    }

    private void UnpackV2(string filePath, string folderPath)
    {
        HeaderV2 header = new();
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br1 = new(fs);
        List<Entry> l = [];

        header.FileCount = br1.ReadInt32();
        header.EntrySize = br1.ReadUInt32();
        ProgressManager.SetMax(header.FileCount);

        for (int i = 0; i < header.FileCount; i++)
        {
            Entry entry = new();
            entry.Size = br1.ReadUInt32();
            entry.Offset = br1.ReadUInt32() + 8 + header.EntrySize;
            entry.Name = br1.ReadCString(Encoding.Unicode);
            entry.Path = Path.Combine(folderPath, entry.Name);

            l.Add(entry);
        }

        foreach (Entry entry in l)
        {
            byte[] buffer = br1.ReadBytes((int)entry.Size);
            if (_unpackOptions.DecryptScripts && IsScriptFile(Path.GetExtension(entry.Path), 2))
            {
                Logger.DebugFormat(MsgStrings.Decrypting, entry.Name);
                DecryptScript(buffer);
            }
            File.WriteAllBytes(entry.Path, buffer);
            buffer = null;
            ProgressManager.Progress();
        }
    }

    private void PackV2(string folderPath, string filePath)
    {
        HeaderV2 header = new();
        List<Entry> l = [];
        uint sizeToNow = 0;

        DirectoryInfo d = new(folderPath);
        FileInfo[] files = d.GetFiles();
        header.FileCount = files.Length;
        header.EntrySize = 0;
        ProgressManager.SetMax(header.FileCount);

        foreach (FileInfo file in files)
        {
            Entry entry = new();
            entry.Name = file.Name;
            entry.Size = (uint)file.Length;
            entry.Offset = sizeToNow;
            sizeToNow += entry.Size;
            l.Add(entry);

            int nameLength = entry.Name.Length;
            header.EntrySize = header.EntrySize + ((uint)nameLength * 2) + 2 + 8;
        }

        using FileStream fs = File.Create(filePath);
        using BinaryWriter bw = new(fs);
        bw.Write(header.FileCount);
        bw.Write(header.EntrySize);

        foreach (Entry file in l)
        {
            bw.Write(file.Size);
            bw.Write(file.Offset);
            bw.Write(Encoding.Unicode.GetBytes(file.Name));
            bw.Write('\0');
            bw.Write('\0');
        }

        foreach (FileInfo file in files)
        {
            byte[] buffer = File.ReadAllBytes(file.FullName);
            if (_packOptions.EncryptScripts && IsScriptFile(file.Extension, 2))
            {
                Logger.DebugFormat(MsgStrings.Encrypting, file.Name);
                EncryptScript(buffer);
            }
            bw.Write(buffer);
            buffer = null;
            ProgressManager.Progress();
        }
    }

    public override void Unpack(string filePath, string folderPath)
    {
        List<Action> actions =
        [
            () => UnpackV2(filePath, folderPath),
            () => UnpackV1(filePath, folderPath)
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
        }
    }

    private bool IsScriptFile(string extension, int version)
    {
        string trimed = extension.TrimStart('.');
        return version switch
        {
            1 => EncryptedFileExtV1.Contains(trimed),
            2 => EncryptedFileExtV2.Contains(trimed),
            _ => false,
        };
    }

    private static void DecryptScript(byte[] data)
    {
        for (int i = 0; i < data.Length; ++i)
        {
            data[i] = Binary.RotByteR(data[i], 2);
        }
    }

    private static void EncryptScript(byte[] data)
    {
        for (int i = 0; i < data.Length; ++i)
        {
            data[i] = Binary.RotByteL(data[i], 2);
        }
    }
}

internal partial class AdvHDARCUnpackOptions : ArcOptions
{
    [ObservableProperty]
    private bool decryptScripts = true;
}

internal partial class AdvHDARCPackOptions : ArcOptions
{
    [ObservableProperty]
    private bool encryptScripts = true;
    [ObservableProperty]
    private int version = 2;
    [ObservableProperty]
    private IReadOnlyList<int> versions = [1, 2];
}
