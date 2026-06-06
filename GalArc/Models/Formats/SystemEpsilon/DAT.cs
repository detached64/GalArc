using GalArc.I18n;
using GalArc.Infrastructure.Logging;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Formats.InnocentGrey;
using GalArc.Models.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GalArc.Models.Formats.SystemEpsilon;

internal class DAT : IGA
{
    public override string Name => "DAT";
    public override string Description => "SYSTEM-ε DAT Archive";
    public override bool CanWrite => true;

    private InnocentGreyIGAUnpackOptions _unpackOptions;
    public override ArcOptions UnpackOptions => _unpackOptions ??= new InnocentGreyIGAUnpackOptions();

    private InnocentGreyIGAPackOptions _packOptions;
    public override ArcOptions PackOptions => _packOptions ??= new InnocentGreyIGAPackOptions();

    private const string Magic = "PACKDAT.";

    private class DatEntry : PackedEntry
    {
        public int BwtIndex { get; set; }
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
            entry.BwtIndex = br.ReadInt32();
            entry.UnpackedSize = br.ReadUInt32();
            entry.Size = br.ReadUInt32();
            entry.IsPacked = entry.Size != entry.UnpackedSize;
            entries.Add(entry);
        }

        foreach (DatEntry entry in entries)
        {
            fs.Position = entry.Offset;
            byte[] data = br.ReadBytes((int)entry.Size);
            if (entry.IsPacked)
            {
                data = Decompress(data, (int)entry.UnpackedSize, entry.BwtIndex);
            }
            if (_unpackOptions.DecryptScripts && Path.GetExtension(entry.Name) == ".s")
            {
                Logger.Debug(MsgStrings.Decrypting, entry.Name);
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] ^= 0xFF;
                }
            }
            File.WriteAllBytes(Path.Combine(folderPath, entry.Name), data);
            ProgressManager.Progress();
        }
    }

    private static byte[] Decompress(byte[] packedData, int unpackedSize, int bwtIndex)
    {
        if (unpackedSize == 0)
            return [];

        byte[] decoded = new byte[unpackedSize];
        List<int> l = new(16);

        using (MemoryStream ms = new(packedData))
        using (BitStream br = new(ms))
        {
            for (int i = 0; i < unpackedSize; i++)
            {
                int sym;
                if (br.ReadBit() == 1)
                {
                    int rank = 1;
                    if (br.ReadBit() == 0)
                    {
                        int depth = 1;
                        while (br.ReadBit() == 0)
                            depth++;
                        while (depth-- > 0)
                            rank = (rank << 1) | br.ReadBit();
                    }
                    sym = (rank > 0 && rank <= l.Count) ? l[^rank] : 256;
                }
                else
                {
                    sym = br.ReadBits(8);
                }

                decoded[i] = (byte)sym;

                l.Remove(sym);
                l.Add(sym);
                if (l.Count >= 16)
                    l.RemoveAt(0);
            }
        }

        if (bwtIndex < 0 || bwtIndex >= unpackedSize)
            throw new InvalidDataException($"Invalid BWT primary index: {bwtIndex}");

        int[] count = new int[256];
        foreach (byte b in decoded)
            count[b]++;
        for (int i = 1; i < 256; i++)
            count[i] += count[i - 1];

        int[] lf = new int[unpackedSize];
        for (int i = unpackedSize - 1; i >= 0; i--)
            lf[--count[decoded[i]]] = i;

        byte[] dst = new byte[unpackedSize];
        for (int i = 0, idx = lf[bwtIndex]; i < unpackedSize; i++)
        {
            dst[i] = decoded[idx];
            idx = lf[idx];
        }

        return dst;
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
            ProgressManager.Progress();
        }
    }
}
