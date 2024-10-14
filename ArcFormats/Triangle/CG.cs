using Log;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utility;

namespace ArcFormats.Triangle
{
    public class CG
    {
        private class Entry
        {
            public string name { get; set; }
            public uint offset { get; set; }
            public uint size { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            List<Entry> entries = new List<Entry>();
            uint fileCount = br.ReadUInt32();
            LogUtility.InitBar(fileCount);
            Directory.CreateDirectory(folderPath);

            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new Entry();
                entry.name = ArcEncoding.Shift_JIS.GetString(br.ReadBytes(16)).TrimEnd('\0');
                //fs.Position = 4 + 20 * i + 16;
                entry.offset = br.ReadUInt32();
                entries.Add(entry);
            }

            for (int i = 0; i < entries.Count - 1; i++)
            {
                byte[] data = br.ReadBytes((int)(entries[i + 1].offset - entries[i].offset));
                string fileName = folderPath + "\\" + entries[i].name;
                File.WriteAllBytes(fileName, data);
                LogUtility.UpdateBar();
            }
            byte[] dataLast = br.ReadBytes((int)(fs.Length - entries[entries.Count - 1].offset));
            string fileNameLast = folderPath + "\\" + entries[entries.Count - 1].name;
            File.WriteAllBytes(fileNameLast, dataLast);
            LogUtility.UpdateBar();
            fs.Dispose();
            br.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            FileStream fs = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fs);
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            FileInfo[] files = dir.GetFiles();
            int fileCount = files.Length;
            bw.Write(fileCount);
            uint dataOffset = (uint)(4 + 20 * fileCount);
            LogUtility.InitBar(fileCount);

            foreach (FileInfo file in files)
            {
                bw.Write(ArcEncoding.Shift_JIS.GetBytes(file.Name.PadRight(16, '\0')));
                bw.Write(dataOffset);
                dataOffset += (uint)file.Length;
            }

            foreach (FileInfo file in files)
            {
                byte[] data = File.ReadAllBytes(file.FullName);
                bw.Write(data);
                LogUtility.UpdateBar();
            }
            fs.Dispose();
            bw.Dispose();
        }
    }
}