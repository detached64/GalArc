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
    public class PAC
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

        private class Entry
        {
            public string FileName { get; set; }
            public string FilePath { get; set; }
            public int Offset { get; set; }
            public int UnpackedSize { get; set; }
            public int PackedSize { get; set; }
        }

        private static string FolderPath = string.Empty;

        private static Encoding DefaultEncoding = Encoding.UTF8;

        private static readonly List<Entry> entries = new List<Entry>();

        private static readonly byte[] Magic = { 0x50, 0x41, 0x43 };       // "PAC"

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            FolderPath = folderPath;
            DefaultEncoding = Config.Encoding;
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

            Directory.CreateDirectory(folderPath);
            Logger.InitBar(fileCount);

            TryReadIndex(br, fileCount);

            foreach (Entry entry in entries)
            {
                fs.Seek(entry.Offset, SeekOrigin.Begin);
                byte[] fileData = br.ReadBytes(entry.PackedSize);

                if (entry.UnpackedSize != entry.PackedSize && method != Method.None && Enum.IsDefined(typeof(Method), method)) // compressed
                {
                    Logger.Debug(string.Format(Resources.logTryDecompressWithMethod, entry.FileName, method.ToString()));
                    try
                    {
                        switch (method)
                        {
                            case Method.Huffman:
                                File.WriteAllBytes(entry.FilePath, Huffman.Decompress(fileData, entry.UnpackedSize));
                                break;

                            case Method.Lzss:
                                File.WriteAllBytes(entry.FilePath, Lzss.Decompress(fileData));
                                break;

                            case Method.Zlib:
                                File.WriteAllBytes(entry.FilePath, Zlib.DecompressBytes(fileData));
                                break;

                            case Method.Zstd:
                                File.WriteAllBytes(entry.FilePath, Zstd.Decompress(fileData));
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
                    File.WriteAllBytes(entry.FilePath, fileData);
                }
                fileData = null;
                Logger.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        private static void TryReadIndex(BinaryReader reader, int fileCount)
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

        private static void ReadOldIndex(BinaryReader reader, int fileCount)
        {
            reader.BaseStream.Position = 12;
            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new Entry();
                entry.FileName = DefaultEncoding.GetString(reader.ReadBytes(64)).TrimEnd('\0');
                entry.FilePath = Path.Combine(FolderPath, entry.FileName);
                entry.Offset = reader.ReadInt32();
                entry.UnpackedSize = reader.ReadInt32();
                entry.PackedSize = reader.ReadInt32();
                entries.Add(entry);
            }
        }

        private static void ReadNewIndex(BinaryReader reader, int fileCount)
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
            byte[] index = Huffman.Decompress(packedIndex, unpackedLen);
            File.WriteAllBytes("C:\\Users\\refrain69\\Desktop\\1.1", index);
            packedIndex = null;
            MemoryStream msIndex = new MemoryStream(index);
            BinaryReader readerIndex = new BinaryReader(msIndex);
            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new Entry();
                entry.FileName = DefaultEncoding.GetString(readerIndex.ReadBytes(64)).TrimEnd('\0');
                entry.FilePath = Path.Combine(FolderPath, entry.FileName);
                entry.Offset = readerIndex.ReadInt32();
                entry.UnpackedSize = readerIndex.ReadInt32();
                entry.PackedSize = readerIndex.ReadInt32();
                entries.Add(entry);
            }
        }
    }
}