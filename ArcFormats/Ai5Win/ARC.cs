using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using Utility;
using Utility.Compression;
using Utility.Extensions;

namespace ArcFormats.Ai5Win
{
    public class ARC
    {
        private class Entry
        {
            internal string Name { get; set; }
            internal string Path { get; set; }
            internal uint Size { get; set; }
            internal uint Offset { get; set; }
        }

        private readonly static int[] NameLengths = { 0x14, 0x18, 0x1E, 0x20, 0x100 };

        private static string FolderPath;

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);

            FolderPath = folderPath;

            int fileCount = br.ReadInt32();
            Logger.InitBar(fileCount);
            Directory.CreateDirectory(folderPath);

            TryReadIndex(br, fileCount, out List<Entry> entries);

            foreach (Entry entry in entries)
            {
                fs.Position = entry.Offset;
                byte[] data = br.ReadBytes((int)entry.Size);
                if (entry.Path.HasAnyOfExtensions("mes", "lib", "a", "a6", "msk", "x"))
                {
                    File.WriteAllBytes(entry.Path, Lzss.Decompress(data));
                }
                else
                {
                    File.WriteAllBytes(entry.Path, data);
                }
                data = null;
                Logger.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        private static void TryReadIndex(BinaryReader br, int fileCount, out List<Entry> entries)
        {
            entries = new List<Entry>();
            foreach (int nameLength in NameLengths)
            {
                try
                {
                    entries.Clear();
                    ArcScheme scheme = new ArcScheme();
                    scheme.GuessScheme(br, fileCount, nameLength);
                    br.BaseStream.Position = 4;
                    for (int i = 0; i < fileCount; i++)
                    {
                        Entry entry = new Entry();
                        byte[] nameBuf = br.ReadBytes(nameLength);
                        for (int j = 0; j < nameLength; j++)
                        {
                            nameBuf[j] ^= scheme.NameKey;
                        }
                        entry.Name = ArcEncoding.Shift_JIS.GetString(nameBuf).TrimEnd('\0');
                        if (entry.Name.ContainsInvalidChars())
                        {
                            throw new Exception();
                        }
                        entry.Path = Path.Combine(FolderPath, entry.Name);
                        entry.Size = br.ReadUInt32() ^ scheme.SizeKey;
                        entry.Offset = br.ReadUInt32() ^ scheme.OffsetKey;
                        entries.Add(entry);
                    }
                    return;
                }
                catch
                {
                    continue;
                }
            }
            Logger.Error("Failed to read index.");
        }
    }

    internal class ArcScheme
    {
        internal byte NameKey { get; set; }
        internal uint SizeKey { get; set; }
        internal uint OffsetKey { get; set; }

        internal void GuessScheme(BinaryReader br, int fileCount, int nameLen)
        {
            // guess name key
            br.BaseStream.Position = nameLen + 3;    // last byte of name , hopefully 0x00
            NameKey = br.ReadByte();
            // guess offset key
            uint dataOffset = (uint)((nameLen + 8) * fileCount + 4);
            uint size = br.ReadUInt32();
            uint offset1 = br.ReadUInt32();
            OffsetKey = offset1 ^ dataOffset;
            // guess size key
            br.BaseStream.Position += nameLen + 4;
            uint offset2 = br.ReadUInt32() ^ OffsetKey;
            SizeKey = size ^ (offset2 - dataOffset);
        }
    }
}