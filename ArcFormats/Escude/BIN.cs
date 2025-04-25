using GalArc.Logs;
using GalArc.Templates;
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
        public override WidgetTemplate PackWidget => PackBINWidget.Instance;

        private VersionOptions Options => PackBINWidget.Instance.Options;

        private const string MagicACPX = "ACPXPK01";
        private const string MagicESC1 = "ESC-ARC1";
        private const string MagicESC2 = "ESC-ARC2";

        public override void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            string magic = Encoding.ASCII.GetString(br.ReadBytes(8));
            IndexReader reader = new IndexReader(br);
            List<PackedEntry> entries;
            switch (magic)
            {
                case MagicACPX:
                    entries = reader.ReadACPX();
                    break;
                case MagicESC1:
                    entries = reader.ReadEscV1();
                    break;
                case MagicESC2:
                    entries = reader.ReadEscV2();
                    break;
                default:
                    throw new InvalidVersionException(InvalidVersionType.Unknown);
            }
            Logger.ShowVersion("bin", magic);
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

        public override void Pack(string folderPath, string filePath)
        {
            FileInfo[] files = new DirectoryInfo(folderPath).GetFiles("*");
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            bw.Write(Encoding.ASCII.GetBytes(Options.Version));
            using (IndexBuilder builder = new IndexBuilder(files, Options.Version))
            {
                bw.Write(builder.Build());
            }
            Logger.InitBar(files.Length);
            foreach (FileInfo file in files)
            {
                bw.Write(File.ReadAllBytes(file.FullName));
                Logger.UpdateBar();
            }
            fw.Dispose();
            bw.Dispose();
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

        private abstract class IndexProcessor
        {
            protected const uint C = 0x65AC9365;
            protected uint Seed;

            protected unsafe void Decrypt(byte[] data)
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

            protected uint Get()
            {
                Seed ^= (8 * (Seed ^ C ^ (2 * (Seed ^ C)))) ^ ((Seed ^ C ^ ((Seed ^ C) >> 1)) >> 3) ^ C;
                return Seed;
            }
        }

        private sealed class IndexReader : IndexProcessor
        {
            private readonly BinaryReader Reader;

            private readonly List<PackedEntry> Entries = new List<PackedEntry>();

            public IndexReader(BinaryReader reader)
            {
                Reader = reader;
                Reader.BaseStream.Position = 8;
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
                Seed = Reader.ReadUInt32();
                uint count = Reader.ReadUInt32() ^ Get();
                uint nameSize = Reader.ReadUInt32() ^ Get();
                uint indexSize = 12 * count;
                byte[] index = Reader.ReadBytes((int)indexSize);
                byte[] names = Reader.ReadBytes((int)nameSize);
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
        }

        private sealed class IndexBuilder : IndexProcessor, IDisposable
        {
            private readonly MemoryStream Stream;
            private readonly BinaryWriter Writer;
            private readonly FileInfo[] Files;
            private readonly string Magic;

            public IndexBuilder(FileInfo[] files, string magic)
            {
                Stream = new MemoryStream();
                Writer = new BinaryWriter(Stream);
                Files = files;
                Magic = magic;
            }

            public byte[] Build()
            {
                switch (Magic)
                {
                    case MagicACPX:
                        return BuildACPX();
                    case MagicESC1:
                        return BuildEscV1();
                    case MagicESC2:
                        return BuildEscV2();
                    default:
                        throw new InvalidVersionException(InvalidVersionType.NotSupported);
                }
            }

            private byte[] BuildACPX()
            {
                uint baseOffset = 8 + 4 + 40 * (uint)Files.Length;
                Writer.Write(Files.Length);
                foreach (FileInfo file in Files)
                {
                    Writer.WritePaddedString(file.Name, 32);
                    Writer.Write(baseOffset);
                    Writer.Write((uint)file.Length);
                    baseOffset += (uint)file.Length;
                }
                return Stream.ToArray();
            }

            private byte[] BuildEscV1()
            {
                uint baseOffset = 8 + 8 + 0x88 * (uint)Files.Length;
                Seed = 0;
                Writer.Write(Seed);
                Writer.Write((uint)Files.Length ^ Get());
                using (MemoryStream indexStream = new MemoryStream())
                {
                    using (BinaryWriter indexWriter = new BinaryWriter(indexStream))
                    {
                        foreach (FileInfo file in Files)
                        {
                            indexWriter.WritePaddedString(file.Name, 0x80);
                            indexWriter.Write(baseOffset);
                            indexWriter.Write((uint)file.Length);
                            baseOffset += (uint)file.Length;
                        }
                    }
                    byte[] index = indexStream.ToArray();
                    Decrypt(index);
                    Writer.Write(index);
                }
                return Stream.ToArray();
            }

            private byte[] BuildEscV2()
            {
                byte[] names = CollectNames();
                uint nameOffset = 0;
                uint baseOffset = 8 + 12 + 12 * (uint)Files.Length + (uint)names.Length;
                Seed = 0;
                Writer.Write(Seed);
                Writer.Write((uint)Files.Length ^ Get());
                Writer.Write((uint)names.Length ^ Get());
                using (MemoryStream indexStream = new MemoryStream())
                {
                    using (BinaryWriter indexWriter = new BinaryWriter(indexStream))
                    {
                        foreach (FileInfo file in Files)
                        {
                            indexWriter.Write(nameOffset);
                            nameOffset += (uint)(ArcEncoding.Shift_JIS.GetBytes(file.Name).Length + 1);
                            indexWriter.Write(baseOffset);
                            uint size = (uint)file.Length;
                            indexWriter.Write(size);
                            baseOffset += size;
                        }
                    }
                    byte[] index = indexStream.ToArray();
                    Decrypt(index);
                    Writer.Write(index);
                    Writer.Write(names);
                }
                return Stream.ToArray();
            }

            private byte[] CollectNames()
            {
                using (MemoryStream namesStream = new MemoryStream())
                {
                    using (BinaryWriter namesWriter = new BinaryWriter(namesStream))
                    {
                        foreach (FileInfo file in Files)
                        {
                            namesWriter.Write(ArcEncoding.Shift_JIS.GetBytes(file.Name));
                            namesWriter.Write('\0');
                        }
                    }
                    return namesStream.ToArray();
                }
            }

            public void Dispose()
            {
                Stream.Dispose();
                Writer.Dispose();
            }
        }
    }
}
