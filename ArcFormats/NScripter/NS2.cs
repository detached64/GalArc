using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using Utility;
using Utility.Extensions;

namespace ArcFormats.NScripter
{
    public class NS2 : ArchiveFormat
    {
        private class Ns2Entry : PackedEntry
        {
            public string RelativePath { get; set; }
        }

        public override void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            uint dataOffset = br.ReadUInt32();
            if (dataOffset > new FileInfo(filePath).Length || dataOffset <= 0)
            {
                throw new Exception("Error:Encrypted data detected.");
            }
            Directory.CreateDirectory(folderPath);
            List<Ns2Entry> entries = new List<Ns2Entry>();

            while (fs.Position < dataOffset - 1)//dataOffset - 1 is the end of file path
            {
                Ns2Entry entry = new Ns2Entry();
                br.ReadByte();//skip "
                entry.Path = Path.Combine(folderPath, br.ReadCString(0x22));
                entry.Size = br.ReadUInt32();
                entries.Add(entry);
            }
            br.ReadByte(); //skip e

            Logger.InitBar(entries.Count);

            foreach (Ns2Entry entry in entries)
            {
                byte[] data = br.ReadBytes((int)entry.Size);
                Directory.CreateDirectory(Path.GetDirectoryName(entry.Path));
                File.WriteAllBytes(entry.Path, data);
                data = null;
                Logger.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        public override void Pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);

            string[] fullPaths = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
            string[] relativePaths = Utils.GetRelativePaths(fullPaths, folderPath);

            int fileCount = fullPaths.Length;
            Logger.InitBar(fileCount);
            uint dataOffset = 4;

            Array.Sort(fullPaths, StringComparer.Ordinal);

            List<Ns2Entry> entries = new List<Ns2Entry>();

            for (int i = 0; i < fileCount; i++)
            {
                Ns2Entry entry = new Ns2Entry();
                entry.RelativePath = relativePaths[i];
                entry.Path = Path.Combine(folderPath, relativePaths[i]);
                entry.Size = (uint)new FileInfo(entry.Path).Length;
                dataOffset += (uint)(ArcEncoding.Shift_JIS.GetBytes(entry.RelativePath).Length + 2);
                dataOffset += 4;
                entries.Add(entry);
            }
            dataOffset++;//'e'
            bw.Write(dataOffset);
            foreach (Ns2Entry entry in entries)
            {
                bw.Write('\"');
                bw.Write(ArcEncoding.Shift_JIS.GetBytes(entry.RelativePath));
                bw.Write('\"');
                bw.Write(entry.Size);
            }
            bw.Write('e');

            foreach (Ns2Entry entry in entries)
            {
                byte[] buffer = File.ReadAllBytes(entry.Path);
                bw.Write(buffer);
                buffer = null;
                Logger.UpdateBar();
            }
            fw.Dispose();
            bw.Dispose();
        }
    }
}