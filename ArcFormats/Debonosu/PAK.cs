using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility;
using Utility.Compression;
using Utility.Exceptions;
using Utility.Extensions;

namespace ArcFormats.Debonosu
{
    public class PAK : ArcFormat
    {
        private readonly string Magic = "PAK\0";

        public override void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            if (Encoding.ASCII.GetString(br.ReadBytes(4)) != Magic)
            {
                throw new InvalidArchiveException();
            }
            uint info_offset = br.ReadUInt16();
            fs.Position = info_offset;
            uint index_offset = br.ReadUInt32();
            fs.Position += 4;
            int root_count = br.ReadInt32();
            fs.Position += 4;
            uint size = br.ReadUInt32();
            fs.Position += 4;
            byte[] index = ZlibHelper.Decompress(br.ReadBytes((int)size));
            Directory.CreateDirectory(folderPath);
            using (MemoryStream index_stream = new MemoryStream(index))
            using (IndexReader index_reader = new IndexReader(index_stream, info_offset + index_offset + size))
            {
                List<PackedEntry> entries = index_reader.Read(root_count);
                Logger.InitBar(entries.Count);
                foreach (PackedEntry entry in entries)
                {
                    fs.Position = entry.Offset;
                    byte[] data = br.ReadBytes((int)entry.Size);
                    string path = Path.Combine(folderPath, entry.Path);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    if (entry.IsPacked)
                    {
                        data = ZlibHelper.Decompress(data);
                    }
                    File.WriteAllBytes(path, data);
                    data = null;
                    Logger.UpdateBar();
                }
            }

            fs.Dispose();
            br.Dispose();
        }

        public override void Pack(string folderPath, string filePath)
        {
            using (FileStream fw = File.Create(filePath))
            using (BinaryWriter bw = new BinaryWriter(fw))
            {
                bw.Write(Encoding.ASCII.GetBytes(Magic));
                bw.Write((ushort)0x10);
                bw.Write((ushort)6);
                bw.Write((long)1);
                bw.Write(0x18);
                bw.Write(0);
                bw.Write(1);

                using (MemoryStream data = new MemoryStream())
                using (BinaryWriter data_writer = new BinaryWriter(data))
                using (IndexBuilder builder = new IndexBuilder(data_writer))
                {
                    byte[] index = builder.Build(folderPath);
                    bw.Write((uint)index.Length);
                    byte[] compressed = DeflateHelper.Compress(index);
                    bw.Write((uint)compressed.Length);
                    bw.Write(0);
                    bw.Write(compressed);
                    data.Position = 0;
                    fw.Position = fw.Length;
                    data.CopyTo(fw);
                }
            }
        }

        private sealed class IndexReader : IDisposable
        {
            private readonly MemoryStream Index;

            private readonly BinaryReader Reader;

            private readonly uint BaseOffset;

            private readonly List<PackedEntry> Entries = new List<PackedEntry>();

            public IndexReader(MemoryStream index, uint baseOffset)
            {
                Index = index;
                Reader = new BinaryReader(index);
                BaseOffset = baseOffset;
            }

            public List<PackedEntry> Read(int count)
            {
                ReadDir(string.Empty, count);
                return Entries;
            }

            /// <summary>
            /// <param name="path">Relative path to root.</param>
            /// <param name="item_count">Total count of folders and files in it. Only top directory.</param>
            private void ReadDir(string path, int item_count)
            {
                for (int i = 0; i < item_count; i++)
                {
                    long offset = Reader.ReadInt64();
                    long unpacked_size = Reader.ReadInt64();        // Also used as item_count for folders
                    long packed_size = Reader.ReadInt64();
                    uint flag = Reader.ReadUInt32();
                    Reader.BaseStream.Position += 24;               // Skip time
                    string name = Path.Combine(path, Reader.ReadCString());
                    if ((flag & 0x10) != 0)
                    {
                        ReadDir(name, (int)unpacked_size);
                    }
                    else
                    {
                        Entries.Add(new PackedEntry
                        {
                            Path = name,                            // Relative path here, still need combining
                            Offset = (uint)(BaseOffset + offset),
                            UnpackedSize = (uint)unpacked_size,
                            Size = (uint)packed_size,
                            IsPacked = unpacked_size != packed_size
                        });
                    }
                }
            }

            public void Dispose()
            {
                Index.Dispose();
                Reader.Dispose();
            }
        }

        private sealed class IndexBuilder : IDisposable
        {
            private readonly MemoryStream Index = new MemoryStream();
            private readonly BinaryWriter IndexWriter;
            private readonly BinaryWriter DataWriter;
            private long Offset = 0;

            public IndexBuilder(BinaryWriter writer)
            {
                IndexWriter = new BinaryWriter(Index);
                DataWriter = writer;
            }

            public byte[] Build(string dir)
            {
                foreach (string file in Directory.GetFiles(dir))
                {
                    WriteFile(file);
                }
                foreach (string subdir in Directory.GetDirectories(dir))
                {
                    WriteDir(subdir);
                }
                return Index.ToArray();
            }

            private void WriteDir(string dir)
            {
                string[] files = Directory.GetFiles(dir);
                string[] dirs = Directory.GetDirectories(dir);
                long count = files.Length + dirs.Length;
                IndexWriter.Write((long)0);
                IndexWriter.Write(count);
                IndexWriter.Write((long)0);
                IndexWriter.Write(0x10);
                IndexWriter.Write(new byte[24]);        // Skip time
                IndexWriter.Write(ArcEncoding.Shift_JIS.GetBytes(Path.GetFileName(dir)));
                IndexWriter.Write('\0');

                foreach (string file in files)
                {
                    WriteFile(file);
                }
                foreach (string subdir in dirs)
                {
                    WriteDir(subdir);
                }
            }

            private void WriteFile(string file)
            {
                byte[] raw = File.ReadAllBytes(file);
                byte[] data = DeflateHelper.Compress(raw);
                IndexWriter.Write(Offset);
                IndexWriter.Write((long)raw.Length);
                IndexWriter.Write((long)data.Length);
                IndexWriter.Write(0);
                IndexWriter.Write(new byte[24]);        // Skip time
                IndexWriter.Write(ArcEncoding.Shift_JIS.GetBytes(Path.GetFileName(file)));
                IndexWriter.Write('\0');

                DataWriter.Write(data);
                Offset += data.Length;

                raw = null;
                data = null;
            }

            public void Dispose()
            {
                IndexWriter.Dispose();
                DataWriter.Dispose();
                Index.Dispose();
            }
        }
    }
}
