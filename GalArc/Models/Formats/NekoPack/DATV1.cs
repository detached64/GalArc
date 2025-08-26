using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GalArc.Models.Formats.NekoPack;

internal class DATV1 : ArcFormat
{
    public override string Name => "DAT";
    public override string Description => "NEKOPACK Archive v1";

    private const string Magic = "NEKOPACK";

    private class NekoPackEntry : Entry
    {
        public byte Base;
    }

    private readonly uint[] Temp = new uint[625];

    public override void Unpack(string input, string output)
    {
        using FileStream fs = File.OpenRead(input);
        using BinaryReader br = new(fs);
        if (!string.Equals(Encoding.ASCII.GetString(br.ReadBytes(8)), Magic))
        {
            throw new InvalidArchiveException();
        }

        fs.Position = 16;
        int count = br.ReadInt32();
        List<NekoPackEntry> entries = new(count);
        Init(0x9999);
        for (int i = 0; i < count; i++)
        {
            NekoPackEntry entry = new();
            entry.Base = br.ReadByte();
            byte name_len = br.ReadByte();
            byte[] raw_name = br.ReadBytes(name_len);
            entry.Name = Decrypt(raw_name, name_len);
            entry.Path = Path.Combine(output, entry.Name);
            entry.Offset = br.ReadUInt32();
            entry.Size = br.ReadUInt32();
            entries.Add(entry);
        }
        ProgressManager.SetMax(entries.Count);
        foreach (NekoPackEntry entry in entries)
        {
            fs.Position = entry.Offset;
            byte[] data = br.ReadBytes((int)entry.Size);
            Directory.CreateDirectory(Path.GetDirectoryName(entry.Path));
            if (entry.Name.HasAnyOfExtensions("txt", "img", "map"))     // Game: 3days～満ちてゆく刻の彼方で～
            {
                Decompress(entry, data);
            }
            File.WriteAllBytes(entry.Path, data);
            ProgressManager.Progress();
        }
    }

    private unsafe void Init(int key)
    {
        fixed (uint* data = Temp)
        {
            int* p32 = (int*)data;
            for (int i = 0; i < 624; i++)
            {
                key *= 0x10DCD;
                *p32 = key;
                p32++;
            }
            *p32 = 0;
        }
    }

    private string Decrypt(byte[] raw, int name_length)
    {
        for (int i = 0; i < name_length; i++)
        {
            raw[i] = DecryptByte(raw[i]);
        }
        return Encoding.GetEncoding(932).GetString(raw);
    }

    private byte DecryptByte(byte raw)
    {
        if (Temp[624] >= 0x270)
        {
            Init((int)Temp[623]);
        }
        int index = (int)Temp[624];
        Temp[624]++;
        uint t = (((((((Temp[index] >> 11) ^ Temp[index]) << 7) & 0x31518A63) ^ (Temp[index] >> 11) ^ Temp[index]) << 15) & 0x17F1CA43) ^ ((((Temp[index] >> 11) ^ Temp[index]) << 7) & 0x31518A63) ^ (Temp[index] >> 11) ^ Temp[index];
        return (byte)(raw ^ (t >> 18) ^ t);
    }

    private void Decompress(NekoPackEntry entry, byte[] data)
    {
        Init(entry.Base + 0x9999);
        Decrypt(data, data.Length);
    }
}
