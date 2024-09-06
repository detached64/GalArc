using Log;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Utility;

namespace ArcFormats.AmuseCraft
{
    public class PAC
    {
        private struct AmuseCraft_pac_entry
        {
            public string fileName { get; set; }
            public uint fileSize { get; set; }
            public uint offset { get; set; }
        }

        public static void Unpack(string filePath, string folderPath, Encoding encoding)
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
                AmuseCraft_pac_entry entry = new AmuseCraft_pac_entry();
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
        }
        public static void Pack(string folderPath, string filePath, string version, Encoding encoding)
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
        }
    }

}

