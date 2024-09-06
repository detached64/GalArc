using Log;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Utility;

namespace ArcFormats.KID
{
    public class DAT
    {
        private struct KID_dat_header
        {
            public string magic { get; set; }
            public uint fileCount { get; set; }
            public long reserve { get; set; }
        }
        private struct KID_dat_entry
        {
            public uint offset { get; set; }
            public uint size { get; set; }
            public string name { get; set; }
        }

        public static void Unpack(string filePath, string folderPath, Encoding encoding)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            if (Encoding.ASCII.GetString(br.ReadBytes(4)).TrimEnd('\0') != "LNK")
            {
                LogUtility.Error_NotValidArchive();
            }
            int fileCount = br.ReadInt32();
            br.ReadBytes(8);
            uint dataOffset = 16 + 32 * (uint)fileCount;
            Directory.CreateDirectory(folderPath);
            LogUtility.InitBar(fileCount);
            for (int i = 0; i < fileCount; i++)
            {
                KID_dat_entry entry = new KID_dat_entry();
                entry.offset = br.ReadUInt32() + dataOffset;
                entry.size = br.ReadUInt32() >> 1;
                entry.name = ArcEncoding.Shift_JIS.GetString(br.ReadBytes(24)).TrimEnd('\0');
                long thisPos = fs.Position;
                fs.Position = entry.offset;
                byte[] data = br.ReadBytes((int)entry.size);
                File.WriteAllBytes(folderPath + "\\" + entry.name, data);
                fs.Position = thisPos;
                LogUtility.UpdateBar();
            }
            fs.Dispose();
        }
        public static void Pack(string folderPath, string filePath, string version, Encoding encoding)
        {
            FileStream fw = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fw);
            bw.Write(Encoding.ASCII.GetBytes("LNK"));
            bw.Write((byte)0);
            int fileCount = Utilities.GetFileCount_TopOnly(folderPath);
            bw.Write(fileCount);
            bw.Write((long)0);
            DirectoryInfo d = new DirectoryInfo(folderPath);
            LogUtility.InitBar(fileCount);
            uint offset = 0;
            foreach (var file in d.GetFiles("*.*", SearchOption.TopDirectoryOnly))
            {
                bw.Write(offset);
                bw.Write((uint)file.Length << 1);
                bw.Write(ArcEncoding.Shift_JIS.GetBytes(file.Name.PadRight(24, '\0')));
                offset += (uint)file.Length;
            }
            foreach (var file in d.GetFiles("*.*", SearchOption.TopDirectoryOnly))
            {
                bw.Write(File.ReadAllBytes(file.FullName));
                LogUtility.UpdateBar();
            }
            fw.Dispose();
        }

    }
}
