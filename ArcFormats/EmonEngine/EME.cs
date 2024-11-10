using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility;
using Utility.Compression;

namespace ArcFormats.EmonEngine
{
    internal class EME
    {
        private static readonly string Magic = "RREDATA ";

        public string GetNullTerminatedString(byte[] data, int offset, int maxLength)
        {
            int end = Array.IndexOf(data, (byte)0, offset, maxLength);
            if (end == -1)
                end = offset + maxLength;

            return ArcEncoding.Shift_JIS.GetString(data, offset, end - offset); //Using sjis just to be safe. Otherwise ascii can do fine as well.
        }
        public class Entry
        {
            public string Name { get; set; }
            public uint Offset { get; set; }
            public uint PackedSize { get; set; }
            public uint UnpackedSize { get; set; }
            public int LzssFrameSize { get; set; }
            public int LzssInitPos { get; set; }
            public int SubType { get; set; }
            public bool IsPacked { get; set; }
        }
        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);

            if (Encoding.ASCII.GetString(br.ReadBytes(8)) != Magic)
            {
                throw new InvalidDataException("Not a Valid EME Archive");
            }

            fs.Position = fs.Length - 4;
            int fileCount = br.ReadInt32();

            if (fileCount < 0 || fileCount > 100000)
            {
                throw new InvalidDataException("Invalid File Count");
            }

            uint indexSize = (uint)fileCount * 0x60;
            var indexOffset = fs.Length - 4 - indexSize;
            fs.Position = indexOffset - 40;
            var key = br.ReadBytes(40);

            fs.Position = indexOffset;
            var index = br.ReadBytes((int)indexSize);

            int currentOffset = 0;
            var entries = new List<Entry>(fileCount);

            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new Entry();
                DECRYPT.Decrypt(index, currentOffset, 0x60, key);

                // Bounds check before reading the name to prevent reading beyond buffer PackedSize
                if (currentOffset + 0x60 > index.Length)
                {
                    throw new InvalidDataException("Index entry exceeds buffer PackedSize.");
                }
                entry.Name = GetNullTerminatedString(index, currentOffset, 0x40);

                entry.LzssFrameSize = BitConverter.ToUInt16(index, currentOffset + 0x40);
                entry.LzssInitPos = BitConverter.ToUInt16(index, currentOffset + 0x42);

                if (entry.LzssFrameSize != 0)
                    entry.LzssInitPos = (entry.LzssFrameSize - entry.LzssInitPos) % entry.LzssFrameSize;

                entry.SubType = BitConverter.ToUInt16(index, currentOffset + 0x48);  // Adjusted offset for `SubType`
                entry.PackedSize = BitConverter.ToUInt32(index, currentOffset + 0x4C);
                entry.UnpackedSize = BitConverter.ToUInt32(index, currentOffset + 0x50);
                entry.Offset = BitConverter.ToUInt32(index, currentOffset + 0x54);
                entry.IsPacked = entry.UnpackedSize != entry.PackedSize;
                entries.Add(entry);
                currentOffset += 0x60;
            }
            foreach (var entry in entries)
            {
                Logger.Debug(entry.Name);
            }
            Logger.InitBar(fileCount);
            Directory.CreateDirectory(folderPath);

            foreach (var entry in entries)
            {
                fs.Position = entry.Offset;
                var header = br.ReadBytes(12);
                DECRYPT.Decrypt(header, 0, 12, key);

                if (0 == entry.LzssFrameSize)
                {
                    byte[] data = br.ReadBytes((int)entry.PackedSize);
                    string fileName = Path.Combine(folderPath, entry.Name);
                    File.WriteAllBytes(fileName, data);
                    Logger.UpdateBar();
                }
                int part2unpackedSize = BitConverter.ToInt32(header, 4);
                if (0 != part2unpackedSize && part2unpackedSize < entry.PackedSize)
                {
                    uint packedSize = BitConverter.ToUInt32(header, 0);
                    string fileName = Path.Combine(folderPath, entry.Name);
                    br.BaseStream.Seek(entry.Offset + 12, SeekOrigin.Begin);
                    byte[] part2data = br.ReadBytes((int)packedSize);
                    part2data = Lzss.Decompress(part2data);
                    br.BaseStream.Seek(entry.Offset + 12 + packedSize, SeekOrigin.Begin);
                    int part1UnpackedSize = (int)entry.PackedSize;
                    byte[] part1data = br.ReadBytes(part1UnpackedSize);
                    part1data = Lzss.Decompress(part1data);

                    //Combine part1data and part2data
                    byte[] combinedData = new byte[part1data.Length + part2data.Length];
                    Array.Copy(part1data, 0, combinedData, 0, part1data.Length);
                    Array.Copy(part2data, 0, combinedData, part1data.Length, part2data.Length);

                    File.WriteAllBytes(fileName, combinedData);
                }

                else
                {
                    fs.Position = entry.Offset + 12;
                    byte[] data = br.ReadBytes((int)entry.PackedSize);
                    data = Lzss.Decompress(data);
                    string fileName = Path.Combine(folderPath, entry.Name);
                    File.WriteAllBytes(fileName, data);
                    Logger.UpdateBar();
                    data = null;
                }
            }

            fs.Dispose();
            br.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            // Track file entries for better organization and accuracy
            var fileEntries = new List<(
                string Name,
                long UnpackedSize,
                long PackedSize,
                long Offset,
                byte[] CompressedData,
                byte[] HeaderData
            )>();

            // Get all files and prepare total size estimate
            FileInfo[] files = new DirectoryInfo(folderPath).GetFiles();
            Logger.InitBar(files.Length);

            using (FileStream fw = File.OpenWrite(filePath))
            using (BinaryWriter bw = new BinaryWriter(fw))
            {
                // Write file count at the end
                fw.Position = 0;
                // Write magic bytes
                bw.Write(Encoding.ASCII.GetBytes(Magic), 0, Magic.Length);
                // Start offset after magic bytes
                long currentOffset = Magic.Length;

                var key = Utils.HexStringToByteArray("0104020800000000f962a8ec11000000f8e296ca0700000000000000000000000000000000000000");

                // First pass: Process and store all file data
                foreach (FileInfo file in files)
                {
                    // Prepare and encrypt header
                    var header = new byte[12];
                    BitConverter.GetBytes(0).CopyTo(header, 0);
                    BitConverter.GetBytes(0).CopyTo(header, 4);
                    BitConverter.GetBytes(0).CopyTo(header, 8);

                    var encryptedHeader = ENCRYPT.Encrypt(header, 0, header.Length, key);
                    var finalHeader = ENCRYPT.ApplyHeaderXorMask(encryptedHeader);

                    // Read and compress file data
                    byte[] originalData = File.ReadAllBytes(file.FullName);
                    byte[] compressedData = Lzss.Compress(originalData);

                    // Store file information with actual compressed data
                    fileEntries.Add((
                        Name: file.Name,
                        UnpackedSize: originalData.Length,
                        PackedSize: compressedData.Length,
                        Offset: currentOffset,
                        CompressedData: compressedData,
                        HeaderData: finalHeader
                    ));

                    // Write header and compressed data
                    bw.Write(finalHeader);
                    bw.Write(compressedData);

                    // Update offset for next file
                    currentOffset += finalHeader.Length + compressedData.Length;

                    Logger.UpdateBar();
                }

                // Write encryption key
                bw.Write(key);

                // Write file entries
                foreach (var entry in fileEntries)
                {
                    var ms = new MemoryStream();
                    var headerWriter = new BinaryWriter(ms);
                    // File name (padded to 0x40 bytes)
                    byte[] name = Utils.PaddedBytes(entry.Name, 0x40);
                    headerWriter.Write(name);

                    // Standard header values
                    headerWriter.Write((ushort)4096);  // LZSS frame size is same for all.
                    headerWriter.Write((ushort)18);    // LZSS position is same for all.
                    headerWriter.Write((uint)1);       // Version flag for script only.
                    headerWriter.Write((uint)3);       // SubType for Script files.

                    // Size information
                    headerWriter.Write((uint)entry.PackedSize);  // Original size
                    headerWriter.Write((uint)entry.UnpackedSize);    // Compressed size
                    headerWriter.Write((uint)entry.Offset);        // Data offset in file

                    // Reserved fields
                    headerWriter.Write((uint)0);
                    headerWriter.Write((uint)0);
                    byte[] headerBytes = ms.ToArray();
                    var encryptedHeader = ENCRYPT.Encrypt(headerBytes, 0, headerBytes.Length, key);
                    var finalHeader = ENCRYPT.ApplyXorMask(encryptedHeader);
                    bw.Write(finalHeader);
                    ms.Dispose();

                }
                bw.Write((uint)0);
                fw.Position = fw.Length - 4;
                bw.Write(files.Length);
                bw.Dispose();
                fw.Dispose();
            }

        }

    }
}

