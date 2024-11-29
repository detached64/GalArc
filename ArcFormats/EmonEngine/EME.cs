using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility.Compression;
using Utility.Extensions;

namespace ArcFormats.EmonEngine
{
    internal class EME
    {
        private string Magic => "RREDATA ";

        private class Entry
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public uint Offset { get; set; }
            public uint PackedSize { get; set; }
            public uint UnpackedSize { get; set; }
            public int LzssFrameSize { get; set; }
            public int LzssInitPos { get; set; }
            public int SubType { get; set; }
            public uint Magic { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);

            if (Encoding.ASCII.GetString(br.ReadBytes(8)) != Magic)
            {
                Logger.ErrorInvalidArchive();
            }

            fs.Position = fs.Length - 4;
            int fileCount = br.ReadInt32();

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
                EmonUtils.Decrypt(index, currentOffset, 0x60, key);

                // Bounds check before reading the name to prevent reading beyond buffer PackedSize
                if (currentOffset + 0x60 > index.Length)
                {
                    throw new InvalidDataException("Index entry exceeds buffer PackedSize.");
                }
                entry.Name = index.GetCString(currentOffset, 0x40);
                entry.Path = Path.Combine(folderPath, entry.Name);
                entry.LzssFrameSize = BitConverter.ToUInt16(index, currentOffset + 0x40);
                entry.LzssInitPos = BitConverter.ToUInt16(index, currentOffset + 0x42);

                if (entry.LzssFrameSize != 0)
                {
                    entry.LzssInitPos = (entry.LzssFrameSize - entry.LzssInitPos) % entry.LzssFrameSize;
                }

                entry.SubType = BitConverter.ToUInt16(index, currentOffset + 0x48);
                entry.PackedSize = BitConverter.ToUInt32(index, currentOffset + 0x4C);
                entry.UnpackedSize = BitConverter.ToUInt32(index, currentOffset + 0x50);
                entry.Offset = BitConverter.ToUInt32(index, currentOffset + 0x54);
                entries.Add(entry);
                currentOffset += 0x60;
            }

            Logger.InitBar(fileCount);
            Directory.CreateDirectory(folderPath);

            foreach (var entry in entries)
            {
                if (entry.SubType == 3)
                {
                    ExtractScript(br, entry, key);
                }
                else if (entry.SubType == 4)
                {
                    ExtractBMP(br, entry, key);
                }
                else if (entry.SubType == 5 && entry.PackedSize > 4)
                {
                    ExtractType5(br, entry, key);
                }
                else
                {
                    br.BaseStream.Position = entry.Offset;
                    byte[] data = br.ReadBytes((int)entry.PackedSize);
                    File.WriteAllBytes(entry.Path, data);
                    data = null;
                }
                Logger.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        private void ExtractScript(BinaryReader br, Entry entry, byte[] key)
        {
            br.BaseStream.Position = entry.Offset;
            byte[] header = br.ReadBytes(12);
            EmonUtils.Decrypt(header, 0, 12, key);
            if (entry.LzssFrameSize == 0)
            {
                byte[] data = new byte[entry.PackedSize + 12];
                Array.Copy(header, 0, data, 0, 12);
                br.Read(data, 12, (int)entry.PackedSize);
                File.WriteAllBytes(entry.Path, data);
                return;
            }

            int part2unpackedSize = BitConverter.ToInt32(header, 4);
            if (0 != part2unpackedSize && part2unpackedSize < entry.UnpackedSize)
            {
                uint packedSize = BitConverter.ToUInt32(header, 0);

                br.BaseStream.Seek(entry.Offset + 12, SeekOrigin.Begin);
                byte[] part2data = br.ReadBytes((int)packedSize);
                part2data = LzssHelper.Decompress(part2data);

                br.BaseStream.Seek(entry.Offset + 12 + packedSize, SeekOrigin.Begin);
                int part1UnpackedSize = (int)entry.PackedSize;
                byte[] part1data = br.ReadBytes(part1UnpackedSize);
                part1data = LzssHelper.Decompress(part1data);

                //Combine part1data and part2data
                byte[] combinedData = new byte[entry.UnpackedSize];
                Array.Copy(part1data, 0, combinedData, 0, part1data.Length);
                Array.Copy(part2data, 0, combinedData, part1data.Length, entry.UnpackedSize - part1data.Length);

                File.WriteAllBytes(entry.Path, combinedData);
                part1data = null;
                part2data = null;
                combinedData = null;
            }
            else
            {
                br.BaseStream.Position = entry.Offset + 12;
                byte[] data = br.ReadBytes((int)entry.PackedSize);
                data = LzssHelper.Decompress(data);
                File.WriteAllBytes(entry.Path, data);
                data = null;
            }
        }

        private void ExtractBMP(BinaryReader br, Entry entry, byte[] key)
        {
            br.BaseStream.Position = entry.Offset;
            byte[] header = br.ReadBytes(32);
            EmonUtils.Decrypt(header, 0, 32, key);
            uint entrySize = entry.PackedSize + 32;
            int colors = BitConverter.ToUInt16(header, 6);
            if (0 != colors && header[0] != 7)
            {
                entrySize += (uint)Math.Max(colors, 3) * 4;
            }
            br.BaseStream.Position = entry.Offset;
            byte[] data = br.ReadBytes((int)entrySize);
            Array.Copy(header, 0, data, 0, 32);
            File.WriteAllBytes(entry.Path, data);
            data = null;
        }

        private void ExtractType5(BinaryReader br, Entry entry, byte[] key)
        {
            br.BaseStream.Position = entry.Offset;
            byte[] data = br.ReadBytes((int)entry.PackedSize);
            EmonUtils.Decrypt(data, 0, 4, key);
            File.WriteAllBytes(entry.Path, data);
            data = null;
        }

        public void Pack(string folderPath, string filePath)
        {
            // Get all files and prepare total size estimate
            FileInfo[] files = new DirectoryInfo(folderPath).GetFiles();
            Logger.InitBar(files.Length, 2);
            List<Entry> entries = new List<Entry>();
            using (FileStream fw = File.OpenWrite(filePath))
            {
                using (BinaryWriter bw = new BinaryWriter(fw))
                {
                    bw.Write(Encoding.ASCII.GetBytes(Magic));
                    uint currentOffset = 8;

                    foreach (FileInfo file in files)
                    {
                        Entry entry = new Entry();
                        entry.Name = file.Name;
                        entry.Offset = currentOffset;
                        switch (file.Extension.ToLower())
                        {
                            case ".txt":
                                entry.UnpackedSize = (uint)file.Length;
                                entry.SubType = 3;
                                entry.Magic = 1;
                                entry.LzssFrameSize = 0x1000;
                                entry.LzssInitPos = 0x12;
                                bw.Write(0);
                                bw.Write(0);
                                bw.Write(0);
                                byte[] packedTXT = LzssHelper.Compress(File.ReadAllBytes(file.FullName));
                                entry.PackedSize = (uint)packedTXT.Length;
                                bw.Write(packedTXT);
                                packedTXT = null;
                                break;
                            case ".ogg":
                                entry.PackedSize = (uint)file.Length;
                                entry.UnpackedSize = entry.PackedSize;
                                entry.SubType = 0;
                                entry.Magic = 0x20400000u;
                                bw.Write(File.ReadAllBytes(file.FullName));
                                break;
                            case ".bmp":
                                byte[] data = File.ReadAllBytes(file.FullName);
                                entry.PackedSize = (uint)data.Length - 32;
                                int colors = BitConverter.ToUInt16(data, 6);
                                if (data[0] != 7 && colors != 0)
                                {
                                    entry.PackedSize -= (uint)Math.Max(colors, 3) * 4;
                                }
                                entry.UnpackedSize = entry.PackedSize;
                                entry.SubType = 4;
                                entry.Magic = 0x10;
                                entry.LzssFrameSize = 0x1000;
                                entry.LzssInitPos = 0x12;
                                bw.Write(data);
                                break;
                            default:
                                continue;
                        }
                        currentOffset = (uint)fw.Position;
                        entries.Add(entry);
                        Logger.UpdateBar();
                    }

                    // Write encryption key: set to 0x00
                    bw.Write(new byte[40]);

                    foreach (var entry in entries)
                    {
                        bw.WritePaddedString(entry.Name, 0x40);
                        bw.Write((ushort)entry.LzssFrameSize);
                        bw.Write((ushort)entry.LzssInitPos);
                        bw.Write(entry.Magic);
                        bw.Write(entry.SubType);
                        bw.Write(entry.PackedSize);
                        bw.Write(entry.UnpackedSize);
                        bw.Write(entry.Offset);
                        bw.Write(0);
                        bw.Write(0);
                        Logger.UpdateBar();
                    }
                    bw.Write(files.Length);
                    files = null;
                    bw.Dispose();
                    fw.Dispose();
                }
            }
        }

    }
}

