using Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility;

namespace ArcFormats.NScripter
{
    public class NS2
    {
        private struct Entry
        {
            public string filePath { get; set; }
            public uint fileSize { get; set; }
            public string filePathDivided { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
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
                entry.filePath = folderPath + "\\" + Utilities.ReadCString(br, ArcEncoding.Shift_JIS, 0x22);
                entry.fileSize = br.ReadUInt32();
                entries.Add(entry);
            }
            br.ReadByte(); //skip e

            LogUtility.InitBar(entries.Count);

            foreach (Entry entry in entries)
            {
                byte[] data = br.ReadBytes((int)entry.fileSize);
                Directory.CreateDirectory(Path.GetDirectoryName(entry.filePath));
                File.WriteAllBytes(entry.filePath, data);
                LogUtility.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            int fileCount = Utilities.GetFileCount_All(folderPath);
            LogUtility.InitBar(fileCount);
            uint dataOffset = 4;

            string[] pathString = new string[fileCount];
            DirectoryInfo d = new DirectoryInfo(folderPath);
            int i = 0;
            foreach (FileInfo file in d.GetFiles("*.*", SearchOption.AllDirectories))
            {
                pathString[i] = file.FullName.Substring(folderPath.Length + 1);
                i++;
            }
            Utilities.InsertSort(pathString);//cannot use getfiles,like pf8

            List<Entry> entries = new List<Entry>();

            for (int j = 0; j < fileCount; j++)
            {
                Entry entry = new Entry();
                entry.filePathDivided = pathString[j];              //contains '/'
                entry.filePath = folderPath + "\\" + pathString[j];
                entry.fileSize = (uint)new FileInfo(entry.filePath).Length;
                dataOffset += (uint)(ArcEncoding.Shift_JIS.GetBytes(entry.filePathDivided).Length + 2);//to avoid japanese character length error
                dataOffset += 4;
                entries.Add(entry);
            }
            dataOffset += 1;//'e'
            FileStream fw = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fw);
            bw.Write(dataOffset);
            for (int j = 0; j < fileCount; j++)
            {
                bw.Write('\"');
                bw.Write(ArcEncoding.Shift_JIS.GetBytes(entries[j].filePathDivided));
                bw.Write('\"');
                bw.Write(entries[j].fileSize);
            }
            bw.Write('e');

            for (int j = 0; j < fileCount; j++)
            {
                byte[] buffer = File.ReadAllBytes(entries[j].filePath);
                bw.Write(buffer);
                LogUtility.UpdateBar();
            }
            fw.Dispose();
            bw.Dispose();
        }
    }
}