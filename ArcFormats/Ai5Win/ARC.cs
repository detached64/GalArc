using Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utility;
using Utility.Compression;

namespace ArcFormats.Ai5Win
{
    internal class Entry
    {
        internal string name;
        internal string filePath;
        internal uint size;
        internal uint offset;
    }
    public class ARC
    {
        private static int[] NameLengths = { 0x14, 0x18, 0x1E, 0x20, 0x100 };

        private static string[] ExtNeedLzss = { "mes", "lib", "a", "a6", "msk", "x" };

        private static string FolderPath;

        public static void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            ArcScheme scheme = new ArcScheme();
            FolderPath = folderPath;
            int fileCount = br.ReadInt32();
            LogUtility.InitBar(fileCount);
            Directory.CreateDirectory(folderPath);

            TryReadIndex(br, fileCount, out List<Entry> entries);

            foreach (Entry entry in entries)
            {
                fs.Position = entry.offset;
                byte[] data = br.ReadBytes((int)entry.size);
                if (ExtNeedLzss.Contains(Path.GetExtension(entry.filePath).TrimStart('.').ToLower()))
                {
                    File.WriteAllBytes(entry.filePath, Lzss.Decompress(data));
                }
                else
                {
                    File.WriteAllBytes(entry.filePath, data);
                }
                LogUtility.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        private static void TryReadIndex(BinaryReader br, int fileCount, out List<Entry> entries)
        {
            int lengthIndex = 0;
            entries = new List<Entry>();
            while (lengthIndex < 3)
            {
                try
                {
                    entries.Clear();
                    ArcScheme scheme = new ArcScheme();
                    scheme.GuessScheme(br, fileCount, NameLengths[lengthIndex]);
                    br.BaseStream.Position = 4;
                    for (int i = 0; i < fileCount; i++)
                    {
                        Entry entry = new Entry();
                        entry.name = ArcEncoding.Shift_JIS.GetString(Xor.xor(br.ReadBytes(NameLengths[lengthIndex]), scheme.nameKey)).TrimEnd('\0');
                        entry.filePath = Path.Combine(FolderPath, entry.name);
                        entry.size = br.ReadUInt32() ^ scheme.sizeKey;
                        entry.offset = br.ReadUInt32() ^ scheme.offsetKey;
                        entries.Add(entry);
                    }
                    return;
                }
                catch
                {
                    lengthIndex++;
                }
            }
            LogUtility.Error("Failed to read index.");
        }
    }

    internal class ArcScheme
    {
        internal byte nameKey;
        internal uint sizeKey;
        internal uint offsetKey;


        internal void GuessScheme(BinaryReader br, int fileCount, int nameLen)
        {
            // guess name key
            br.BaseStream.Position = nameLen + 3;    // last byte of name , hopefully 0x00
            nameKey = br.ReadByte();
            // guess offset key
            uint dataOffset = (uint)((nameLen + 8) * fileCount + 4);
            uint size = br.ReadUInt32();
            uint offset1 = br.ReadUInt32();
            offsetKey = offset1 ^ dataOffset;
            // guess size key
            br.BaseStream.Position += nameLen + 4;
            uint offset2 = br.ReadUInt32() ^ offsetKey;
            sizeKey = size ^ (offset2 - dataOffset);
        }
    }
}