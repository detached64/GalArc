using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System.Collections.Generic;
using System.IO;

namespace GalArc.Models.Formats.Triangle;

internal class CG : ArcFormat
{
    public override string Name => "CG";
    public override string Description => "Triangle CG Archive";
    public override bool CanWrite => true;

    public override void Unpack(string filePath, string folderPath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using BinaryReader br = new(fs);
        List<Entry> entries = [];
        int fileCount = br.ReadInt32();
        ProgressManager.SetMax(fileCount);

        for (int i = 0; i < fileCount; i++)
        {
            Entry entry = new();
            entry.Name = ArcEncoding.Shift_JIS.GetString(br.ReadBytes(16)).TrimEnd('\0');
            //fs.Position = 4 + 20 * i + 16;
            entry.Offset = br.ReadUInt32();
            entries.Add(entry);
        }

        for (int i = 0; i < entries.Count - 1; i++)
        {
            byte[] data = br.ReadBytes((int)(entries[i + 1].Offset - entries[i].Offset));
            string fileName = Path.Combine(folderPath, entries[i].Name);
            File.WriteAllBytes(fileName, data);
            data = null;
            ProgressManager.Progress();
        }
        byte[] dataLast = br.ReadBytes((int)(fs.Length - entries[^1].Offset));
        string fileNameLast = Path.Combine(folderPath, entries[^1].Name);
        File.WriteAllBytes(fileNameLast, dataLast);
        dataLast = null;
        ProgressManager.Progress();
    }

    public override void Pack(string folderPath, string filePath)
    {
        using FileStream fs = File.Create(filePath);
        using BinaryWriter bw = new(fs);
        DirectoryInfo dir = new(folderPath);
        FileInfo[] files = dir.GetFiles();
        int fileCount = files.Length;
        bw.Write(fileCount);
        uint dataOffset = (uint)(4 + (20 * fileCount));
        ProgressManager.SetMax(fileCount);

        foreach (FileInfo file in files)
        {
            bw.WritePaddedString(file.Name, 16);
            bw.Write(dataOffset);
            dataOffset += (uint)file.Length;
        }

        foreach (FileInfo file in files)
        {
            byte[] data = File.ReadAllBytes(file.FullName);
            bw.Write(data);
            data = null;
            ProgressManager.Progress();
        }
    }
}
