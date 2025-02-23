using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility.Extensions;

namespace ArcFormats.Escude
{
    public class BIN : ArchiveFormat
    {
        private readonly string Magic = "ESC-ARC";
        public override void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            if (!string.Equals(Encoding.ASCII.GetString(br.ReadBytes(7)), Magic))
            {
                Logger.ErrorInvalidArchive();
            }
            switch (br.ReadChar() - '0')
            {
                case 1:
                    break;
                case 2:
                    UnpackV2(br, folderPath);
                    break;
                default:
                    throw new Exception("Unknown ver");
            }
            fs.Dispose();
            br.Dispose();
        }

        private void UnpackV2(BinaryReader br, string folder)
        {
            br.BaseStream.Position = 8;
            uint seed = br.ReadUInt32();
            Keygen keygen = new Keygen(seed);
            uint count = br.ReadUInt32() ^ keygen.Get();
            uint size1 = br.ReadUInt32() ^ keygen.Get();
            uint size2 = 12 * count;
            byte[] index = br.ReadBytes((int)size2);
            byte[] names = br.ReadBytes((int)size1);
            Decrypt(index, keygen);
            List<PackedEntry> entries = new List<PackedEntry>((int)count);
            for (int i = 0; i < count; i++)
            {
                PackedEntry entry = new PackedEntry();
                uint name_offset = BitConverter.ToUInt32(index, 12 * i);
                entry.Offset = BitConverter.ToUInt32(index, 12 * i + 4);
                entry.Size = BitConverter.ToUInt32(index, 12 * i + 8);
                entry.Name = names.GetCString((int)name_offset);
                entry.Path = Path.Combine(folder, entry.Name);
                entries.Add(entry);
            }
            Directory.CreateDirectory(folder);
            Logger.InitBar(entries.Count);
            foreach (var entry in entries)
            {
                br.BaseStream.Position = entry.Offset;
                byte[] data = br.ReadBytes((int)entry.Size);
                File.WriteAllBytes(entry.Path, data);
                Logger.UpdateBar();
            }
            br.BaseStream.Dispose();
            br.Dispose();
        }

        private unsafe void Decrypt(byte[] data, Keygen keygen)
        {
            fixed (byte* b = data)
            {
                uint* data32 = (uint*)b;
                for (int i = 0; i < data.Length / 4; i++)
                {
                    data32[i] ^= keygen.Get();
                }
            }
        }

        //private byte[] Decompress(byte[] data)
        //{

        //}

        private class Keygen
        {
            private const uint c = 0x65AC9365;
            private uint seed;

            public Keygen(uint s)
            {
                seed = s;
            }

            public uint Get()
            {
                seed ^= (8 * (seed ^ c ^ (2 * (seed ^ c)))) ^ ((seed ^ c ^ ((seed ^ c) >> 1)) >> 3) ^ c;
                return seed;
            }
        }
    }
}
