using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GalArc.Models.Formats.AiWin
{
    internal class ARC : ArcFormat
    {
        public override string Name => "ARC";
        public override string Description => "AiWin Engine Archive";
        public override bool CanWrite => true;

        public override void Unpack(string filePath, string folderPath)
        {
            using FileStream fs = File.OpenRead(filePath);
            using BinaryReader br = new(fs);

            int fileCount = br.ReadInt32();
            List<Entry> entries = new(fileCount);

            ProgressManager.SetMax(fileCount);

            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new()
                {
                    Name = Encoding.ASCII.GetString(br.ReadBytes(0x10)).TrimEnd('\0'),
                    Offset = br.ReadUInt32(),
                    Size = br.ReadUInt32()
                };
                entries.Add(entry);
            }

            foreach (Entry entry in entries)
            {
                fs.Seek(entry.Offset, SeekOrigin.Begin);
                byte[] data = br.ReadBytes((int)entry.Size);

                if (entry.Name.EndsWith(".mes", StringComparison.OrdinalIgnoreCase))
                {
                    using MemoryStream ms = new(data);
                    data = DecompressLzss(ms);
                }

                string outPath = Path.Combine(folderPath, entry.Name);
                string? outDir = Path.GetDirectoryName(outPath);

                if (!string.IsNullOrEmpty(outDir) && !Directory.Exists(outDir))
                    Directory.CreateDirectory(outDir);

                File.WriteAllBytes(outPath, data);
                ProgressManager.Progress();
            }
        }

        public override void Pack(string folderPath, string filePath)
        {
            var files = new DirectoryInfo(folderPath).GetFiles("*", SearchOption.TopDirectoryOnly);
            ProgressManager.SetMax(files.Length);

            List<Entry> entries = new();
            uint headerSize = (uint)(4 + files.Length * (0x10 + 4 + 4));
            uint dataOffset = headerSize;

            foreach (FileInfo file in files)
            {
                byte[] fileData = File.ReadAllBytes(file.FullName);
                uint entrySize;

                if (file.Name.EndsWith(".mes", StringComparison.OrdinalIgnoreCase))
                {
                    using MemoryStream ms = new(fileData);
                    byte[] packedData = CompressLzss(ms);
                    entrySize = (uint)packedData.Length;
                }
                else
                {
                    entrySize = (uint)fileData.Length;
                }

                entries.Add(new Entry
                {
                    Name = file.Name,
                    Offset = dataOffset,
                    Size = entrySize
                });

                dataOffset += entrySize;
            }

            using FileStream fs = File.Create(filePath);
            using BinaryWriter bw = new(fs);

            bw.Write(entries.Count);

            foreach (var entry in entries)
            {
                bw.WritePaddedString(entry.Name, 0x10);
                bw.Write(entry.Offset);
                bw.Write(entry.Size);
            }

            foreach (var entry in entries)
            {
                string fileFullPath = Path.Combine(folderPath, entry.Name);
                byte[] data = File.ReadAllBytes(fileFullPath);

                if (entry.Name.EndsWith(".mes", StringComparison.OrdinalIgnoreCase))
                {
                    using MemoryStream ms = new(data);
                    data = CompressLzss(ms);
                }

                bw.Write(data);
                ProgressManager.Progress();
            }
        }

        public static byte[] DecompressLzss(Stream input)
        {
            const int N = 4096;
            const int Nmask = 0xFFF;
            byte[] ring = new byte[N];
            int r = 1;

            using MemoryStream output = new();
            using BitStream bitStream = new(input, BitStreamEndianness.Msb, BitStreamMode.Read);

            while (true)
            {
                if (bitStream.ReadBit() == 1)
                {
                    int b = bitStream.ReadBits(8);
                    output.WriteByte((byte)b);
                    ring[r] = (byte)b;
                    r = (r + 1) & Nmask;
                }
                else
                {
                    int offset = bitStream.ReadBits(12);
                    if (offset == 0)
                        break;

                    int length = bitStream.ReadBits(4) + 2;
                    int src = offset;

                    for (int i = 0; i < length; i++)
                    {
                        byte b = ring[src];
                        output.WriteByte(b);
                        ring[r] = b;
                        r = (r + 1) & Nmask;
                        src = (src + 1) & Nmask;
                    }
                }
            }

            return output.ToArray();
        }

        public static byte[] CompressLzss(Stream input)
        {
            const int N = 4096;
            const int Nmask = 0xFFF;
            const int MinMatch = 2;
            const int MaxMatch = 17;

            byte[] data;
            using (MemoryStream ms = new())
            {
                input.CopyTo(ms);
                data = ms.ToArray();
            }

            if (data == null || data.Length == 0)
                return Array.Empty<byte>();

            using MemoryStream output = new();
            using BitStream bitStream = new(output, BitStreamEndianness.Msb, BitStreamMode.Write);

            byte[] ring = new byte[N];
            int r = 1;
            int pos = 0;

            var table = new Dictionary<int, List<int>>(N);

            while (pos < data.Length)
            {
                int bestLength = 0;
                int bestOffsetRingIndex = 0;

                if (pos + 2 < data.Length)
                {
                    int hash = (data[pos] << 16) ^ (data[pos + 1] << 8) ^ data[pos + 2];

                    if (table.TryGetValue(hash, out var list))
                    {
                        for (int i = list.Count - 1; i >= 0; i--)
                        {
                            int prevPos = list[i];
                            int dist = pos - prevPos;
                            if (dist <= 0 || dist >= N)
                                continue;

                            int matchLen = 0;
                            while (matchLen < MaxMatch &&
                                   pos + matchLen < data.Length &&
                                   data[prevPos + matchLen] == data[pos + matchLen])
                            {
                                matchLen++;
                            }

                            if (matchLen >= MinMatch && matchLen > bestLength)
                            {
                                int tempOffset = (r - dist) & Nmask;

                                if (tempOffset != 0)
                                {
                                    bestLength = matchLen;
                                    bestOffsetRingIndex = tempOffset;

                                    if (bestLength == MaxMatch)
                                        break;
                                }
                            }
                        }
                    }

                    if (!table.TryGetValue(hash, out var positions))
                    {
                        positions = new List<int>(4);
                        table[hash] = positions;
                    }
                    positions.Add(pos);
                    if (positions.Count > 64)
                        positions.RemoveAt(0);
                }

                if (bestLength >= MinMatch)
                {
                    bitStream.WriteBit(0);
                    bitStream.WriteBits(new byte[] { (byte)(bestOffsetRingIndex >> 4) }, 8);
                    bitStream.WriteBits(new byte[] { (byte)(((bestOffsetRingIndex & 0xF) << 4) | (bestLength - 2)) }, 8);

                    for (int i = 0; i < bestLength; i++)
                    {
                        ring[r] = data[pos + i];
                        r = (r + 1) & Nmask;
                    }
                    pos += bestLength;
                }
                else
                {
                    bitStream.WriteBit(1);
                    bitStream.WriteBits(new byte[] { data[pos] }, 8);

                    ring[r] = data[pos];
                    r = (r + 1) & Nmask;
                    pos++;
                }
            }

            bitStream.WriteBit(0);
            bitStream.WriteBits(new byte[] { 0, 0 }, 12);

            return output.ToArray();
        }
    }
}
