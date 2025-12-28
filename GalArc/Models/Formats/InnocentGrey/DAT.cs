using GalArc.I18n;
using GalArc.Infrastructure.Logging;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GalArc.Models.Formats.InnocentGrey;

internal class DAT : IGA
{
    public override string Name => "DAT";
    public override string Description => "InnocentGrey Archive";
    public override bool CanWrite => true;

    private const string Magic = "PACKDAT.";

    private class DatEntry : PackedEntry
    {
        public uint FileType { get; set; }
    }

    public override void Unpack(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        if (Encoding.ASCII.GetString(br.ReadBytes(8)) != Magic)
        {
            throw new InvalidArchiveException();
        }
        int fileCount = br.ReadInt32();
        br.BaseStream.Position += 4;

        ProgressManager.SetMax(fileCount);
        List<DatEntry> entries = [];

        for (int i = 0; i < fileCount; i++)
        {
            DatEntry entry = new();
            entry.Name = ArcEncoding.Shift_JIS.GetString(br.ReadBytes(32)).TrimEnd('\0');
            entry.Offset = br.ReadUInt32();
            entry.FileType = br.ReadUInt32();
            entry.UnpackedSize = br.ReadUInt32();
            entry.Size = br.ReadUInt32();
            entry.IsPacked = entry.Size != entry.UnpackedSize;
            //if (entry.IsCompressed)     //skip compressed data for now
            //{
            //    throw new NotImplementedException("Compressed data detected. Temporarily not supported.");
            //}
            entries.Add(entry);
        }

        foreach (DatEntry entry in entries)
        {
            fs.Position = entry.Offset;
            byte[] data = br.ReadBytes((int)entry.Size);
            if (_unpackOptions.DecryptScripts && Path.GetExtension(entry.Name) == ".s")
            {
                Logger.Debug(MsgStrings.Decrypting, entry.Name);
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] ^= 0xFF;
                }
            }
            File.WriteAllBytes(Path.Combine(folderPath, entry.Name), data);
            data = null;
            ProgressManager.Progress();
        }
    }

    public override void Pack(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        DirectoryInfo d = new(folderPath);
        FileInfo[] files = d.GetFiles();
        int fileCount = files.Length;
        ProgressManager.SetMax(fileCount);
        bw.Write(Encoding.ASCII.GetBytes(Magic));
        bw.Write(fileCount);
        bw.Write(fileCount);
        uint dataOffset = 16 + ((uint)fileCount * 48);

        foreach (FileInfo file in files)
        {
            bw.Write(Encoding.ASCII.GetBytes(file.Name.PadRight(32, '\0')));
            bw.Write(dataOffset);
            uint size = (uint)file.Length;
            dataOffset += size;
            bw.Write(0x20000000);
            bw.Write(size);
            bw.Write(size);
        }

        foreach (FileInfo file in files)
        {
            byte[] data = File.ReadAllBytes(file.FullName);
            if (_packOptions.EncryptScripts && file.Extension == ".s")
            {
                Logger.Debug(MsgStrings.Encrypting, file.Name);
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] ^= 0xFF;
                }
            }
            bw.Write(data);
            data = null;
            ProgressManager.Progress();
        }
    }
}
