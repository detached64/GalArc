using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace GalArc.Models.Formats.Valkyria;

internal class DAT : ArcFormat
{
    public override string Name => "DAT";
    public override string Description => "Valkyria Archive";
    public override bool CanWrite => true;

    public override void Unpack(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        try
        {
            uint indexSize = br.ReadUInt32();
            if (indexSize == 0 || indexSize >= fs.Length)
            {
                throw new InvalidDataException("Invalid index size");
            }

            int fileCount = (int)indexSize / 0x10C;
            if (indexSize != (uint)fileCount * 0x10C || (uint)fileCount > 10000)
            {
                throw new InvalidDataException("Invalid file count or index size");
            }

            uint indexOffset = 4;
            long baseOffset = indexOffset + indexSize;
            List<Entry> entries = [];

            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new();
                entry.Name = br.ReadCString();
                fs.Position = indexOffset + 0x104;
                uint offset = br.ReadUInt32();
                entry.Offset = (uint)(baseOffset + offset);
                entry.Size = br.ReadUInt32();
                if (entry.Offset + entry.Size > fs.Length)
                {
                    throw new InvalidDataException($"Invalid file placement: {entry.Name} at offset {entry.Offset} with size {entry.Size}");
                }

                entries.Add(entry);
                indexOffset += 0x10C;
            }
            ProgressManager.SetMax(fileCount);

            foreach (Entry entry in entries)
            {
                string safeName = Path.GetFileName(entry.Name);
                string outputPath = Path.Combine(folderPath, safeName);

                fs.Position = entry.Offset;
                byte[] data = new byte[entry.Size];
                int bytesRead = fs.Read(data, 0, (int)entry.Size);

                if (bytesRead == entry.Size)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                    File.WriteAllBytes(outputPath, data);
                    ProgressManager.Progress();
                    data = null;
                }
                else
                {
                    throw new InvalidDataException($"Failed to read complete file: {entry.Name}");
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"Error processing DAT archive: {ex.Message}", ex);
        }
    }

    public override void Pack(string folderPath, string filePath)
    {
        using FileStream fw = File.Create(filePath);
        using BinaryWriter bw = new(fw);
        FileInfo[] files = new DirectoryInfo(folderPath).GetFiles();
        uint indexSize = (uint)(files.Length * 0x10C);
        uint offset = 0;
        bw.Write(indexSize);

        ProgressManager.SetMax(files.Length);

        foreach (FileInfo file in files)
        {
            bw.WritePaddedString(file.Name, 0x104);
            bw.Write(offset);
            bw.Write((uint)file.Length);
            offset += (uint)file.Length;
        }

        foreach (FileInfo file in files)
        {
            bw.Write(File.ReadAllBytes(file.FullName));
            ProgressManager.Progress();
        }
    }
}
