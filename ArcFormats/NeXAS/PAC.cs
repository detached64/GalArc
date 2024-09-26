using Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utility;
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
            Zstd = 7
        }

        private struct Entry
        {
            public string FileName;
            public string FilePath;
            public int Offset;
            public int UnpackedSize;
            public int PackedSize;
        }
        private static string FolderPath = string.Empty;

        private static Encoding encodings = Encoding.UTF8;

        private static List<Entry> entries = new List<Entry>();

        private static byte[] signature = { 0x50, 0x41, 0x43 };       //"PAC"

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader reader = new BinaryReader(fs);
            FolderPath = folderPath;
            encodings = Global.Encoding;
            if (!reader.ReadBytes(3).SequenceEqual(signature))
            {
                LogUtility.Error_NotValidArchive();
            }
            reader.ReadByte();
            long fileSize = reader.BaseStream.Length;
            int fileCount = reader.ReadInt32();
            int methodCount = reader.ReadInt32();
            switch (methodCount)
            {
                case 4:
                case 5:
                    methodCount = 3;
                    break;
            }
            Method method = (Method)methodCount;
            Directory.CreateDirectory(folderPath);
            LogUtility.InitBar(fileCount);

            ReadOldIndex(reader, fileCount, out bool isSuccess);
            if (!isSuccess)
            {
                entries.Clear();
                ReadNewIndex(reader, fileCount, out isSuccess);
            }
            if (!isSuccess)
            {
                LogUtility.Error("Failed to read index.");
                return;
            }

            for (int i = 0; i < fileCount; i++)
            {
                fs.Seek(entries[i].Offset, SeekOrigin.Begin);
                byte[] fileData = reader.ReadBytes(entries[i].PackedSize);

                if (entries[i].UnpackedSize != entries[i].PackedSize && method != Method.None && Enum.IsDefined(typeof(Method), method)) // compressed
                {
                    LogUtility.Debug("Packed file detected:" + entries[i].FileName + ".Try " + method.ToString() + "……");
                    try
                    {
                        switch (method)
                        {
                            case Method.Huffman:
                                File.WriteAllBytes(entries[i].FilePath, Huffman.Decompress(fileData, entries[i].UnpackedSize));
                                break;

                            case Method.Lzss:
                                File.WriteAllBytes(entries[i].FilePath, Lzss.Decompress(fileData));
                                break;

                            case Method.Zlib:
                                File.WriteAllBytes(entries[i].FilePath, Zlib.DecompressBytes(fileData));
                                break;

                            case Method.Zstd:
                                File.WriteAllBytes(entries[i].FilePath, Zstd.Decompress(fileData));
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogUtility.Error("Decompress " + method.ToString() + " failed:" + entries[i].FileName, false);
                        LogUtility.Debug(ex.Message);
                    }
                }
                else                                                    // No compression or unknown method
                {
                    File.WriteAllBytes(entries[i].FilePath, fileData);
                }
                LogUtility.UpdateBar();
            }
            entries.Clear();
            fs.Dispose();
            reader.Dispose();
        }

        private static void ReadOldIndex(BinaryReader reader, int fileCount, out bool isSuccess)
        {
            reader.BaseStream.Position = 12;
            try
            {
                for (int i = 0; i < fileCount; i++)
                {
                    Entry entry = new Entry();
                    entry.FileName = encodings.GetString(reader.ReadBytes(64)).TrimEnd('\0');
                    entry.FilePath = Path.Combine(FolderPath, entry.FileName);
                    entry.Offset = reader.ReadInt32();
                    entry.UnpackedSize = reader.ReadInt32();
                    entry.PackedSize = reader.ReadInt32();
                    entries.Add(entry);
                }
            }
            catch
            {
                isSuccess = false;
                return;
            }
            isSuccess = true;
        }

        private static void ReadNewIndex(BinaryReader reader, int fileCount, out bool isSuccess)
        {
            reader.BaseStream.Seek(-4, SeekOrigin.End);
            int packedLen = reader.ReadInt32();
            int unpackedLen = fileCount * 76;
            reader.BaseStream.Seek(-4 - packedLen, SeekOrigin.End);
            byte[] packedIndex = Xor.xor(reader.ReadBytes(packedLen), 0xff);
            byte[] index = Huffman.Decompress(packedIndex, unpackedLen);
            MemoryStream msIndex = new MemoryStream(index);
            BinaryReader readerIndex = new BinaryReader(msIndex);
            try
            {
                for (int i = 0; i < fileCount; i++)
                {
                    Entry entry = new Entry();
                    entry.FileName = encodings.GetString(readerIndex.ReadBytes(64)).TrimEnd('\0');
                    entry.FilePath = Path.Combine(FolderPath, entry.FileName);
                    entry.Offset = readerIndex.ReadInt32();
                    entry.UnpackedSize = readerIndex.ReadInt32();
                    entry.PackedSize = readerIndex.ReadInt32();
                    entries.Add(entry);
                }
            }
            catch
            {
                isSuccess = false;
                return;
            }
            isSuccess = true;
        }
    }
}