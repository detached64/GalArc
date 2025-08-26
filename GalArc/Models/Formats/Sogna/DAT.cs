using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GalArc.Models.Formats.Sogna;

internal class DAT : ArcFormat
{
    public override string Name => "DAT";
    public override string Description => "Sogna Archive";

    private readonly string Magic = "SGS.DAT 1.00";

    public override void Unpack(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);

        if (Encoding.ASCII.GetString(br.ReadBytes(12)) != Magic)
        {
            throw new InvalidArchiveException();
        }

        int fileCount = br.ReadInt32();
        uint indexOffset = 0x10;
        List<PackedEntry> entries = new(fileCount);
        for (int i = 0; i < fileCount; i++)
        {
            PackedEntry entry = new();
            br.BaseStream.Position = indexOffset;
            entry.Name = ArcEncoding.Shift_JIS.GetString(br.ReadBytes(0x10)).TrimEnd('\0');
            br.BaseStream.Position = indexOffset + 0x13;
            entry.IsPacked = br.ReadByte() != 0;
            br.BaseStream.Position = indexOffset + 0x14;
            entry.Size = br.ReadUInt32();
            br.BaseStream.Position = indexOffset + 0x18;
            entry.UnpackedSize = br.ReadUInt32();
            br.BaseStream.Position = indexOffset + 0x1C;
            entry.Offset = br.ReadUInt32();
            entries.Add(entry);
            indexOffset += 0x20;
        }

        ProgressManager.SetMax(fileCount);
        foreach (PackedEntry entry in entries)
        {
            br.BaseStream.Position = entry.Offset;
            string fileName = Path.Combine(folderPath, entry.Name);
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            byte[] data = br.ReadBytes((int)entry.Size);
            if (entry.IsPacked)
            {
                br.BaseStream.Position = entry.Offset;
                byte[] unpacked = new byte[entry.UnpackedSize];
                LzUnpack(data, unpacked);
                File.WriteAllBytes(fileName, unpacked);
            }
            else
            {
                File.WriteAllBytes(fileName, data);
            }
            ProgressManager.Progress();
        }
    }

    public override void Pack(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);

        FileInfo[] files = new DirectoryInfo(folderPath).GetFiles("*", SearchOption.AllDirectories);
        ProgressManager.SetMax(files.Length);

        bw.Write(Encoding.ASCII.GetBytes(Magic));
        bw.BaseStream.Position = 12;
        bw.Write(files.Length);

        long indexOffset = 0x10;
        long dataOffset = 0x10 + (files.Length * 0x20);

        foreach (FileInfo file in files)
        {
            string relativePath = file.FullName[(folderPath.Length + 1)..];

            bw.BaseStream.Position = indexOffset;
            bw.WritePaddedString(relativePath, 0x10);
            bw.BaseStream.Position = indexOffset + 0x13;
            bw.Write((byte)0); // Not packed
            bw.BaseStream.Position = indexOffset + 0x14;
            bw.Write((uint)file.Length);
            bw.BaseStream.Position = indexOffset + 0x18;
            bw.Write((uint)file.Length);
            bw.BaseStream.Position = indexOffset + 0x1C;
            bw.Write((uint)dataOffset);
            indexOffset += 0x20;

            bw.BaseStream.Position = dataOffset;
            byte[] fileData = File.ReadAllBytes(file.FullName);
            bw.Write(fileData);
            dataOffset += file.Length;
            ProgressManager.Progress();
        }
    }

    private static void LzUnpack(byte[] input, byte[] output)
    {
        using MemoryStream ms = new(input);
        using BinaryReader reader = new(ms);
        int dst = 0;
        int bits = 0;
        byte mask = 0;
        while (dst < output.Length)
        {
            mask >>= 1;
            if (mask == 0)
            {
                bits = reader.ReadByte();
                if (-1 == bits)
                {
                    break;
                }
                mask = 0x80;
            }
            if ((mask & bits) != 0)
            {
                int offset = reader.ReadUInt16();
                int count = (offset >> 12) + 1;
                offset &= 0xFFF;
                Binary.CopyOverlapped(output, dst - offset, dst, count);
                dst += count;
            }
            else
            {
                output[dst++] = reader.ReadByte();
            }
        }
    }
}
