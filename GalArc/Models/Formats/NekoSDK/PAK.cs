using CommunityToolkit.Mvvm.ComponentModel;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GalArc.Models.Formats.NekoSDK;

internal class PAK : ArcFormat, IPackConfigurable
{
    public override string Name => "PAK";
    public override string Description => "NekoSDK PAK Archive";
    public override bool CanWrite => true;

    private NekoSDKPAKPackOptions _packOptions;
    public ArcOptions PackOptions => _packOptions ??= new NekoSDKPAKPackOptions();

    private readonly string Magic = "NEKOPACK";

    public override void Unpack(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        if (Encoding.ASCII.GetString(br.ReadBytes(8)) != Magic)
        {
            throw new InvalidArchiveException();
        }
        string version = Encoding.ASCII.GetString(br.ReadBytes(2));
        uint dataOffset = version switch
        {
            "4A" => 10 + br.ReadUInt32(),
            "4S" => 16 + (uint)br.ReadUInt16(),
            _ => throw new InvalidVersionException(InvalidVersionType.Unknown),
        };
        List<Entry> entries = [];

        while (fs.Position < dataOffset)
        {
            Entry entry = new();
            int nameLen = br.ReadInt32();
            byte[] nameBuf = br.ReadBytes(nameLen);
            entry.Name = ArcEncoding.Shift_JIS.GetString(nameBuf).TrimEnd('\0');

            int key = 0;
            for (int i = 0; i < nameLen; ++i)
            {
                key += (sbyte)nameBuf[i];
            }
            entry.Offset = br.ReadUInt32() ^ (uint)key;
            entry.Size = br.ReadUInt32() ^ (uint)key;
            entries.Add(entry);
            nameBuf = null;
        }
        ProgressManager.SetMax(entries.Count);

        foreach (Entry entry in entries)
        {
            fs.Position = entry.Offset;
            byte[] header = br.ReadBytes(4);
            byte[] data = br.ReadBytes((int)(entry.Size - 8));
            uint key = (entry.Size / 8) + 0x22;
            for (int i = 0; i < 4; i++)
            {
                header[i] ^= (byte)key;
                key <<= 3;
            }
            byte[] fileData = ZlibHelper.Decompress([.. header, .. data]);
            File.WriteAllBytes(Path.Combine(folderPath, entry.Name), fileData);
            fileData = null;
            data = null;
            ProgressManager.Progress();
        }
    }

    public override void Pack(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);

        writer.Write(Encoding.ASCII.GetBytes(Magic));
        writer.Write(Encoding.ASCII.GetBytes("4A"));
        DirectoryInfo d = new(folderPath);
        FileInfo[] files = d.GetFiles();

        ProgressManager.SetMax(files.Length);
        uint baseOffset = ((uint)files.Length * 12) + 18;

        foreach (FileInfo file in files)
        {
            baseOffset += (uint)ArcEncoding.Shift_JIS.GetByteCount(file.Name) + 1;
        }
        writer.Write(baseOffset);
        fw.Position = baseOffset;

        foreach (FileInfo file in files)
        {
            string name = file.Name;
            byte[] nameBuf = ArcEncoding.Shift_JIS.GetBytes(name);
            writer.Write(nameBuf.Length + 1);
            writer.Write(nameBuf);
            writer.Write((byte)0);
            int key = 0;
            for (int i = 0; i < nameBuf.Length; ++i)
            {
                key += (sbyte)nameBuf[i];
            }
            writer.Write(baseOffset ^ (uint)key);

            byte[] data = ZlibHelper.Compress(file.FullName);
            uint size = (uint)data.Length + 4;
            uint dataKey = (size / 8) + 0x22;
            for (int i = 0; i < 4; i++)
            {
                data[i] ^= (byte)dataKey;
                dataKey <<= 3;
            }
            bw.Write(data);
            data = null;
            bw.Write((uint)file.Length);

            writer.Write(size ^ (uint)key);
            baseOffset += size;
            ProgressManager.Progress();
        }
        writer.Write(0);
        fw.Position = 0;
        stream.WriteTo(fw);
    }
}

internal partial class NekoSDKPAKPackOptions : ArcOptions
{
    [ObservableProperty]
    private string version = "4A";
    [ObservableProperty]
    private IReadOnlyList<string> versions = ["4A"];
}
