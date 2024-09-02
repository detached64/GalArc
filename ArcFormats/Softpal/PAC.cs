using Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utility;

namespace ArcFormats.Softpal
{
    public class PAC
    {
        struct Softpal_pac_entry
        {
            public string fileName { get; set; }
            public uint fileSize { get; set; }
            public uint offset { get; set; }
        }
        public static void Unpack(string filePath, string folderPath, Encoding encoding)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            int fileCount = br.ReadInt32();
            fs.Position = 0x3fe;
            List<Softpal_pac_entry> entries = new List<Softpal_pac_entry>();
            LogUtility.InitBar(fileCount);
            Directory.CreateDirectory(folderPath);

            for (int i = 0; i < fileCount; i++)
            {
                Softpal_pac_entry entry = new Softpal_pac_entry();
                entry.fileName = ArcEncoding.Shift_JIS.GetString(br.ReadBytes(32)).TrimEnd('\0');
                entry.fileSize = br.ReadUInt32();
                entry.offset = br.ReadUInt32();
                entries.Add(entry);
            }
            for (int i = 0; i < fileCount; i++)
            {
                byte[] data = br.ReadBytes((int)entries[i].fileSize);
                File.WriteAllBytes(folderPath + "\\" + entries[i].fileName, data);
                LogUtility.UpdateBar();
            }
            fs.Dispose();
        }
        public static void Pack(string folderPath, string filePath, string version, Encoding encoding)
        {
            FileStream fw = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fw);
            int fileCount = Utilities.GetFileCount_TopOnly(folderPath);
            LogUtility.InitBar(fileCount);
            bw.Write(fileCount);
            bw.Write(new byte[1018]);
            DirectoryInfo d = new DirectoryInfo(folderPath);
            uint offset = 0x3fe + (uint)(40 * fileCount);
            foreach (var file in d.GetFiles("*.*", SearchOption.TopDirectoryOnly))
            {
                bw.Write(ArcEncoding.Shift_JIS.GetBytes(file.Name.PadRight(32, '\0')));
                bw.Write((int)file.Length);
                bw.Write(offset);
                offset += (uint)file.Length;
            }
            foreach (var file in d.GetFiles("*.*", SearchOption.TopDirectoryOnly))
            {
                bw.Write(File.ReadAllBytes(file.FullName));
                LogUtility.UpdateBar();
            }
            bw.Write(Encoding.ASCII.GetBytes("EOF "));
            fw.Dispose();
        }
    }
}
