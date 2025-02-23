using GalArc.Logs;
using System.Collections.Generic;
using System.IO;
using Utility;
using Utility.Extensions;

namespace ArcFormats.Triangle
{
    public class CG : ArchiveFormat
    {
        public override void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            List<Entry> entries = new List<Entry>();
            uint fileCount = br.ReadUInt32();
            Logger.InitBar(fileCount);
            Directory.CreateDirectory(folderPath);

            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new Entry();
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
                Logger.UpdateBar();
            }
            byte[] dataLast = br.ReadBytes((int)(fs.Length - entries[entries.Count - 1].Offset));
            string fileNameLast = Path.Combine(folderPath, entries[entries.Count - 1].Name);
            File.WriteAllBytes(fileNameLast, dataLast);
            dataLast = null;
            Logger.UpdateBar();
            fs.Dispose();
            br.Dispose();
        }

        public override void Pack(string folderPath, string filePath)
        {
            FileStream fs = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fs);
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            FileInfo[] files = dir.GetFiles();
            int fileCount = files.Length;
            bw.Write(fileCount);
            uint dataOffset = (uint)(4 + 20 * fileCount);
            Logger.InitBar(fileCount);

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
                Logger.UpdateBar();
            }
            fs.Dispose();
            bw.Dispose();
        }
    }
}