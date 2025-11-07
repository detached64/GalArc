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

namespace GalArc.Models.Formats.SFA;

internal class AOS : ArcFormat, IUnpackConfigurable, IPackConfigurable
{
    public override string Name => "AOS";
    public override string Description => "SFA AOS Archive";
    public override bool CanWrite => true;

    private SFAAOSUnpackOptions _unpackOptions;
    public ArcOptions UnpackOptions => _unpackOptions ??= new SFAAOSUnpackOptions();

    private SFAAOSPackOptions _packOptions;
    public ArcOptions PackOptions => _packOptions ??= new SFAAOSPackOptions();

    public override void Unpack(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        if (br.ReadUInt32() != 0)
            throw new InvalidArchiveException();
        uint dataOffset = br.ReadUInt32();
        uint indexSize = br.ReadUInt32();
        byte[] arcName = br.ReadBytes(261);
        if (Encoding.ASCII.GetString(arcName).TrimEnd('\0') != Path.GetFileName(filePath))
            Logger.Info("Archive name in header does not match file name.");
        int fileCount = (int)(indexSize / 40);
        ProgressManager.SetMax(fileCount);
        List<Entry> entries = new(fileCount);
        for (int i = 0; i < fileCount; i++)
        {
            Entry entry = new();
            entry.Name = ArcEncoding.Shift_JIS.GetString(br.ReadBytes(32)).TrimEnd('\0');
            entry.Offset = br.ReadUInt32() + dataOffset;
            entry.Size = br.ReadUInt32();
            entries.Add(entry);
        }
        foreach (Entry entry in entries)
        {
            string path = Path.Combine(folderPath, entry.Name);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            if (Path.GetExtension(path) == ".cmp")
                Path.ChangeExtension(path, ".abm");
            if (_unpackOptions.DecryptScripts && string.Equals(Path.GetExtension(entry.Name), ".scr", StringComparison.OrdinalIgnoreCase))
            {
                fs.Position = entry.Offset;
                int decompressedSize = br.ReadInt32();
                byte[] raw = br.ReadBytes((int)entry.Size - 4);
                Logger.DebugFormat(MsgStrings.Decrypting, entry.Name);
                File.WriteAllBytes(path, HuffmanHelper.Decode(raw, decompressedSize));
            }
            else
            {
                using SubStream subStream = new(fs, entry.Offset, entry.Size);
                using FileStream outFs = File.Create(path);
                subStream.CopyTo(outFs);
            }
            ProgressManager.Progress();
        }
    }

    public override void Pack(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        bw.Write(0);
        FileInfo[] files = new DirectoryInfo(folderPath).GetFiles();
        int fileCount = files.Length;
        int indexSize = fileCount * 40;
        int dataOffset = 261 + 12 + indexSize;
        bw.Write(dataOffset);
        bw.Write(indexSize);
        string name = Path.GetFileName(filePath);
        bw.Write(Encoding.ASCII.GetBytes(name.PadRight(261, '\0')));
        ProgressManager.SetMax(fileCount);
        fw.Position = dataOffset;
        List<Entry> entries = new(fileCount);
        foreach (FileInfo file in files)
        {
            Entry entry = new();
            entry.Name = file.Name;
            entry.Offset = (uint)(fw.Position - dataOffset);
            if (_packOptions.EncryptScripts && string.Equals(file.Extension, ".scr", StringComparison.OrdinalIgnoreCase))
            {
                Logger.DebugFormat(MsgStrings.Encrypting, file.Name);
                byte[] raw = File.ReadAllBytes(file.FullName);
                byte[] encoded = HuffmanHelper.Encode(raw);
                bw.Write(raw.Length);
                bw.Write(encoded);
                entry.Size = (uint)(4 + encoded.Length);
            }
            else
            {
                using FileStream fr = file.OpenRead();
                fr.CopyTo(fw);
                entry.Size = (uint)fr.Length;
            }
            entries.Add(entry);
            ProgressManager.Progress();
        }
        fw.Position = 261 + 12;
        foreach (Entry entry in entries)
        {
            byte[] nameBytes = ArcEncoding.Shift_JIS.GetBytes(entry.Name.PadRight(32, '\0'));
            bw.Write(nameBytes);
            bw.Write(entry.Offset);
            bw.Write(entry.Size);
        }
    }
}

internal partial class SFAAOSUnpackOptions : ArcOptions
{
    [ObservableProperty]
    private bool decryptScripts = true;
}

internal partial class SFAAOSPackOptions : ArcOptions
{
    [ObservableProperty]
    private bool encryptScripts = true;
}
