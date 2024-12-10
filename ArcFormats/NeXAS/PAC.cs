using ArcFormats.Properties;
using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utility.Compression;

namespace ArcFormats.NeXAS
{
    public class PAC : ArchiveFormat
    {
        private enum Method
        {
            None,
            Lzss,
            Huffman,
            Zlib,
            Zlib1,
            Zlib2,
            Zstd = 7
        }

        private string FolderPath;

        private Encoding DefaultEncoding = Encoding.UTF8;

        private List<PackedEntry> entries = new List<PackedEntry>();

        private readonly byte[] Magic = { 0x50, 0x41, 0x43 };       // "PAC"

        public override void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            FolderPath = folderPath;
            DefaultEncoding = ArcSettings.Encoding;
            if (!br.ReadBytes(3).SequenceEqual(Magic))
            {
                Logger.ErrorInvalidArchive();
            }
            fs.Position++;
            long fileSize = br.BaseStream.Length;
            int fileCount = br.ReadInt32();

            Method method = (Method)br.ReadInt32();
            if (method == Method.Zlib1 || method == Method.Zlib2)
            {
                method = Method.Zlib;
            }

            TryReadIndex(br, fileCount);

            Directory.CreateDirectory(folderPath);
            Logger.InitBar(fileCount);
            foreach (PackedEntry entry in entries)
            {
                fs.Seek(entry.Offset, SeekOrigin.Begin);
                byte[] fileData = br.ReadBytes((int)entry.Size);

                if (entry.UnpackedSize != entry.Size && method != Method.None && Enum.IsDefined(typeof(Method), method)) // compressed
                {
                    Logger.Debug(string.Format(Resources.logTryDecompressWithMethod, entry.Name, method.ToString()));
                    try
                    {
                        switch (method)
                        {
                            case Method.Huffman:
                                File.WriteAllBytes(entry.Path, HuffmanDecoder.Decompress(fileData, (int)entry.UnpackedSize));
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
                        Logger.Error(Resources.logErrorDecompressFailed, false);
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
            br.Dispose();
        }

        private void TryReadIndex(BinaryReader reader, int fileCount)
        {
            List<Action> actions = new List<Action>()
            {
                () => ReadNewIndex(reader, fileCount),
                () => ReadOldIndex(reader, fileCount),
            };
            foreach (Action action in actions)
            {
                try
                {
                    entries.Clear();
                    action();
                    return;
                }
                catch
                {
                    continue;
                }
            }
            Logger.Error(Resources.logErrorContainsInvalid);
        }

        private void ReadOldIndex(BinaryReader reader, int fileCount)
        {
            reader.BaseStream.Position = 12;
            for (int i = 0; i < fileCount; i++)
            {
                PackedEntry entry = new PackedEntry();
                entry.Name = this.DefaultEncoding.GetString(reader.ReadBytes(64)).TrimEnd('\0');
                entry.Path = Path.Combine(FolderPath, entry.Name);
                entry.Offset = reader.ReadUInt32();
                entry.UnpackedSize = reader.ReadUInt32();
                entry.Size = reader.ReadUInt32();
                entries.Add(entry);
            }
        }

        private void ReadNewIndex(BinaryReader reader, int fileCount)
        {
            reader.BaseStream.Seek(-4, SeekOrigin.End);
            int packedLen = reader.ReadInt32();
            int unpackedLen = fileCount * 76;
            reader.BaseStream.Seek(-4 - packedLen, SeekOrigin.End);
            byte[] packedIndex = reader.ReadBytes(packedLen);
            for (int i = 0; i < packedLen; i++)
            {
                packedIndex[i] ^= 0xff;
            }
            byte[] index = HuffmanDecoder.Decompress(packedIndex, unpackedLen);
            packedIndex = null;
            using (MemoryStream msIndex = new MemoryStream(index))
            {
                using (BinaryReader readerIndex = new BinaryReader(msIndex))
                {
                    for (int i = 0; i < fileCount; i++)
                    {
                        PackedEntry entry = new PackedEntry();
                        entry.Name = DefaultEncoding.GetString(readerIndex.ReadBytes(64)).TrimEnd('\0');
                        entry.Path = Path.Combine(FolderPath, entry.Name);
                        entry.Offset = readerIndex.ReadUInt32();
                        entry.UnpackedSize = readerIndex.ReadUInt32();
                        entry.Size = readerIndex.ReadUInt32();
                        entries.Add(entry);
                    }
                }
            }
        }
    }
}