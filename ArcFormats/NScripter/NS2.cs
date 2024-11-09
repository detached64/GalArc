using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using Utility;
using Utility.Extensions;

namespace ArcFormats.NScripter
{
    public class NS2
    {
        private class Entry
        {
            public string FullPath { get; set; }
            public uint Size { get; set; }
            public string RelativePath { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            uint dataOffset = br.ReadUInt32();
            if (dataOffset > new FileInfo(filePath).Length || dataOffset <= 0)
            {
                throw new Exception("Error:Encrypted data detected.");
            }
            Directory.CreateDirectory(folderPath);
            List<Entry> entries = new List<Entry>();

            while (fs.Position < dataOffset - 1)//dataOffset - 1 is the end of file path
            {
                Entry entry = new Entry();
                br.ReadByte();//skip "
                entry.FullPath = Path.Combine(folderPath, br.ReadCString(0x22));
                entry.Size = br.ReadUInt32();
                entries.Add(entry);
            }
            br.ReadByte(); //skip e

            Logger.InitBar(entries.Count);

            foreach (Entry entry in entries)
            {
                byte[] data = br.ReadBytes((int)entry.Size);
                Utils.CreateParentDirectoryIfNotExists(entry.FullPath);
                File.WriteAllBytes(entry.FullPath, data);
                data = null;
                Logger.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);

            string[] fullPaths = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
            string[] relativePaths = Utils.GetRelativePaths(fullPaths, folderPath);

            int fileCount = fullPaths.Length;
            Logger.InitBar(fileCount);
            uint dataOffset = 4;

            Utils.Sort(fullPaths);

            List<Entry> entries = new List<Entry>();

            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new Entry();
                entry.RelativePath = relativePaths[i];
                entry.FullPath = Path.Combine(folderPath, relativePaths[i]);
                entry.Size = (uint)new FileInfo(entry.FullPath).Length;
                dataOffset += (uint)(ArcEncoding.Shift_JIS.GetBytes(entry.RelativePath).Length + 2);
                dataOffset += 4;
                entries.Add(entry);
            }
            dataOffset += 1;//'e'
            bw.Write(dataOffset);
            foreach (Entry entry in entries)
            {
                bw.Write('\"');
                bw.Write(ArcEncoding.Shift_JIS.GetBytes(entry.RelativePath));
                bw.Write('\"');
                bw.Write(entry.Size);
            }
            bw.Write('e');

            foreach (Entry entry in entries)
            {
                byte[] buffer = File.ReadAllBytes(entry.FullPath);
                bw.Write(buffer);
                buffer = null;
                Logger.UpdateBar();
            }
            fw.Dispose();
            bw.Dispose();
        }
    }
}