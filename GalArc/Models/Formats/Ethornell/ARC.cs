using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.Infrastructure.Logging;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GalArc.Models.Formats.Ethornell;

internal class ARC : ArcFormat, IPackConfigurable
{
    public override string Name => "ARC";
    public override string Description => "Ethornell/BGI Archive";
    public override bool CanWrite => true;

    private EthornellARCPackOptions _packOptions;
    public ArcOptions PackOptions => _packOptions ??= new EthornellARCPackOptions();

    private const string Magic = "PackFile    ";
    private const string Magic20 = "BURIKO ARC20";
    private const string MagicDSCFormat100 = "DSC FORMAT 1.00\0";

    public override void Unpack(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        int version = Encoding.ASCII.GetString(br.ReadBytes(12)) switch
        {
            Magic => 1,
            Magic20 => 2,
            _ => throw new InvalidArchiveException(),
        };
        Logger.ShowVersion("arc", version);
        int count = br.ReadInt32();
        List<Entry> entries = new(count);
        uint baseOffset = (uint)(16 + ((version == 1 ? 0x20 : 0x80) * count));
        for (int i = 0; i < count; i++)
        {
            Entry entry = new();
            entry.Name = ArcEncoding.Shift_JIS.GetString(br.ReadBytes(version == 1 ? 0x10 : 0x60)).TrimEnd('\0');
            entry.Offset = br.ReadUInt32() + baseOffset;
            entry.Size = br.ReadUInt32();
            br.BaseStream.Position += version == 1 ? 8 : 0x18;
            entries.Add(entry);
        }
        ProgressManager.SetMax(count);
        foreach (Entry entry in entries)
        {
            br.BaseStream.Position = entry.Offset;
            byte[] data = br.ReadBytes((int)entry.Size);
            File.WriteAllBytes(Path.Combine(folderPath, entry.Name), Decrypt(data));
            ProgressManager.Progress();
            data = null;
        }
    }

    public override void Pack(string folderPath, string filePath)
    {
        FileInfo[] files = new DirectoryInfo(folderPath).GetFiles();
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        string magic = _packOptions.Version == 1 ? Magic : Magic20;
        bw.Write(Encoding.ASCII.GetBytes(magic));
        bw.Write(files.Length);
        uint baseOffset = 0;
        foreach (FileInfo file in files)
        {
            bw.WritePaddedString(file.Name, _packOptions.Version == 1 ? 0x10 : 0x60);
            bw.Write(baseOffset);
            uint size = (uint)file.Length;
            bw.Write(size);
            baseOffset += size;
            bw.Write(new byte[_packOptions.Version == 1 ? 8 : 0x18]);
        }
        foreach (FileInfo file in files)
        {
            byte[] data = File.ReadAllBytes(file.FullName);
            bw.Write(data);
            data = null;
        }
    }

    private static byte[] Decrypt(byte[] data)
    {
        switch (Encoding.ASCII.GetString(data, 0, 16))
        {
            case MagicDSCFormat100:
                DscDecoder dscDecoder = new(data);
                return dscDecoder.Decode();
            default:
                return data;
        }
    }
}

internal partial class EthornellARCPackOptions : ArcOptions
{
    [ObservableProperty]
    private IReadOnlyList<int> versions = [1, 2];
    [ObservableProperty]
    private int version = 2;
}
