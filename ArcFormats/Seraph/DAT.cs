using GalArc.Controls;
using GalArc.Database;
using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Utility.Compression;

namespace ArcFormats.Seraph
{
    public class DAT : ArchiveFormat
    {
        private static readonly Lazy<OptionsTemplate> _lazyUnpackOptions = new Lazy<OptionsTemplate>(() => new UnpackDATOptions());
        public static OptionsTemplate UnpackExtraOptions => _lazyUnpackOptions.Value;

        private class Group
        {
            public uint Offset { get; set; }
            public int FileCount { get; set; }
            public List<Entry> Entries { get; set; }
        }

        private readonly string ArchPacName = "ArchPac.dat";

        private readonly string ScnPacName = "ScnPac.dat";

        private readonly string VoicePacName = @"^Voice(?:\d|pac)\.dat$";

        private List<Group> Groups { get; } = new List<Group>(0x40);

        private HashSet<long> Indices { get; } = new HashSet<long>();

        internal static SeraphScheme Scheme;

        public override void Unpack(string filePath, string folderPath)
        {
            string name = Path.GetFileName(filePath);
            if (name.Equals(ArchPacName, StringComparison.OrdinalIgnoreCase))
            {
                using (FileStream fs = File.OpenRead(filePath))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        Indices.Clear();
                        bool isGiven = false;
                        if (UnpackDATOptions.UseBrutalForce)
                        {
                            Logger.Debug(Seraph.logBrutalForcing);
                            AddIndex();
                            GuessIndexArchPac(br);
                        }
                        else if (UnpackDATOptions.UseSpecifiedIndexOffset)
                        {
                            isGiven = true;
                            Indices.Add(Convert.ToUInt32(UnpackDATOptions.SpecifiedIndexOffsetString, 16));
                        }
                        TryReadIndexArchPac(br, isGiven);
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
                        Logger.Info(string.Format(Seraph.logIndexOffset, "00000000"));
                        UnpackScnPac(br, folderPath);
                    }
                }
            }
            else if (Regex.IsMatch(name, VoicePacName, RegexOptions.IgnoreCase))
            {
                using (FileStream fs = File.OpenRead(filePath))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        UnpackVoicePac(br, folderPath);
                    }
                }
            }
            else
            {
                Logger.Error(Seraph.logInvalidArchiveType);
            }
        }

        private void GuessIndexArchPac(BinaryReader br)
        {
            int thisByte, lastByte = 0;
            long maxOffset = br.BaseStream.Length;
            for (uint i = 0; i < maxOffset; i++)
            {
                if (br.BaseStream.Position >= maxOffset)
                {
                    break;
                }

                thisByte = br.ReadByte();
                if (thisByte == 0 && lastByte != 0)
                {
                    Indices.Add(br.BaseStream.Position - 2);
                }
                lastByte = thisByte;
            }
        }

        private void TryReadIndexArchPac(BinaryReader br, bool isGiven)
        {
            Logger.SetBarMax(Indices.Count);
            int count = 1;
            foreach (var i in Indices)
            {
                Groups.Clear();
                Logger.SetBarValue(count);
                if (ReadIndexArchPac(br, i))
                {
                    return;
                }
                count++;
            }
            if (isGiven)
            {
                Logger.Error(Seraph.logSpecifiedIndexOffsetFailed);
            }
            else
            {
                Logger.Error(Seraph.logBrutalForceFailed);
            }
        }

        private bool ReadIndexArchPac(BinaryReader br, long indexOffset)
        {
            if (indexOffset >= br.BaseStream.Length)
            {
                return false;
            }
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
            Logger.Info(string.Format(Seraph.logIndexOffset, $"{indexOffset:X8}"));
            Logger.ResetBar();
            Logger.InitBar(fileCount);
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
                        Buffer.BlockCopy(buffer, 6, raw, 0, raw.Length);
                        raw = ZlibHelper.Decompress(raw);
                        File.WriteAllBytes(Path.Combine(folderPath, entry.Name), raw);
                        raw = null;
                    }
                    else if ((sig & 0xffff) == 0x9c78)
                    {
                        byte[] raw = new byte[buffer.Length - 2];
                        Buffer.BlockCopy(buffer, 2, raw, 0, raw.Length);
                        raw = ZlibHelper.Decompress(raw);
                        File.WriteAllBytes(Path.Combine(folderPath, entry.Name), raw);
                        raw = null;
                    }
                    else
                    {
                        File.WriteAllBytes(Path.Combine(folderPath, entry.Name), buffer);
                    }
                    buffer = null;
                    Logger.UpdateBar();
                }
            }
        }

        private void UnpackScnPac(BinaryReader br, string folderPath)
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
            Logger.InitBar(group.FileCount);
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
                    Buffer.BlockCopy(buffer, 6, raw, 0, raw.Length);
                    buffer = ZlibHelper.Decompress(raw);
                }

                try
                {
                    byte[] lz = SeraphUtils.LzDecompress(buffer);
                    File.WriteAllBytes(Path.Combine(folderPath, entry.Name), lz);
                }
                catch
                {
                    File.WriteAllBytes(Path.Combine(folderPath, entry.Name), buffer);
                }
                finally
                {
                    buffer = null;
                }
                Logger.UpdateBar();
            }
        }

        private void UnpackVoicePac(BinaryReader br, string folderPath)
        {
            int fileCount = br.ReadUInt16();
            uint dataOffset = 2 + 4 * (uint)fileCount;
            uint nextOffset = br.ReadUInt32();
            if (nextOffset < dataOffset || nextOffset > br.BaseStream.Length)
            {
                UnpackVoicePacV2(br, fileCount, folderPath);
            }
            else
            {
                UnpackVoicePacV1(br, fileCount, folderPath);
            }
        }

        private void UnpackVoicePacV2(BinaryReader br, int fileCount, string folderPath)
        {
            br.BaseStream.Position = 2;
            List<Entry> entries = new List<Entry>(fileCount);
            for (int i = 0; i < fileCount; i++)
            {
                br.BaseStream.Position += 4;
                Entry entry = new Entry();
                entry.Offset = br.ReadUInt32();
                entry.Size = br.ReadUInt32();
                entry.Name = $"{i:D5}.wav";
                entries.Add(entry);
            }
            Logger.InitBar(fileCount);
            Directory.CreateDirectory(folderPath);
            foreach (var entry in entries)
            {
                br.BaseStream.Position = entry.Offset;
                byte[] buffer = br.ReadBytes((int)entry.Size);
                File.WriteAllBytes(Path.Combine(folderPath, entry.Name), buffer);
                buffer = null;
                Logger.UpdateBar();
            }
        }

        private void UnpackVoicePacV1(BinaryReader br, int fileCount, string folderPath)
        {
            br.BaseStream.Position = 2;
            List<Entry> entries = new List<Entry>(fileCount);
            uint thisOffset = br.ReadUInt32();
            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new Entry();
                entry.Offset = thisOffset;
                thisOffset = br.ReadUInt32();
                entry.Size = thisOffset - entry.Offset;
                entry.Name = $"{i:D5}.wav";
                entries.Add(entry);
            }
            Logger.InitBar(fileCount);
            Directory.CreateDirectory(folderPath);
            foreach (var entry in entries)
            {
                br.BaseStream.Position = entry.Offset;
                byte[] buffer = br.ReadBytes((int)entry.Size);
                File.WriteAllBytes(Path.Combine(folderPath, entry.Name), buffer);
                buffer = null;
                Logger.UpdateBar();
            }
        }

        private void AddIndex()
        {
            if (Scheme == null)
            {
                return;
            }
            foreach (long offset in Scheme.KnownOffsets)
            {
                Indices.Add(offset);
            }
        }

        public override void DeserializeScheme(out string name, out int count)
        {
            Scheme = Deserializer.ReadScheme<SeraphScheme>();
            name = "Seraph";
            count = Scheme?.KnownOffsets?.Count ?? 0;
        }
    }
}
