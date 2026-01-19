using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GalArc.Models.Formats.NekoPack;

internal class DATV2 : ArcFormat
{
    public override string Name => "DAT";
    public override string Description => "NEKOPACK Archive v2";

    private const string Magic = "NEKOPACK";

    private class NekoPackEntry : Entry
    {
        public uint Key { get; set; }
    }

    public override void Unpack(string input, string output)
    {
        using FileStream fs = File.OpenRead(input);
        using BinaryReader br = new(fs);
        if (!string.Equals(Encoding.ASCII.GetString(br.ReadBytes(8)), Magic))
        {
            throw new InvalidArchiveException();
        }
        fs.Position += 4;
        uint key = br.ReadUInt32();
        byte[] table = br.ReadBytes(0x400);
        uint round = (key % 7) + 3;
        while (round-- > 0)
        {
            Decrypt(table, 0x400, table, key, true);
        }
        key = br.ReadUInt32();
        byte[] indexInfo = br.ReadBytes(8);
        Decrypt(indexInfo, 8, table, key);
        int indexSize = BitConverter.ToInt32(indexInfo, 0);
        byte[] index = br.ReadBytes(indexSize);
        Decrypt(index, indexSize, table, key);
        uint dataOffset = 0x400 + 0x10 + 4 + 8 + (uint)indexSize;
        List<NekoPackEntry> entries = [];
        using (MemoryStream ms = new(index))
        using (BinaryReader indexReader = new(ms))
        {
            int dirCount = indexReader.ReadInt32();
            for (int i = 0; i < dirCount; i++)
            {
                int nameLength = indexReader.ReadByte();
                string dirName = ArcEncoding.Shift_JIS.GetString(indexReader.ReadBytes(nameLength));
                int fileCount = indexReader.ReadInt32();
                for (int j = 0; j < fileCount; j++)
                {
                    ms.Position++;
                    int fileNameLength = indexReader.ReadByte();
                    string fileName = ArcEncoding.Shift_JIS.GetString(indexReader.ReadBytes(fileNameLength));
                    NekoPackEntry entry = new()
                    {
                        Path = Path.Combine(dirName, fileName),
                        Offset = indexReader.ReadUInt32() + dataOffset
                    };
                    entries.Add(entry);
                }
            }
        }
        ProgressManager.SetMax(entries.Count);
        foreach (NekoPackEntry entry in entries)
        {
            fs.Position = entry.Offset;
            entry.Key = br.ReadUInt32();
            byte[] fileInfo = br.ReadBytes(8);
            Decrypt(fileInfo, 8, table, entry.Key);
            int fileSize = BitConverter.ToInt32(fileInfo, 0);
            string filePath = Path.Combine(output, entry.Path);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            byte[] data = br.ReadBytes(fileSize);
            Decrypt(data, fileSize, table, entry.Key);
            File.WriteAllBytes(filePath, data);
            ProgressManager.Progress();
        }
    }

    private static unsafe void Decrypt(byte[] data, int length, byte[] table, uint key, bool init = false)
    {
        int count = length / 4;
        fixed (byte* data8 = data)
        {
            uint* data32 = (uint*)data8;
            while (count-- > 0)
            {
                uint s = *data32;
                key = (key + 0xC3) % 0x200;
                uint d = s ^ BitConverter.ToUInt32(table, (int)key);
                key += init ? s : d;
                *data32++ = d;
            }
        }
    }
}
