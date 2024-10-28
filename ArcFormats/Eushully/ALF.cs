using Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility.Compression;

namespace ArcFormats.Eushully
{
    public class ALF
    {
        private static readonly string IndexFileNameV5 = "SYS5INI.BIN";

        private static readonly string IndexFileMagicV5 = "S5IC502 ";

        private class Entry
        {
            public uint Offset { get; set; }
            public uint Size { get; set; }
            public string Name { get; set; }
            public int ArchiveIndex { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            string indexPath = Path.Combine(Path.GetDirectoryName(filePath), IndexFileNameV5);
            if (!File.Exists(indexPath))
            {
                LogUtility.ErrorNeedAnotherFile(IndexFileNameV5);
            }
            byte[] index;

            using (FileStream fs = File.OpenRead(indexPath))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    if (Encoding.Unicode.GetString(br.ReadBytes(16)) != IndexFileMagicV5)
                    {
                        LogUtility.ErrorInvalidArchive();
                    }
                    fs.Position = 552;
                    byte[] raw = br.ReadBytes((int)fs.Length - 532);
                    index = Lzss.Decompress(raw);
                    raw = null;
                }
            }
            List<string> archives = new List<string>();
            int archiveIndex;
            List<Entry> entries = new List<Entry>();

            using (MemoryStream ms = new MemoryStream(index))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    int pos = 4;
                    // read archive info
                    int archiveCount = br.ReadInt32();
                    for (int i = 0; i < archiveCount; i++)
                    {
                        ms.Position = pos;
                        string name = Encoding.Unicode.GetString(br.ReadBytes(24)).TrimEnd('\0');
                        archives.Add(name);
                        pos += 512;
                    }
                    archiveIndex = archives.IndexOf(Path.GetFileName(filePath));
                    if (archiveIndex == -1)
                    {
                        LogUtility.ErrorInvalidArchive();
                    }
                    else
                    {
                        ms.Position = pos;
                        int fileCount = br.ReadInt32();
                        for (int i = 0; i < fileCount; i++)
                        {
                            Entry entry = new Entry();
                            entry.Name = Encoding.Unicode.GetString(br.ReadBytes(128)).TrimEnd('\0');
                            entry.ArchiveIndex = br.ReadInt32();
                            if (entry.ArchiveIndex != archiveIndex)
                            {
                                ms.Position += 12;
                                continue;
                            }
                            ms.Position += 4;
                            entry.Offset = br.ReadUInt32();
                            entry.Size = br.ReadUInt32();
                            entries.Add(entry);
                        }
                    }
                }
            }
            LogUtility.InitBar(entries.Count);
            Directory.CreateDirectory(folderPath);
            using (FileStream fs = File.OpenRead(filePath))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    foreach (Entry entry in entries)
                    {
                        fs.Position = entry.Offset;
                        byte[] buffer = br.ReadBytes((int)entry.Size);
                        File.WriteAllBytes(Path.Combine(folderPath, entry.Name), buffer);
                        buffer = null;
                        LogUtility.UpdateBar();
                    }
                }
            }
            index = null;
        }
    }
}
