using Log;
using System.IO;
using System.Text;
using Utility;

namespace ArcFormats.KID
{
    public class DAT
    {
        private static readonly string Magic = "LNK\0";

        private class Entry
        {
            public uint Offset { get; set; }
            public uint Size { get; set; }
            public string Name { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            if (Encoding.ASCII.GetString(br.ReadBytes(4)) != Magic)
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
                entry.Offset = br.ReadUInt32() + dataOffset;
                entry.Size = br.ReadUInt32() >> 1;
                entry.Name = ArcEncoding.Shift_JIS.GetString(br.ReadBytes(24)).TrimEnd('\0');
                long thisPos = fs.Position;
                fs.Position = entry.Offset;
                byte[] data = br.ReadBytes((int)entry.Size);
                File.WriteAllBytes(Path.Combine(folderPath, entry.Name), data);
                data = null;
                fs.Position = thisPos;
                LogUtility.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            bw.Write(Encoding.ASCII.GetBytes(Magic));
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
                byte[] data = File.ReadAllBytes(file.FullName);
                bw.Write(data);
                data = null;
                LogUtility.UpdateBar();
            }
            fw.Dispose();
        }
    }
}