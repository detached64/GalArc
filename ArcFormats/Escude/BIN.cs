using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility;
using Utility.Exceptions;
using Utility.Extensions;

namespace ArcFormats.Escude
{
    public class BIN : ArcFormat
    {
        private readonly string MagicACPX = "ACPXPK0";
        private readonly string MagicESC = "ESC-ARC";

        private string Folder;

        public override void Unpack(string filePath, string folderPath)
        {
            Folder = folderPath;
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            string magic = Encoding.ASCII.GetString(br.ReadBytes(7));
            if (string.Equals(magic, MagicESC))
            {
                switch (br.ReadChar() - '0')
                {
                    case 1:
                        Logger.ShowVersion("bin", 1);
                        break;
                    case 2:
                        Logger.ShowVersion("bin", 2);
                        UnpackV2(br);
                        break;
                    default:
                        throw new InvalidDataException("Unknown version");
                }
            }
            else if (string.Equals(magic, MagicACPX))
            {
                br.ReadChar();
                UnpackACPX(br);
            }
            else
            {
                throw new InvalidArchiveException();
            }
            fs.Dispose();
            br.Dispose();
        }

        private void UnpackACPX(BinaryReader br)
        {
            int fileCount = br.ReadInt32();
            List<PackedEntry> entries = new List<PackedEntry>(fileCount);
            for (int i = 0; i < fileCount; i++)
            {
                PackedEntry entry = new PackedEntry();
                long pos = br.BaseStream.Position;
                entry.Name = br.ReadCString();
                br.BaseStream.Position = pos + 32;
                entry.Offset = br.ReadUInt32();
                entry.Size = br.ReadUInt32();
                entry.Path = Path.Combine(Folder, entry.Name);
                entries.Add(entry);
            }
            Extract(entries, br);
        }

        private void UnpackV2(BinaryReader br)
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
                entry.Path = Path.Combine(Folder, entry.Name);
                entries.Add(entry);
            }
            Extract(entries, br);
        }

        private void Extract(List<PackedEntry> entries, BinaryReader br)
        {
            Directory.CreateDirectory(Folder);
            Logger.InitBar(entries.Count);
            foreach (PackedEntry entry in entries)
            {
                br.BaseStream.Position = entry.Offset;
                byte[] data = br.ReadBytes((int)entry.Size);
                if (BitConverter.ToUInt32(data, 0) == 0x00706361)  // "acp\0"
                {
                    entry.UnpackedSize = BigEndian.Convert(BitConverter.ToUInt32(data, 4));
                    byte[] raw = new byte[data.Length - 8];
                    Buffer.BlockCopy(data, 8, raw, 0, raw.Length);
                    data = Decompress(raw, (int)entry.UnpackedSize);
                    raw = null;
                }
                Directory.CreateDirectory(Path.GetDirectoryName(entry.Path));
                File.WriteAllBytes(entry.Path, data);
                Logger.UpdateBar();
                data = null;
            }
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

        private byte[] Decompress(byte[] data, int unpacked_size)
        {
            byte[] output = new byte[unpacked_size];
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BitStream input = new BitStream(ms, BitStreamMode.Read))
                {
                    int[] dic = new int[0x8900];
                    int dic_pos = 0;
                    int dst_pos = 0;
                    int length = 9;
                    while (dst_pos < unpacked_size)
                    {
                        int prefix_code = input.ReadBits(length);
                        switch (prefix_code)
                        {
                            case -1:
                                throw new EndOfStreamException();
                            case 256:
                                break;
                            case 257:
                                length++;
                                if (length > 24)
                                {
                                    throw new InvalidDataException(nameof(length));
                                }
                                break;
                            case 258:
                                length = 9;
                                dic_pos = 0;
                                break;
                            default:
                                dic[dic_pos++] = dst_pos;
                                if (prefix_code < 256)
                                {
                                    output[dst_pos++] = (byte)prefix_code;
                                }
                                else
                                {
                                    prefix_code -= 259;
                                    int offset = dic[prefix_code];
                                    int count = Math.Min(unpacked_size - dst_pos, dic[prefix_code + 1] - offset + 1);
                                    Binary.CopyOverlapped(output, offset, dst_pos, count);
                                    dst_pos += count;
                                }
                                break;
                        }
                    }
                }
            }

            return output;
        }

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
