using ArcFormats.Templates;
using Log;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Utility;

namespace ArcFormats.Majiro
{
    public class ARC
    {
        public static UserControl PackExtraOptions = new VersionOnly("1/2/3");

        private static readonly string Magic = "MajiroArcV";

        private static readonly string magicV1 = "MajiroArcV1.000\x00";

        private static readonly string magicV2 = "MajiroArcV2.000\x00";

        private static readonly string magicV3 = "MajiroArcV3.000\x00";

        private static int UnpackVersion;

        private static int PackVersion;

        private struct Entry
        {
            internal string name;
            internal uint dataOffset;
            internal uint size;
        }

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            string magic = Encoding.ASCII.GetString(br.ReadBytes(10));
            UnpackVersion = br.ReadByte() - '0';
            fs.Dispose();
            br.Dispose();
            if (magic == Magic)
            {
                LogUtility.ShowVersion("arc", UnpackVersion);
                switch (UnpackVersion)
                {
                    case 1:
                        arcV1_unpack(filePath, folderPath);
                        break;
                    case 2:
                    case 3:
                        arcV2_unpack(filePath, folderPath);
                        break;
                }
            }
            else
            {
                LogUtility.ErrorInvalidArchive();
            }
        }

        private static void arcV1_unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            fs.Position = 16;
            int fileCount = br.ReadInt32();
            uint nameOffset = br.ReadUInt32();
            uint dataOffset = br.ReadUInt32();

            uint indexLength = 8 * (uint)fileCount + 8;
            MemoryStream ms = new MemoryStream(br.ReadBytes((int)indexLength));
            BinaryReader brIndex = new BinaryReader(ms);

            List<Entry> entries = new List<Entry>();
            Directory.CreateDirectory(folderPath);
            LogUtility.InitBar(fileCount);

            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new Entry();
                brIndex.ReadBytes(4);            //skip crc32
                entry.dataOffset = brIndex.ReadUInt32();
                entry.name = Utilities.ReadCString(br, ArcEncoding.Shift_JIS);
                entries.Add(entry);
            }
            Entry lastEntry = new Entry();
            brIndex.ReadBytes(4);               //skip crc32:0x00000000
            lastEntry.dataOffset = brIndex.ReadUInt32();
            entries.Add(lastEntry);

            for (int i = 0; i < fileCount; i++)
            {
                File.WriteAllBytes(folderPath + "\\" + entries[i].name, br.ReadBytes((int)(entries[i + 1].dataOffset - entries[i].dataOffset)));
                LogUtility.UpdateBar();
            }

            fs.Dispose();
            br.Dispose();
            brIndex.Dispose();
            ms.Dispose();
        }

        private static void arcV2_unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            fs.Position = 16;
            int fileCount = br.ReadInt32();
            uint nameOffset = br.ReadUInt32();
            uint dataOffset = br.ReadUInt32();

            uint indexLength = (uint)((UnpackVersion + 1) * 4 * fileCount);
            MemoryStream ms = new MemoryStream(br.ReadBytes((int)indexLength));
            BinaryReader brIndex = new BinaryReader(ms);

            Directory.CreateDirectory(folderPath);
            LogUtility.InitBar(fileCount);

            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new Entry();
                brIndex.ReadBytes(4 * (UnpackVersion - 1));            //skip checksum
                entry.dataOffset = brIndex.ReadUInt32();
                entry.size = brIndex.ReadUInt32();
                entry.name = Utilities.ReadCString(br, ArcEncoding.Shift_JIS);
                long pos = fs.Position;
                fs.Position = entry.dataOffset;
                File.WriteAllBytes(Path.Combine(folderPath, entry.name), br.ReadBytes((int)entry.size));
                fs.Position = pos;
                LogUtility.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
            brIndex.Dispose();
            ms.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            PackVersion = int.Parse(Global.Version);
            switch (PackVersion)
            {
                case 1:
                    arcV1_pack(folderPath, filePath);
                    break;
                case 2:
                case 3:
                    arcV2_pack(folderPath, filePath);
                    break;
            }
        }

        private static void arcV1_pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            int fileCount = Utilities.GetFileCount_TopOnly(folderPath);
            LogUtility.InitBar(fileCount);
            bw.Write(Encoding.ASCII.GetBytes(magicV1));
            bw.Write(fileCount);
            uint nameOffset = 28 + 8 * ((uint)fileCount + 1);
            uint dataOffset = 0;
            bw.Write(nameOffset);
            bw.Write(dataOffset);       // pos = 24

            // write name
            bw.BaseStream.Position = nameOffset;
            string[] files = Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < fileCount; i++)
            {
                bw.Write(ArcEncoding.Shift_JIS.GetBytes(Path.GetFileName(files[i])));
                bw.Write('\0');
            }
            // write data
            dataOffset = (uint)fw.Position;
            for (int i = 0; i < fileCount; i++)
            {
                bw.Write(File.ReadAllBytes(files[i]));
            }
            uint maxOffset = (uint)fw.Position;
            // write index
            bw.BaseStream.Position = 24;
            bw.Write(dataOffset);
            for (int i = 0; i < fileCount; i++)
            {
                bw.Write(Crc32.Calculate(ArcEncoding.Shift_JIS.GetBytes(Path.GetFileName(files[i]))));
                bw.Write(dataOffset);
                dataOffset += (uint)new FileInfo(files[i]).Length;
                LogUtility.UpdateBar();
            }
            bw.Write(0);
            bw.Write(maxOffset);
            bw.Dispose();
            fw.Dispose();
        }

        private static void arcV2_pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            int fileCount = Utilities.GetFileCount_TopOnly(folderPath);
            LogUtility.InitBar(fileCount);
            bw.Write(Encoding.ASCII.GetBytes(PackVersion == 2 ? magicV2 : magicV3));
            bw.Write(fileCount);
            uint nameOffset = 28 + (uint)((PackVersion + 1) * 4 * fileCount);
            uint dataOffset = 0;
            bw.Write(nameOffset);
            bw.Write(dataOffset);       // pos = 24

            // write name
            bw.BaseStream.Position = nameOffset;
            string[] files = Directory.GetFiles(folderPath);
            for (int i = 0; i < fileCount; i++)
            {
                bw.Write(ArcEncoding.Shift_JIS.GetBytes(Path.GetFileName(files[i])));
                bw.Write('\0');
            }
            // write data
            dataOffset = (uint)fw.Position;
            for (int i = 0; i < fileCount; i++)
            {
                bw.Write(File.ReadAllBytes(files[i]));
            }
            // write index
            bw.BaseStream.Position = 24;
            bw.Write(dataOffset);
            for (int i = 0; i < fileCount; i++)
            {
                if (PackVersion == 2)
                {
                    bw.Write(Crc32.Calculate(ArcEncoding.Shift_JIS.GetBytes(Path.GetFileName(files[i]))));
                }
                else
                {
                    bw.Write((long)0);      // don't know what this 8 bytes are , set to 0
                }
                bw.Write(dataOffset);
                uint fileSize = (uint)new FileInfo(files[i]).Length;
                bw.Write(fileSize);
                dataOffset += fileSize;
                LogUtility.UpdateBar();
            }
            bw.Dispose();
            fw.Dispose();
        }
    }
}