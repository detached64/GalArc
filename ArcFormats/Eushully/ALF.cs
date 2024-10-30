using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility.Compression;
using Utility.Extensions;

namespace ArcFormats.Eushully
{
    public class ALF
    {
        private static readonly string IndexFileNameV5 = "SYS5INI.BIN";

        private static readonly string IndexFileMagicV5 = "S5IC502 ";

        private static readonly string IndexFileNameV4 = "sys4ini.bin";

        private static readonly string IndexFileMagicV4 = "S4IC415 ";

        private class Entry
        {
            public uint Offset { get; set; }
            public uint Size { get; set; }
            public string Name { get; set; }
            public int ArchiveIndex { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            string indexPathV4 = Path.Combine(Path.GetDirectoryName(filePath), IndexFileNameV4);
            string indexPathV5 = Path.Combine(Path.GetDirectoryName(filePath), IndexFileNameV5);
            if (File.Exists(indexPathV5))
            {
                UnpackV5(filePath, folderPath, indexPathV5);
            }
            else if (File.Exists(indexPathV4))
            {
                UnpackV4(filePath, folderPath, indexPathV4);
            }
            else
            {
                Logger.ErrorNeedAnotherFile($"{IndexFileNameV4}/{IndexFileNameV5}");
            }
        }

        private static void UnpackV4(string filePath, string folderPath, string indexPath)
        {
            byte[] index;

            using (FileStream fs = File.OpenRead(indexPath))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    if (Encoding.ASCII.GetString(br.ReadBytes(8)) != IndexFileMagicV4)
                    {
                        Logger.ErrorInvalidArchive();
                    }
                    fs.Position = 312;
                    byte[] raw = br.ReadBytes((int)fs.Length - 312);
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
                        string name = br.ReadCString(Encoding.ASCII);
                        archives.Add(name.ToLower());
                        pos += 256;
                    }
                    archiveIndex = archives.IndexOf(Path.GetFileName(filePath).ToLower());
                    if (archiveIndex == -1)
                    {
                        Logger.ErrorInvalidArchive();
                    }
                    else
                    {
                        Logger.ShowVersion("alf", 4);
                        ms.Position = pos;
                        int fileCount = br.ReadInt32();
                        for (int i = 0; i < fileCount; i++)
                        {
                            Entry entry = new Entry();
                            entry.Name = Encoding.ASCII.GetString(br.ReadBytes(64)).TrimEnd('\0');
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

            Logger.InitBar(entries.Count);
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
                        Logger.UpdateBar();
                    }
                }
            }
            index = null;
        }

        private static void UnpackV5(string filePath, string folderPath, string indexPath)
        {
            byte[] index;

            using (FileStream fs = File.OpenRead(indexPath))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    if (Encoding.Unicode.GetString(br.ReadBytes(16)) != IndexFileMagicV5)
                    {
                        Logger.ErrorInvalidArchive();
                    }
                    fs.Position = 552;
                    byte[] raw = br.ReadBytes((int)fs.Length - 552);
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
                        archives.Add(name.ToLower());
                        pos += 512;
                    }
                    archiveIndex = archives.IndexOf(Path.GetFileName(filePath).ToLower());
                    if (archiveIndex == -1)
                    {
                        Logger.ErrorInvalidArchive();
                    }
                    else
                    {
                        Logger.ShowVersion("alf", 5);
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
            Logger.InitBar(entries.Count);
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
                        Logger.UpdateBar();
                    }
                }
            }
            index = null;

        }
    }
}
