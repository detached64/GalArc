using Log;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utility;

namespace ArcFormats.Softpal
{
    public class PAC
    {
        private static byte[] magicV2 = { 0x50, 0x41, 0x43, 0x20 };//"PAC "
        private struct Entry
        {
            public string fileName { get; set; }
            public uint fileSize { get; set; }
            public uint offset { get; set; }
        }

        public static void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            bool isVer1 = !br.ReadBytes(4).SequenceEqual(magicV2);
            fs.Dispose();
            br.Dispose();
            if (isVer1)
            {
                LogUtility.ShowVersion("pac", 1);
                pacV1_unpack(filePath, folderPath);
            }
            else
            {
                LogUtility.ShowVersion("pac", 2);
                pacV2_unpack(filePath, folderPath);
            }
        }

        private static void pacV1_unpack(string filePath, string folderPath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            int fileCount = br.ReadInt32();
            fs.Position = 0x3fe;
            List<Entry> entries = new List<Entry>();
            LogUtility.InitBar(fileCount);
            Directory.CreateDirectory(folderPath);

            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new Entry();
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

        private static void pacV2_unpack(string filePath, string folderPath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            if (Encoding.ASCII.GetString(br.ReadBytes(4)) != "PAC ")
            {
                LogUtility.Error_NotValidArchive();
            }
            br.ReadInt32();
            uint fileCount = br.ReadUInt32();
            LogUtility.InitBar((int)fileCount);
            fs.Position = 0x804;
            Directory.CreateDirectory(folderPath);
            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new Entry();
                entry.fileName = ArcEncoding.Shift_JIS.GetString(br.ReadBytes(32)).TrimEnd('\0');
                entry.fileSize = br.ReadUInt32();
                entry.offset = br.ReadUInt32();
                long pos = fs.Position;
                fs.Position = entry.offset;
                byte[] fileData = br.ReadBytes((int)entry.fileSize);
                File.WriteAllBytes(folderPath + "\\" + entry.fileName, fileData);
                fs.Position = pos;
                LogUtility.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        public static void Pack(string folderPath, string filePath)
        {
            if (Global.Version == "1")
            {
                pacV1_pack(folderPath, filePath);
            }
            else
            {
                pacV2_pack(folderPath, filePath);
            }
        }

        private static void pacV1_pack(string folderPath, string filePath)
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

        private static void pacV2_pack(string folderPath, string filePath)
        {
            uint fileCount = (uint)Utilities.GetFileCount_All(folderPath);
            LogUtility.InitBar((int)fileCount);
            FileStream fw = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fw);
            //header
            bw.Write(Encoding.ASCII.GetBytes("PAC "));
            bw.Write(0);
            bw.Write(fileCount);
            bw.Write(new byte[532]);
            bw.Write(new byte[1508]);
            //entries
            DirectoryInfo d = new DirectoryInfo(folderPath);
            uint baseOffset = 2052 + 40 * fileCount;
            uint currentOffset = baseOffset;
            foreach (FileInfo fi in d.GetFiles())
            {
                bw.Write(ArcEncoding.Shift_JIS.GetBytes(fi.Name.PadRight(32, '\0')));
                bw.Write((uint)fi.Length);
                bw.Write(currentOffset);
                currentOffset += (uint)fi.Length;
            }
            //data
            foreach (FileInfo fi in d.GetFiles())
            {
                byte[] fileData = File.ReadAllBytes(fi.FullName);
                bw.Write(fileData);
                LogUtility.UpdateBar();
            }
            //end
            bw.Write(0);
            bw.Write(Encoding.ASCII.GetBytes("EOF "));
            fw.Dispose();
            bw.Dispose();
        }
    }
}