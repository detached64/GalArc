using Log;
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
            public string filePath { get; set; }
            public uint fileSize { get; set; }
            public string relativePath { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            int dataOffset = br.ReadInt32();
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
                entry.filePath = Path.Combine(folderPath, br.ReadCString(ArcEncoding.Shift_JIS, 0x22));
                entry.fileSize = br.ReadUInt32();
                entries.Add(entry);
            }
            br.ReadByte(); //skip e

            LogUtility.InitBar(entries.Count);

            foreach (Entry entry in entries)
            {
                byte[] data = br.ReadBytes((int)entry.fileSize);
                Utils.CreateParentDirectoryIfNotExists(entry.filePath);
                File.WriteAllBytes(entry.filePath, data);
                LogUtility.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            int fileCount = Utils.GetFileCount(folderPath);
            LogUtility.InitBar(fileCount);
            uint dataOffset = 4;

            string[] fullPaths = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
            string[] relativePaths = Utils.GetRelativePaths(fullPaths, folderPath);
            Utils.InsertSort(fullPaths);

            List<Entry> entries = new List<Entry>();

            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new Entry();
                entry.relativePath = relativePaths[i];
                entry.filePath = Path.Combine(folderPath, relativePaths[i]);
                entry.fileSize = (uint)new FileInfo(entry.filePath).Length;
                dataOffset += (uint)(ArcEncoding.Shift_JIS.GetBytes(entry.relativePath).Length + 2);
                dataOffset += 4;
                entries.Add(entry);
            }
            dataOffset += 1;//'e'
            FileStream fw = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fw);
            bw.Write(dataOffset);
            for (int i = 0; i < fileCount; i++)
            {
                bw.Write('\"');
                bw.Write(ArcEncoding.Shift_JIS.GetBytes(entries[i].relativePath));
                bw.Write('\"');
                bw.Write(entries[i].fileSize);
            }
            bw.Write('e');

            for (int i = 0; i < fileCount; i++)
            {
                byte[] buffer = File.ReadAllBytes(entries[i].filePath);
                bw.Write(buffer);
                LogUtility.UpdateBar();
            }
            fw.Dispose();
            bw.Dispose();
        }
    }
}