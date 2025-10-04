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

internal class AOS : ArcFormat, IUnpackConfigurable
{
    public override string Name => "AOS";
    public override string Description => "SFA AOS Archive";

    private SFAAOSUnpackOptions _unpackOptions;
    public ArcOptions UnpackOptions => _unpackOptions ??= new SFAAOSUnpackOptions();

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
}

internal partial class SFAAOSUnpackOptions : ArcOptions
{
    [ObservableProperty]
    private bool decryptScripts = true;
}
