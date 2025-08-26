using GalArc.Infrastructure.Logging;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GalArc.Models.Formats.Eushully;

internal class ALF : ArcFormat
{
    public override string Name => "ALF";
    public override string Description => "Eushully Archive";

    private readonly string IndexFileNameV5 = "SYS5INI.BIN";

    private readonly string IndexFileMagicV5 = "S5IC502 ";

    private readonly string IndexFileNameV4 = "sys4ini.bin";

    private readonly string IndexFileMagicV4 = "S4IC415 ";

    private class AlfEntry : Entry
    {
        public int ArchiveIndex { get; set; }
    }

    public override void Unpack(string filePath, string folderPath)
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
            throw new FileNotFoundException($"{IndexFileNameV4}/{IndexFileNameV5} not found.");
        }
    }

    private void UnpackV4(string filePath, string folderPath, string indexPath)
    {
        byte[] index;

        using (FileStream fs = File.OpenRead(indexPath))
        {
            using BinaryReader br = new(fs);
            if (Encoding.ASCII.GetString(br.ReadBytes(8)) != IndexFileMagicV4)
            {
                throw new InvalidArchiveException();
            }
            fs.Position = 312;
            byte[] raw = br.ReadBytes((int)fs.Length - 312);
            index = LzssHelper.Decompress(raw);
            raw = null;
        }
        List<string> archives = [];
        int archiveIndex;
        List<AlfEntry> entries = [];

        using (MemoryStream ms = new(index))
        {
            using BinaryReader br = new(ms);
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
                throw new InvalidArchiveException();
            }
            else
            {
                Logger.ShowVersion("alf", 4);
                ms.Position = pos;
                int fileCount = br.ReadInt32();
                for (int i = 0; i < fileCount; i++)
                {
                    AlfEntry entry = new();
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

        ProgressManager.SetMax(entries.Count);
        using (FileStream fs = File.OpenRead(filePath))
        {
            using BinaryReader br = new(fs);
            foreach (AlfEntry entry in entries)
            {
                fs.Position = entry.Offset;
                byte[] buffer = br.ReadBytes((int)entry.Size);
                File.WriteAllBytes(Path.Combine(folderPath, entry.Name), buffer);
                buffer = null;
                ProgressManager.Progress();
            }
        }
        index = null;
    }

    private void UnpackV5(string filePath, string folderPath, string indexPath)
    {
        byte[] index;

        using (FileStream fs = File.OpenRead(indexPath))
        {
            using BinaryReader br = new(fs);
            if (Encoding.Unicode.GetString(br.ReadBytes(16)) != IndexFileMagicV5)
            {
                throw new InvalidArchiveException();
            }
            fs.Position = 552;
            byte[] raw = br.ReadBytes((int)fs.Length - 552);
            index = LzssHelper.Decompress(raw);
            raw = null;
        }
        List<string> archives = [];
        int archiveIndex;
        List<AlfEntry> entries = [];

        using (MemoryStream ms = new(index))
        {
            using BinaryReader br = new(ms);
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
                throw new InvalidArchiveException();
            }
            else
            {
                Logger.ShowVersion("alf", 5);
                ms.Position = pos;
                int fileCount = br.ReadInt32();
                for (int i = 0; i < fileCount; i++)
                {
                    AlfEntry entry = new()
                    {
                        Name = Encoding.Unicode.GetString(br.ReadBytes(128)).TrimEnd('\0'),
                        ArchiveIndex = br.ReadInt32()
                    };
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
        ProgressManager.SetMax(entries.Count);
        using (FileStream fs = File.OpenRead(filePath))
        {
            using BinaryReader br = new(fs);
            foreach (AlfEntry entry in entries)
            {
                fs.Position = entry.Offset;
                byte[] buffer = br.ReadBytes((int)entry.Size);
                File.WriteAllBytes(Path.Combine(folderPath, entry.Name), buffer);
                buffer = null;
                ProgressManager.Progress();
            }
            index = null;
        }
    }
}
