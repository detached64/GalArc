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

        public override void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            string magic = Encoding.ASCII.GetString(br.ReadBytes(7));
            IndexReader reader = new IndexReader(br);
            List<PackedEntry> entries = new List<PackedEntry>();
            if (string.Equals(magic, MagicESC))
            {
                switch (br.ReadChar() - '0')
                {
                    case 1:
                        Logger.ShowVersion("bin", 1);
                        entries = reader.ReadEscV1();
                        break;
                    case 2:
                        Logger.ShowVersion("bin", 2);
                        entries = reader.ReadEscV2();
                        break;
                    default:
                        throw new InvalidVersionException(InvalidVersionType.Unknown);
                }
            }
            else if (string.Equals(magic, MagicACPX))
            {
                br.ReadChar();
                entries = reader.ReadACPX();
            }
            else
            {
                throw new InvalidArchiveException();
            }
            Directory.CreateDirectory(folderPath);
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
                entry.Path = Path.Combine(folderPath, entry.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(entry.Path));
                File.WriteAllBytes(entry.Path, data);
                Logger.UpdateBar();
                data = null;
            }
            fs.Dispose();
            br.Dispose();
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

        private sealed class IndexReader
        {
            private const uint c = 0x65AC9365;

            private BinaryReader Reader;

            private List<PackedEntry> Entries;

            private uint Seed;

            public IndexReader(BinaryReader reader)
            {
                Reader = reader;
                Entries = new List<PackedEntry>();
            }

            public List<PackedEntry> ReadACPX()
            {
                int fileCount = Reader.ReadInt32();
                for (int i = 0; i < fileCount; i++)
                {
                    PackedEntry entry = new PackedEntry();
                    long pos = Reader.BaseStream.Position;
                    entry.Name = Reader.ReadCString();
                    Reader.BaseStream.Position = pos + 32;
                    entry.Offset = Reader.ReadUInt32();
                    entry.Size = Reader.ReadUInt32();
                    Entries.Add(entry);
                }
                return Entries;
            }

            public List<PackedEntry> ReadEscV1()
            {
                Seed = Reader.ReadUInt32();
                uint count = Reader.ReadUInt32() ^ Get();
                uint indexSize = 0x88 * count;
                byte[] index = Reader.ReadBytes((int)indexSize);
                Decrypt(index);
                int offset = 0;
                for (int i = 0; i < count; i++)
                {
                    PackedEntry entry = new PackedEntry();
                    entry.Name = index.GetCString(offset);
                    offset += 0x80;
                    entry.Offset = BitConverter.ToUInt32(index, offset);
                    entry.Size = BitConverter.ToUInt32(index, offset + 4);
                    offset += 8;
                    Entries.Add(entry);
                }
                return Entries;
            }

            public List<PackedEntry> ReadEscV2()
            {
                Reader.BaseStream.Position = 8;
                Seed = Reader.ReadUInt32();
                uint count = Reader.ReadUInt32() ^ Get();
                uint size1 = Reader.ReadUInt32() ^ Get();
                uint size2 = 12 * count;
                byte[] index = Reader.ReadBytes((int)size2);
                byte[] names = Reader.ReadBytes((int)size1);
                Decrypt(index);
                for (int i = 0; i < count; i++)
                {
                    PackedEntry entry = new PackedEntry();
                    uint name_offset = BitConverter.ToUInt32(index, 12 * i);
                    entry.Offset = BitConverter.ToUInt32(index, 12 * i + 4);
                    entry.Size = BitConverter.ToUInt32(index, 12 * i + 8);
                    entry.Name = names.GetCString((int)name_offset);
                    Entries.Add(entry);
                }
                return Entries;
            }

            private unsafe void Decrypt(byte[] data)
            {
                fixed (byte* b = data)
                {
                    uint* data32 = (uint*)b;
                    for (int i = 0; i < data.Length / 4; i++)
                    {
                        data32[i] ^= Get();
                    }
                }
            }

            public uint Get()
            {
                Seed ^= (8 * (Seed ^ c ^ (2 * (Seed ^ c)))) ^ ((Seed ^ c ^ ((Seed ^ c) >> 1)) >> 3) ^ c;
                return Seed;
            }
        }
    }
}
