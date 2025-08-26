using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace GalArc.Models.Formats.Ai5Win;

internal class ARC : ArcFormat
{
    public override string Name => "ARC";
    public override string Description => "Ai5Win Archive";

    private readonly int[] NameLengths = [0x14, 0x18, 0x1E, 0x20, 0x100];

    private List<Entry> entries;

    public override void Unpack(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        int fileCount = br.ReadInt32();
        ProgressManager.SetMax(fileCount);

        TryReadIndex(br, fileCount, folderPath);

        foreach (Entry entry in entries)
        {
            fs.Position = entry.Offset;
            using Stream entryStream = new SubStream(fs, entry.Offset, entry.Size, true);
            using FileStream output = File.Create(entry.Path);
            if (entry.Path.HasAnyOfExtensions("mes", "lib", "a", "a6", "msk", "x", "s4", "dat", "map"))
            {
                using LzssStream lzss = new(entryStream, CompressionMode.Decompress);
                lzss.CopyTo(output);
            }
            else
            {
                entryStream.CopyTo(output);
            }
            ProgressManager.Progress();
        }
    }

    private void TryReadIndex(BinaryReader br, int fileCount, string folderPath)
    {
        entries = new List<Entry>(fileCount);
        foreach (int nameLength in NameLengths)
        {
            try
            {
                entries.Clear();
                ArcScheme scheme = new();
                scheme.GuessScheme(br, fileCount, nameLength);
                br.BaseStream.Position = 4;
                for (int i = 0; i < fileCount; i++)
                {
                    Entry entry = new();
                    byte[] nameBuf = br.ReadBytes(nameLength);
                    for (int j = 0; j < nameLength; j++)
                    {
                        nameBuf[j] ^= scheme.NameKey;
                    }
                    entry.Name = ArcEncoding.Shift_JIS.GetString(nameBuf).TrimEnd('\0');
                    if (entry.Name.ContainsInvalidChars())
                    {
                        throw new Exception();
                    }
                    entry.Path = Path.Combine(folderPath, entry.Name);
                    entry.Size = br.ReadUInt32() ^ scheme.SizeKey;
                    entry.Offset = br.ReadUInt32() ^ scheme.OffsetKey;
                    entries.Add(entry);
                }
                return;
            }
            catch
            { }
        }
        throw new InvalidOperationException("Failed to read index.");
    }

    protected class ArcScheme
    {
        public byte NameKey { get; set; }
        public uint SizeKey { get; set; }
        public uint OffsetKey { get; set; }

        public void GuessScheme(BinaryReader br, int fileCount, int nameLen)
        {
            // guess name key
            br.BaseStream.Position = nameLen + 3;    // last byte of name , hopefully 0x00
            NameKey = br.ReadByte();
            // guess offset key
            uint dataOffset = (uint)(((nameLen + 8) * fileCount) + 4);
            uint size = br.ReadUInt32();
            uint offset1 = br.ReadUInt32();
            OffsetKey = offset1 ^ dataOffset;
            // guess size key
            br.BaseStream.Position += nameLen + 4;
            uint offset2 = br.ReadUInt32() ^ OffsetKey;
            SizeKey = size ^ (offset2 - dataOffset);
        }
    }
}
