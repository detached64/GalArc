using Log;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility;

namespace ArcFormats.InnocentGrey
{
    public class IGA
    {
        private struct InnocentGrey_iga_header
        {
            public byte[] magic { get; set; }//IGA0
            public uint unknown1 { get; set; }//checksum?
            public uint unknown2 { get; set; }//2
            public uint unknown3 { get; set; }//2
        }

        private struct InnocentGrey_iga_entry
        {
            public uint nameOffset { get; set; }
            public uint dataOffset { get; set; }
            public uint fileSize { get; set; }
            public string fileName { get; set; }
            public uint nameLen { get; set; }
        }

        public static void Unpack(string filePath, string folderPath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            List<InnocentGrey_iga_entry> entries = new List<InnocentGrey_iga_entry>();
            List<InnocentGrey_iga_entry> entriesUpdate = new List<InnocentGrey_iga_entry>();
            if (Encoding.ASCII.GetString(br.ReadBytes(4)) != "IGA0")
            {
                LogUtility.Error_NotValidArchive();
            }

            fs.Position = 16;
            uint indexSize = Varint.UnpackUint(br);

            long endPos = fs.Position + indexSize;
            while (fs.Position < endPos)
            {
                var entry = new InnocentGrey_iga_entry();
                entry.nameOffset = Varint.UnpackUint(br);
                entry.dataOffset = Varint.UnpackUint(br);
                entry.fileSize = Varint.UnpackUint(br);
                entries.Add(entry);
            }

            LogUtility.InitBar(entries.Count);
            uint nameIndexSize = Varint.UnpackUint(br);
            long endName = fs.Position + nameIndexSize;

            Directory.CreateDirectory(folderPath);

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                uint nameLenThis;
                if (i + 1 < entries.Count)
                {
                    nameLenThis = entries[i + 1].nameOffset - entries[i].nameOffset;
                }
                else
                {
                    nameLenThis = nameIndexSize - entries[i].nameOffset;
                }

                entry.fileName = Varint.UnpackString(br, nameLenThis);
                entry.dataOffset += (uint)endName;
                entriesUpdate.Add(entry);
            }

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entriesUpdate[i];
                fs.Position = entry.dataOffset;
                byte[] buffer = new byte[entry.fileSize];
                br.Read(buffer, 0, (int)entry.fileSize);
                int key = Path.GetExtension(entry.fileName) == ".s" ? 0xFF : 0;//decrypt script file,详见garbro
                for (uint j = 0; j < entry.fileSize; j++)
                {
                    buffer[j] ^= (byte)((j + 2) ^ key);
                }
                File.WriteAllBytes(folderPath + "\\" + entry.fileName, buffer);
                LogUtility.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        public static void Pack(string folderPath, string filePath)
        {
            FileStream fw = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fw);
            InnocentGrey_iga_header header = new InnocentGrey_iga_header();
            header.magic = Encoding.ASCII.GetBytes("IGA0");
            bw.Write(header.magic);
            bw.Write(0);//don't know accurate value,but 0 seems valid
            bw.Write(2);
            bw.Write(2);
            List<InnocentGrey_iga_entry> l = new List<InnocentGrey_iga_entry>();

            DirectoryInfo dir = new DirectoryInfo(folderPath);
            uint nameOffset = 0;
            uint dataOffset = 0;
            foreach (FileInfo file in dir.GetFiles("*.*", searchOption: SearchOption.TopDirectoryOnly))
            {
                InnocentGrey_iga_entry entry = new InnocentGrey_iga_entry();
                entry.fileName = file.Name;
                entry.nameLen = (uint)entry.fileName.Length;
                entry.nameOffset = nameOffset;
                entry.dataOffset = dataOffset;
                entry.fileSize = (uint)file.Length;
                l.Add(entry);
                nameOffset += entry.nameLen;
                dataOffset += entry.fileSize;
            }

            MemoryStream msEntry = new MemoryStream();
            BinaryWriter bwEntry = new BinaryWriter(msEntry);
            MemoryStream msFileName = new MemoryStream();
            BinaryWriter bwFileName = new BinaryWriter(msFileName);
            int fileCount = Utilities.GetFileCount_All(folderPath);
            LogUtility.InitBar(fileCount);

            for (int i = 0; i < fileCount; i++)
            {
                bwEntry.Write(Varint.PackUint(l[i].nameOffset));
                bwEntry.Write(Varint.PackUint(l[i].dataOffset));
                bwEntry.Write(Varint.PackUint(l[i].fileSize));
                bwFileName.Write(Varint.PackString(l[i].fileName));
            }
            bw.Write(Varint.PackUint((uint)msEntry.Length));
            msEntry.WriteTo(fw);

            bw.Write(Varint.PackUint((uint)msFileName.Length));
            msFileName.WriteTo(fw);

            for (int i = 0; i < l.Count; i++)
            {
                byte[] buffer = File.ReadAllBytes(folderPath + "\\" + l[i].fileName);
                int key = Path.GetExtension(l[i].fileName) == ".s" ? 0xFF : 0;
                for (uint j = 0; j < l[i].fileSize; j++)
                {
                    buffer[j] ^= (byte)((j + 2) ^ key);
                }
                bw.Write(buffer);
                LogUtility.UpdateBar();
            }

            fw.Dispose();
        }
    }
}