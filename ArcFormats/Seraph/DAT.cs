using Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Utility;
using Utility.Compression;

namespace ArcFormats.Seraph
{
    public class DAT
    {
        public static UserControl UnpackExtraOptions = new UnpackDATOptions();

        private class Entry
        {
            public uint Offset { get; set; }
            public uint Size { get; set; }
            public string Name { get; set; }
        }

        private class Group
        {
            public uint Offset { get; set; }
            public int FileCount { get; set; }
            public List<Entry> Entries { get; set; }
        }

        private static readonly string ArchPacName = "ArchPac.dat";

        private static readonly string ScnPacName = "ScnPac.dat";

        private List<Group> Groups { get; set; } = new List<Group>(0x40);

        private HashSet<long> Indices { get; set; } = new HashSet<long>();

        public void Unpack(string filePath, string folderPath)
        {
            string name = Path.GetFileName(filePath);
            if (name.Equals(ArchPacName, StringComparison.OrdinalIgnoreCase))
            {
                using (FileStream fs = File.OpenRead(filePath))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        Indices.Clear();
                        if (UnpackDATOptions.useBrutalForce)
                        {
                            LogUtility.Debug(Seraph.logBrutalForcing);
                            GuessIndexArchPac(br);
                        }
                        else if (UnpackDATOptions.useSpecifiedIndexOffset)
                        {
                            AddIndex(Convert.ToUInt32(UnpackDATOptions.specifiedIndexOffsetString, 16), br.BaseStream.Length);
                        }
                        TryReadIndexArchPac(br);
                        ExtractArchPac(br, folderPath);
                    }
                }
            }
            else if (name.Equals(ScnPacName, StringComparison.OrdinalIgnoreCase))
            {
                using (FileStream fs = File.OpenRead(filePath))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        LogUtility.Debug(string.Format(Seraph.logIndexOffset, "00000000"));
                        ReadIndexScnPac(br, folderPath);
                    }
                }
            }
            else
            {
                LogUtility.Error("Invaild archive type.");
            }
        }

        private void GuessIndexArchPac(BinaryReader br)
        {
            int byteRead;
            long maxOffset = br.BaseStream.Length;
            for (uint i = 0; i < maxOffset; i++)
            {
                if (br.BaseStream.Position >= maxOffset)
                {
                    break;
                }

                byteRead = br.ReadByte();
                if (byteRead == 0)
                {
                    AddIndex(br.BaseStream.Position - 1, maxOffset);
                }
            }
        }

        private void AddIndex(long index, long maxOffset)
        {
            for (int i = -1; i < 0; i++)
            {
                long pos = index + i;
                if (pos < maxOffset && pos >= 0)
                {
                    Indices.Add(pos);
                }
            }
        }

        private void TryReadIndexArchPac(BinaryReader br)
        {
            LogUtility.SetBarMax(1000);
            int count = 1;
            foreach (var i in Indices)
            {
                Groups.Clear();
                LogUtility.SetBarValue(count * 1000 / Indices.Count);
                if (ReadIndexArchPac(br, i))
                {
                    return;
                }
                count++;
            }
            LogUtility.Error(Seraph.logBrutalForceFailed);
        }

        private bool ReadIndexArchPac(BinaryReader br, long indexOffset)
        {
            br.BaseStream.Seek(indexOffset, SeekOrigin.Begin);
            int groupCount = br.ReadInt32();
            int fileCount = br.ReadInt32();
            if (groupCount <= 0 || groupCount > 0x40 || fileCount <= 0)
            {
                return false;
            }
            for (int i = 0; i < groupCount; i++)
            {
                Group group = new Group();
                group.Offset = br.ReadUInt32();
                group.FileCount = br.ReadInt32();
                if (group.FileCount <= 0 || group.FileCount > fileCount || group.Offset > br.BaseStream.Length)
                {
                    return false;
                }
                group.Entries = new List<Entry>(group.FileCount);
                Groups.Add(group);
            }
            uint baseOffset = 0;
            for (int i = groupCount - 1; i >= 0; i--)
            {
                uint thisOffset = br.ReadUInt32();
                if (thisOffset != 0)
                {
                    return false;
                }
                thisOffset += baseOffset;
                for (int j = 0; j < Groups[i].FileCount; j++)
                {
                    Entry entry = new Entry();
                    entry.Offset = thisOffset;
                    thisOffset = br.ReadUInt32() + baseOffset;
                    entry.Size = thisOffset - entry.Offset;
                    entry.Name = $"{i}_{j:D5}.cts";
                    Groups[i].Entries.Add(entry);
                }
                baseOffset = Groups[i].Entries[Groups[i].FileCount - 1].Offset + Groups[i].Entries[Groups[i].FileCount - 1].Size;
            }
            LogUtility.Debug(string.Format(Seraph.logIndexOffset, $"{indexOffset:X8}"));
            LogUtility.ResetBar();
            LogUtility.InitBar(fileCount);
            return true;
        }

        private void ExtractArchPac(BinaryReader br, string folderPath)
        {
            Directory.CreateDirectory(folderPath);
            foreach (Group group in Groups)
            {
                foreach (Entry entry in group.Entries)
                {
                    br.BaseStream.Position = entry.Offset;
                    byte[] buffer = br.ReadBytes((int)entry.Size);
                    if (buffer.Length < 4)
                    {
                        File.WriteAllBytes(Path.Combine(folderPath, entry.Name), buffer);
                        buffer = null;
                        continue;
                    }
                    uint sig = BitConverter.ToUInt32(buffer, 0);
                    if (sig == 1 && buffer[4] == 0x78)
                    {
                        byte[] raw = new byte[buffer.Length - 6];
                        Array.Copy(buffer, 6, raw, 0, raw.Length);
                        raw = Zlib.DecompressBytes(raw);
                        File.WriteAllBytes(Path.Combine(folderPath, entry.Name), raw);
                        raw = null;
                    }
                    else if ((sig & 0xffff) == 0x9c78)
                    {
                        byte[] raw = new byte[buffer.Length - 2];
                        Array.Copy(buffer, 2, raw, 0, raw.Length);
                        raw = Zlib.DecompressBytes(raw);
                        File.WriteAllBytes(Path.Combine(folderPath, entry.Name), raw);
                        raw = null;
                    }
                    else
                    {
                        File.WriteAllBytes(Path.Combine(folderPath, entry.Name), buffer);
                    }
                    buffer = null;
                    LogUtility.UpdateBar();
                }
            }
        }

        private void ReadIndexScnPac(BinaryReader br, string folderPath)
        {
            Group group = new Group();
            group.FileCount = br.ReadInt32();
            if (group.FileCount <= 0)
            {
                throw new Exception("Failed to read index.");
            }
            group.Entries = new List<Entry>(group.FileCount);
            uint thisOffset = br.ReadUInt32();

            for (int i = 0; i < group.FileCount; i++)
            {
                Entry entry = new Entry();
                entry.Offset = thisOffset;
                thisOffset = br.ReadUInt32();
                entry.Size = thisOffset - entry.Offset;
                entry.Name = $"{i:D5}";
                group.Entries.Add(entry);
            }
            LogUtility.InitBar(group.FileCount);
            Directory.CreateDirectory(folderPath);
            foreach (var entry in group.Entries)
            {
                br.BaseStream.Position = entry.Offset;
                byte[] buffer = br.ReadBytes((int)entry.Size);
                if (buffer.Length < 4)
                {
                    File.WriteAllBytes(Path.Combine(folderPath, entry.Name), buffer);
                    buffer = null;
                    continue;
                }
                uint sig = BitConverter.ToUInt32(buffer, 0);
                if (sig == 1 && buffer[4] == 0x78)
                {
                    byte[] raw = new byte[buffer.Length - 6];
                    Array.Copy(buffer, 6, raw, 0, raw.Length);
                    raw = Zlib.DecompressBytes(raw);
                    try
                    {
                        byte[] lz = Lz.Decompress(raw);
                        File.WriteAllBytes(Path.Combine(folderPath, entry.Name), lz);
                    }
                    catch
                    {
                        File.WriteAllBytes(Path.Combine(folderPath, entry.Name), raw);
                    }
                    finally
                    {
                        raw = null;
                    }
                }
                else if ((sig & 0xffff) == 0x9c78)
                {
                    byte[] raw = new byte[buffer.Length - 2];
                    Array.Copy(buffer, 2, raw, 0, raw.Length);
                    raw = Zlib.DecompressBytes(raw);
                    try
                    {
                        byte[] lz = Lz.Decompress(raw);
                        File.WriteAllBytes(Path.Combine(folderPath, entry.Name), lz);
                    }
                    catch
                    {
                        File.WriteAllBytes(Path.Combine(folderPath, entry.Name), raw);
                    }
                    finally
                    {
                        raw = null;
                    }
                }
                else
                {
                    File.WriteAllBytes(Path.Combine(folderPath, entry.Name), buffer);
                }
                buffer = null;
                LogUtility.UpdateBar();
            }
        }
    }
}
