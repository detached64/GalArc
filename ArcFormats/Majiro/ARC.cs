using ArcFormats.Templates;
using GalArc.Logs;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Utility;
using Utility.Extensions;

namespace ArcFormats.Majiro
{
    public class ARC
    {
        public static UserControl PackExtraOptions = new VersionOnly("1/2/3");

        private static readonly string Magic = "MajiroArcV";

        private static readonly string MagicV1 = "MajiroArcV1.000\x00";

        private static readonly string MagicV2 = "MajiroArcV2.000\x00";

        private static readonly string MagicV3 = "MajiroArcV3.000\x00";

        private static int UnpackVersion;

        private static int PackVersion;

        private class Entry
        {
            internal string Name { get; set; }
            internal uint Offset { get; set; }
            internal uint Size { get; set; }
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
                Logger.ShowVersion("arc", UnpackVersion);
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
                Logger.ErrorInvalidArchive();
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
            Logger.InitBar(fileCount);

            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new Entry();
                brIndex.ReadBytes(4);            //skip crc32
                entry.Offset = brIndex.ReadUInt32();
                entry.Name = br.ReadCString(ArcEncoding.Shift_JIS);
                entries.Add(entry);
            }
            Entry lastEntry = new Entry();
            brIndex.ReadBytes(4);               //skip crc32:0x00000000
            lastEntry.Offset = brIndex.ReadUInt32();
            entries.Add(lastEntry);

            for (int i = 0; i < fileCount; i++)
            {
                byte[] data = br.ReadBytes((int)(entries[i + 1].Offset - entries[i].Offset));
                File.WriteAllBytes(Path.Combine(folderPath, entries[i].Name), data);
                data = null;
                Logger.UpdateBar();
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
            Logger.InitBar(fileCount);

            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new Entry();
                brIndex.ReadBytes(4 * (UnpackVersion - 1));            //skip checksum
                entry.Offset = brIndex.ReadUInt32();
                entry.Size = brIndex.ReadUInt32();
                entry.Name = br.ReadCString(ArcEncoding.Shift_JIS);
                long pos = fs.Position;
                fs.Position = entry.Offset;
                byte[] data = br.ReadBytes((int)entry.Size);
                File.WriteAllBytes(Path.Combine(folderPath, entry.Name), data);
                data = null;
                fs.Position = pos;
                Logger.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
            brIndex.Dispose();
            ms.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            PackVersion = int.Parse(Config.Version);
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
            DirectoryInfo d = new DirectoryInfo(folderPath);
            FileInfo[] files = d.GetFiles();
            int fileCount = files.Length;
            Logger.InitBar(fileCount);
            bw.Write(Encoding.ASCII.GetBytes(MagicV1));
            bw.Write(fileCount);
            uint nameOffset = 28 + 8 * ((uint)fileCount + 1);
            uint dataOffset = 0;
            bw.Write(nameOffset);
            bw.Write(dataOffset);       // pos = 24

            // write name
            bw.BaseStream.Position = nameOffset;
            foreach (FileInfo file in files)
            {
                bw.Write(ArcEncoding.Shift_JIS.GetBytes(file.Name));
                bw.Write('\0');
            }
            // write data
            dataOffset = (uint)fw.Position;
            foreach (FileInfo file in files)
            {
                byte[] data = File.ReadAllBytes(file.FullName);
                bw.Write(data);
                data = null;
            }
            uint maxOffset = (uint)fw.Position;
            // write index
            bw.BaseStream.Position = 24;
            bw.Write(dataOffset);
            foreach (FileInfo file in files)
            {
                bw.Write(Crc32.Calculate(ArcEncoding.Shift_JIS.GetBytes(file.Name)));
                bw.Write(dataOffset);
                dataOffset += (uint)file.Length;
                Logger.UpdateBar();
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
            DirectoryInfo d = new DirectoryInfo(folderPath);
            FileInfo[] files = d.GetFiles();
            int fileCount = files.Length;
            Logger.InitBar(fileCount);

            bw.Write(Encoding.ASCII.GetBytes(PackVersion == 2 ? MagicV2 : MagicV3));
            bw.Write(fileCount);
            uint nameOffset = 28 + (uint)((PackVersion + 1) * 4 * fileCount);
            uint dataOffset = 0;
            bw.Write(nameOffset);
            bw.Write(dataOffset);       // pos = 24

            // write name
            bw.BaseStream.Position = nameOffset;
            foreach (FileInfo file in files)
            {
                bw.Write(ArcEncoding.Shift_JIS.GetBytes(file.Name));
                bw.Write('\0');
            }
            // write data
            dataOffset = (uint)fw.Position;
            foreach (FileInfo file in files)
            {
                byte[] data = File.ReadAllBytes(file.FullName);
                bw.Write(data);
                data = null;
            }
            // write index
            bw.BaseStream.Position = 24;
            bw.Write(dataOffset);
            foreach (FileInfo file in files)
            {
                if (PackVersion == 2)
                {
                    bw.Write(Crc32.Calculate(ArcEncoding.Shift_JIS.GetBytes(file.Name)));
                }
                else
                {
                    bw.Write((long)0);      // don't know what this 8 bytes are , set to 0
                }
                bw.Write(dataOffset);
                uint fileSize = (uint)file.Length;
                bw.Write(fileSize);
                dataOffset += fileSize;
                Logger.UpdateBar();
            }
            bw.Dispose();
            fw.Dispose();
        }
    }
}