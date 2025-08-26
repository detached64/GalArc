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

namespace GalArc.Models.Formats.PJADV;

internal class DAT : ArcFormat, IUnpackConfigurable, IPackConfigurable
{
    public override string Name => "DAT";
    public override string Description => "PJADV DAT Archive";
    public override bool CanWrite => true;

    private PJADVDATUnpackOptions _unpackOptions;
    public ArcOptions UnpackOptions => _unpackOptions ??= new PJADVDATUnpackOptions();

    private PJADVDATPackOptions _packOptions;
    public ArcOptions PackOptions => _packOptions ??= new PJADVDATPackOptions();

    private const string Magic = "GAMEDAT PAC";
    private readonly byte[] ScriptMagic = [0x95, 0x6b, 0x3c, 0x9d, 0x63];

    public override void Unpack(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);

        if (Encoding.ASCII.GetString(br.ReadBytes(11)) != Magic)
        {
            throw new InvalidArchiveException();
        }
        int version = br.ReadByte();
        int nameLen;
        switch (version)
        {
            case 'K':
                nameLen = 16;
                Logger.ShowVersion("dat", 1);
                break;
            case '2':
                nameLen = 32;
                Logger.ShowVersion("dat", 2);
                break;
            default:
                throw new InvalidVersionException(InvalidVersionType.Unknown);
        }

        int count = br.ReadInt32();
        uint baseOffset = 0x10 + (uint)(count * (nameLen + 8));
        List<Entry> entries = [];
        ProgressManager.SetMax(count);

        for (int i = 0; i < count; i++)
        {
            Entry entry = new();
            entry.Name = ArcEncoding.Shift_JIS.GetString(br.ReadBytes(nameLen)).TrimEnd('\0');
            entry.Path = Path.Combine(folderPath, entry.Name);
            entries.Add(entry);
        }
        for (int i = 0; i < count; i++)
        {
            entries[i].Offset = br.ReadUInt32() + baseOffset;
            entries[i].Size = br.ReadUInt32();
        }
        foreach (Entry entry in entries)
        {
            fs.Position = entry.Offset;
            byte[] buffer = br.ReadBytes((int)entry.Size);
            if (_unpackOptions.DecryptScripts && entry.Name.Contains("textdata") && buffer.Take(5).ToArray().SequenceEqual(ScriptMagic))
            {
                Logger.DebugFormat(MsgStrings.Decrypting, entry.Name);
                DecryptScript(buffer);
            }
            File.WriteAllBytes(entry.Path, buffer);
            buffer = null;
            ProgressManager.Progress();
        }
    }

    public override void Pack(string folderPath, string filePath)
    {
        using FileStream fs = File.Create(filePath);
        using BinaryWriter bw = new(fs);
        bw.Write(Encoding.ASCII.GetBytes(Magic));
        int nameLength = _packOptions.Version == 1 ? 16 : 32;
        bw.Write(_packOptions.Version == 1 ? (byte)'K' : (byte)'2');
        DirectoryInfo d = new(folderPath);
        FileInfo[] files = d.GetFiles();

        bw.Write(files.Length);
        List<Entry> entries = [];
        ProgressManager.SetMax(files.Length);
        uint thisOffset = 0;

        foreach (FileInfo file in files)
        {
            Entry entry = new();
            entry.Name = file.Name;
            entry.Path = file.FullName;
            entry.Size = (uint)file.Length;
            entry.Offset = thisOffset;
            thisOffset += entry.Size;
            entries.Add(entry);
        }
        foreach (Entry entry in entries)
        {
            bw.WritePaddedString(entry.Name, nameLength);
        }
        foreach (Entry entry in entries)
        {
            bw.Write(entry.Offset);
            bw.Write(entry.Size);
        }
        foreach (Entry entry in entries)
        {
            byte[] buffer = File.ReadAllBytes(entry.Path);
            if (_packOptions.EncryptScripts && entry.Name.Contains("textdata") && buffer.Take(5).ToArray().SequenceEqual(ScriptMagic))
            {
                Logger.DebugFormat(MsgStrings.Encrypting, entry.Name);
                DecryptScript(buffer);
            }
            bw.Write(buffer);
            buffer = null;
            ProgressManager.Progress();
        }
    }

    private static void DecryptScript(byte[] data)
    {
        byte key = 0xC5;
        for (int i = 0; i < data.Length; ++i)
        {
            data[i] ^= key;
            key += 0x5C;
        }
    }
}

internal partial class PJADVDATUnpackOptions : ArcOptions
{
    [ObservableProperty]
    private bool decryptScripts = true;
}

internal partial class PJADVDATPackOptions : ArcOptions
{
    [ObservableProperty]
    private bool encryptScripts = true;
    [ObservableProperty]
    private int version = 2;
    [ObservableProperty]
    private IReadOnlyList<int> versions = [1, 2];
}
