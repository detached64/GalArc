using Log;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utility;

namespace ArcFormats.Triangle
{
    public class CG
    {
        private struct Triangle_CG_entry
        {
            public string name { get; set; }
            public uint offset { get; set; }
            public uint size { get; set; }
        }

        public static void Unpack(string filePath, string folderPath, Encoding encoding)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            List<Triangle_CG_entry> entries = new List<Triangle_CG_entry>();
            uint fileCount = br.ReadUInt32();
            LogUtility.InitBar(fileCount);
            Directory.CreateDirectory(folderPath);

            for (int i = 0; i < fileCount; i++)
            {
                Triangle_CG_entry entry = new Triangle_CG_entry();
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

        public static void Pack(string folderPath, string filePath, string version, Encoding encoding)
        {
            FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            uint fileCount = (uint)dir.GetFiles("*.*", SearchOption.TopDirectoryOnly).Count();
            bw.Write(fileCount);
            uint dataOffset = (uint)(4 + 20 * Utilities.GetFileCount_All(folderPath));
            LogUtility.InitBar(fileCount);

            foreach (FileInfo file in dir.GetFiles("*.*", SearchOption.TopDirectoryOnly))
            {
                bw.Write(ArcEncoding.Shift_JIS.GetBytes(file.Name.PadRight(16, '\0')));
                bw.Write(dataOffset);
                dataOffset += (uint)file.Length;
            }

            foreach (FileInfo file in dir.GetFiles("*.*", SearchOption.TopDirectoryOnly))
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