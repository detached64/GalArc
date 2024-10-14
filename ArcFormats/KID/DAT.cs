using Log;
using System.IO;
using System.Text;
using Utility;

namespace ArcFormats.KID
{
    public class DAT
    {
        private class Header
        {
            public string magic { get; set; }
            public uint fileCount { get; set; }
            public long reserve { get; set; }
        }

        private class Entry
        {
            public uint offset { get; set; }
            public uint size { get; set; }
            public string name { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            if (Encoding.ASCII.GetString(br.ReadBytes(4)) != "LNK\0")
            {
                LogUtility.ErrorInvalidArchive();
            }
            int fileCount = br.ReadInt32();
            br.ReadBytes(8);
            uint dataOffset = 16 + 32 * (uint)fileCount;
            Directory.CreateDirectory(folderPath);
            LogUtility.InitBar(fileCount);
            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new Entry();
                entry.offset = br.ReadUInt32() + dataOffset;
                entry.size = br.ReadUInt32() >> 1;
                entry.name = ArcEncoding.Shift_JIS.GetString(br.ReadBytes(24)).TrimEnd('\0');
                long thisPos = fs.Position;
                fs.Position = entry.offset;
                byte[] data = br.ReadBytes((int)entry.size);
                File.WriteAllBytes(Path.Combine(folderPath, entry.name), data);
                fs.Position = thisPos;
                LogUtility.UpdateBar();
            }
            fs.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            FileStream fw = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fw);
            bw.Write(Encoding.ASCII.GetBytes("LNK"));
            bw.Write((byte)0);
            DirectoryInfo d = new DirectoryInfo(folderPath);
            FileInfo[] files = d.GetFiles();
            int fileCount = files.Length;
            bw.Write(fileCount);
            bw.Write((long)0);
            LogUtility.InitBar(fileCount);
            uint offset = 0;
            foreach (FileInfo file in files)
            {
                bw.Write(offset);
                bw.Write((uint)file.Length << 1);
                bw.Write(ArcEncoding.Shift_JIS.GetBytes(file.Name.PadRight(24, '\0')));
                offset += (uint)file.Length;
            }
            foreach (FileInfo file in files)
            {
                bw.Write(File.ReadAllBytes(file.FullName));
                LogUtility.UpdateBar();
            }
            fw.Dispose();
        }
    }
}