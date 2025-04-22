using GalArc.Logs;
using GalArc.Strings;
using GalArc.Templates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility.Compression;
using Utility.Exceptions;
using Utility.Extensions;

namespace ArcFormats.NeXAS
{
    public class PAC : ArcFormat
    {
        public override WidgetTemplate PackWidget => PackPACWidget.Instance;

        private NeXASOptions Options => PackPACWidget.Instance.Options;

        private readonly EncodingSetting PacEncoding = new EncodingSetting("NexasPacEncoding");

        public override IEnumerable<ArcSetting> Settings => new[] { PacEncoding };

        internal enum Method
        {
            None,
            Lzss,
            Huffman,
            Zlib,
            Zlib1,
            Zlib2,
            Zstd = 7
        }

        private const string Magic = "PAC";

        private readonly string[] NoComprExts = { "png", "fnt", "ogg" };

        private int FileCount;
        private string FolderPath;
        private BinaryReader Reader;

        internal Method SelectedMethod { private get; set; }

        public override void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            Reader = new BinaryReader(fs);
            FolderPath = folderPath;
            if (Encoding.ASCII.GetString(Reader.ReadBytes(3)) != Magic)
            {
                throw new InvalidArchiveException();
            }
            fs.Position++;
            FileCount = Reader.ReadInt32();

            int methodMagic = Reader.ReadInt32();
            Method method = (Method)methodMagic;
            if (method == Method.Zlib1 || method == Method.Zlib2)
            {
                method = Method.Zlib;
            }
            Logger.Info(string.Format(LogStrings.ArcComprMethod, $"{methodMagic}-{method}"));

            List<PackedEntry> entries = TryReadIndex();

            Directory.CreateDirectory(folderPath);
            Logger.InitBar(FileCount);
            foreach (PackedEntry entry in entries)
            {
                fs.Seek(entry.Offset, SeekOrigin.Begin);
                byte[] fileData = Reader.ReadBytes((int)entry.Size);

                if (entry.UnpackedSize != entry.Size && method != Method.None && Enum.IsDefined(typeof(Method), method)) // compressed
                {
                    Logger.Debug(string.Format(LogStrings.TryDecompressWithMethod, entry.Name, method.ToString()));
                    try
                    {
                        switch (method)
                        {
                            case Method.Huffman:
                                File.WriteAllBytes(entry.Path, Huffman.Decompress(fileData, (int)entry.UnpackedSize));
                                break;

                            case Method.Lzss:
                                File.WriteAllBytes(entry.Path, LzssHelper.Decompress(fileData));
                                break;

                            case Method.Zlib:
                                File.WriteAllBytes(entry.Path, ZlibHelper.Decompress(fileData));
                                break;

                            case Method.Zstd:
                                File.WriteAllBytes(entry.Path, Zstd.Decompress(fileData));
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(LogStrings.ErrorDecompressFailed);
                        Logger.Debug(ex.Message);
                    }
                }
                else    // No compression or unknown method
                {
                    File.WriteAllBytes(entry.Path, fileData);
                }
                fileData = null;
                Logger.UpdateBar();
            }
            fs.Dispose();
            Reader.Dispose();
        }

        private List<PackedEntry> TryReadIndex()
        {
            Encoding encoding = PacEncoding.Get<Encoding>();
            try
            {
                return ReadNewIndex(encoding);
            }
            catch
            {
                try
                {
                    return ReadOldIndex(encoding);
                }
                catch
                {
                    throw new Exception(LogStrings.ErrorContainsInvalid);
                }
            }
        }

        private List<PackedEntry> ReadOldIndex(Encoding encoding)
        {
            List<PackedEntry> entries = new List<PackedEntry>();
            Reader.BaseStream.Position = 12;
            for (int i = 0; i < FileCount; i++)
            {
                PackedEntry entry = new PackedEntry();
                entry.Name = encoding.GetString(Reader.ReadBytes(64)).TrimEnd('\0');
                entry.Path = Path.Combine(FolderPath, entry.Name);
                entry.Offset = Reader.ReadUInt32();
                entry.UnpackedSize = Reader.ReadUInt32();
                entry.Size = Reader.ReadUInt32();
                entries.Add(entry);
            }
            Logger.Info(string.Format(LogStrings.ArcVersion, "1"));
            return entries;
        }

        private List<PackedEntry> ReadNewIndex(Encoding encoding)
        {
            List<PackedEntry> entries = new List<PackedEntry>();
            Reader.BaseStream.Seek(-4, SeekOrigin.End);
            int packedLen = Reader.ReadInt32();
            int unpackedLen = FileCount * 76;
            Reader.BaseStream.Seek(-4 - packedLen, SeekOrigin.End);
            byte[] packedIndex = Reader.ReadBytes(packedLen);
            for (int i = 0; i < packedLen; i++)
            {
                packedIndex[i] ^= 0xff;
            }
            byte[] index = Huffman.Decompress(packedIndex, unpackedLen);
            using (MemoryStream msIndex = new MemoryStream(index))
            {
                using (BinaryReader ReaderIndex = new BinaryReader(msIndex))
                {
                    for (int i = 0; i < FileCount; i++)
                    {
                        PackedEntry entry = new PackedEntry();
                        entry.Name = encoding.GetString(ReaderIndex.ReadBytes(64)).TrimEnd('\0');
                        entry.Path = Path.Combine(FolderPath, entry.Name);
                        entry.Offset = ReaderIndex.ReadUInt32();
                        entry.UnpackedSize = ReaderIndex.ReadUInt32();
                        entry.Size = ReaderIndex.ReadUInt32();
                        entries.Add(entry);
                    }
                }
            }
            Logger.Info(string.Format(LogStrings.ArcVersion, "2"));
            return entries;
        }

        public override void Pack(string folderPath, string filePath)
        {
            string[] files = Directory.GetFiles(folderPath);
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            bw.Write(Encoding.ASCII.GetBytes(Magic));
            bw.Write('\0');
            bw.Write((uint)files.Length);
            bw.Write(Options.Methods[Options.Method][0] - '0');
            int indexLength = 76 * files.Length;
            uint offset = (uint)fw.Position;
            if (Options.Version == "1")
            {
                bw.BaseStream.Position += indexLength;  // Reserve space for index
            }
            Logger.InitBar(files.Length, 2);

            List<PackedEntry> entries = new List<PackedEntry>();
            foreach (string file in files)
            {
                PackedEntry entry = new PackedEntry();
                entry.Name = Path.GetFileName(file);
                entry.Offset = offset;
                byte[] data = File.ReadAllBytes(file);
                entry.UnpackedSize = (uint)data.Length;
                if (!entry.Name.HasAnyOfExtensions(NoComprExts))
                {
                    entry.UnpackedSize = (uint)data.Length;
                    switch (Options.Method)
                    {
                        case 1:
                            data = LzssHelper.Compress(data);
                            break;
                        case 2:
                            data = Huffman.Compress(data);
                            break;
                        case 3:
                        case 4:
                        case 5:
                            data = ZlibHelper.Compress(data);
                            break;
                        case 6:
                            data = Zstd.Compress(data);
                            break;
                    }
                }
                bw.Write(data);
                entry.Size = (uint)data.Length;
                offset += entry.Size;
                entries.Add(entry);
                data = null;
                Logger.UpdateBar();
            }

            Encoding encoding = PacEncoding.Get<Encoding>();
            using (MemoryStream index = new MemoryStream())
            {
                using (BinaryWriter indexWriter = new BinaryWriter(index))
                {
                    foreach (PackedEntry entry in entries)
                    {
                        indexWriter.WritePaddedString(entry.Name, 64, '\0', encoding);
                        indexWriter.Write(entry.Offset);
                        indexWriter.Write(entry.UnpackedSize);
                        indexWriter.Write(entry.Size);
                    }
                }

                byte[] raw = index.ToArray();
                if (Options.Version == "2")
                {
                    byte[] packed = Huffman.Compress(raw);
                    for (int i = 0; i < packed.Length; i++)
                    {
                        packed[i] ^= 0xff;
                    }
                    bw.Write(packed);
                    bw.Write(packed.Length);
                }
                else
                {
                    bw.BaseStream.Position = 12;
                    bw.Write(raw);
                }
                Logger.UpdateBar();
            }

            bw.Dispose();
            fw.Dispose();
        }
    }
}