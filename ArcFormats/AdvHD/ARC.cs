using Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utility;

namespace ArcFormats.AdvHD
{
    public class ARC
    {
        struct AdvHD_arc_v1_header
        {
            public uint typeCount { get; set; }
            public uint fileCountAll { get; set; }
        }
        struct AdvHD_arc_v1_type_header
        {
            public string ext { get; set; }
            public uint fileCount { get; set; }
            public uint indexOffset { get; set; }
        }
        struct AdvHD_arc_v1_entry
        {
            public string fileName { get; set; }
            public uint fileSize { get; set; }
            public uint offset { get; set; }
            public string filePath { get; set; }
        }
        private static void arc_v1_unpack(string filePath, string folderPath)
        {
            AdvHD_arc_v1_header header = new AdvHD_arc_v1_header();
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            header.fileCountAll = 0;
            header.typeCount = br.ReadUInt32();
            List<AdvHD_arc_v1_type_header> typeHeaders = new List<AdvHD_arc_v1_type_header>();
            Directory.CreateDirectory(folderPath);

            for (int i = 0; i < header.typeCount; i++)
            {
                AdvHD_arc_v1_type_header typeHeader = new AdvHD_arc_v1_type_header();
                typeHeader.ext = Encoding.ASCII.GetString(br.ReadBytes(3));
                br.ReadByte();
                typeHeader.fileCount = br.ReadUInt32();
                typeHeader.indexOffset = br.ReadUInt32();
                typeHeaders.Add(typeHeader);
                header.fileCountAll += typeHeader.fileCount;
            }
            LogUtility.InitBar((int)header.fileCountAll);

            for (int i = 0; i < header.typeCount; i++)
            {
                for (int j = 0; j < typeHeaders[i].fileCount; j++)
                {
                    AdvHD_arc_v1_entry index = new AdvHD_arc_v1_entry();
                    index.fileName = Encoding.ASCII.GetString(br.ReadBytes(13)).Replace("\0", string.Empty);

                    index.fileSize = br.ReadUInt32();
                    index.offset = br.ReadUInt32();
                    long pos = fs.Position;
                    index.filePath = folderPath + "\\" + index.fileName + "." + typeHeaders[i].ext;
                    FileStream fw = new FileStream(index.filePath, FileMode.Create, FileAccess.Write);
                    fs.Seek(index.offset, SeekOrigin.Begin);
                    byte[] buffer = br.ReadBytes((int)index.fileSize);
                    fw.Write(buffer, 0, buffer.Length);
                    fw.Close();
                    fs.Seek(pos, SeekOrigin.Begin);
                    LogUtility.UpdateBar();
                }
            }
            fs.Dispose();
        }
        private static void arc_v1_pack(string folderPath, string filePath)
        {
            HashSet<string> uniqueExtension = new HashSet<string>();

            AdvHD_arc_v1_header header = new AdvHD_arc_v1_header();
            header.fileCountAll = (uint)Utilities.GetFileCount_All(folderPath);
            LogUtility.InitBar((int)header.fileCountAll);
            string[] ext = Utilities.GetFileExtensions(folderPath);
            Utilities.InsertSort(ext);
            int extCount = ext.Length;

            header.typeCount = (uint)extCount;
            int length = 13;
            DirectoryInfo d = new DirectoryInfo(folderPath);
            List<AdvHD_arc_v1_type_header> typeHeaders = new List<AdvHD_arc_v1_type_header>();
            int[] type_fileCount = new int[extCount];

            FileStream fw = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            MemoryStream mshead = new MemoryStream();
            MemoryStream mstype = new MemoryStream();
            MemoryStream msentry = new MemoryStream();
            MemoryStream msdata = new MemoryStream();
            BinaryWriter bwhead = new BinaryWriter(mshead);
            BinaryWriter bwtype = new BinaryWriter(mstype);
            BinaryWriter bwentry = new BinaryWriter(msentry);
            BinaryWriter bwdata = new BinaryWriter(msdata);

            bwhead.Write(header.typeCount);
            uint pos = (uint)(12 * extCount + 4);

            for (int i = 0; i < extCount; i++)
            {
                foreach (FileInfo file in d.GetFiles("*" + ext[i]))
                {
                    type_fileCount[i]++;
                    bwentry.Write(Encoding.ASCII.GetBytes(file.Name.Replace("." + ext[i], string.Empty).PadRight(length, '\0')));
                    bwentry.Write((uint)file.Length);
                    bwentry.Write((uint)(4 + 12 * header.typeCount + 21 * header.fileCountAll + msdata.Length));
                    bwdata.Write(File.ReadAllBytes(file.FullName));
                    LogWindow.Instance.bar.PerformStep();
                }
                bwtype.Write(Encoding.ASCII.GetBytes(ext[i]));
                bwtype.Write((byte)0);
                bwtype.Write((uint)type_fileCount[i]);
                bwtype.Write(pos);
                pos = (uint)(12 * extCount + 4 + msentry.Length);
                LogUtility.UpdateBar();
            }
            mshead.WriteTo(fw);
            mstype.WriteTo(fw);
            msentry.WriteTo(fw);
            msdata.WriteTo(fw);

            mshead.Dispose();
            mstype.Dispose();
            msentry.Dispose();
            msdata.Dispose();
            fw.Dispose();
        }

        struct AdvHD_arc_v2_header
        {
            public uint fileCount { get; set; }
            public uint entrySize { get; set; }
        }
        struct AdvHD_arc_v2_entry
        {
            public uint fileSize { get; set; }
            public uint offset { get; set; }
            public string fileName { get; set; }
            public string filePath { get; set; }
        }
        private static void arc_v2_unpack(string filePath, string folderPath)
        {
            //init
            AdvHD_arc_v2_header header = new AdvHD_arc_v2_header();
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader br1 = new BinaryReader(fs);
            List<AdvHD_arc_v2_entry> l = new List<AdvHD_arc_v2_entry>();

            Directory.CreateDirectory(folderPath);

            header.fileCount = br1.ReadUInt32();
            header.entrySize = br1.ReadUInt32();
            LogUtility.InitBar((int)header.fileCount);

            for (int i = 0; i < header.fileCount; i++)
            {
                AdvHD_arc_v2_entry entry = new AdvHD_arc_v2_entry();
                entry.fileSize = br1.ReadUInt32();
                entry.offset = br1.ReadUInt32() + 8 + header.entrySize;
                entry.fileName = Utilities.ReadUntil_Unicode(br1);
                entry.filePath = folderPath + "\\" + entry.fileName;

                l.Add(entry);
            }

            for (int i = 0; i < header.fileCount; i++)
            {
                FileStream fw = new FileStream(l[i].filePath, FileMode.Create, FileAccess.Write);
                byte[] buffer = br1.ReadBytes((int)l[i].fileSize);
                fw.Write(buffer, 0, buffer.Length);
                fw.Dispose();
                LogUtility.UpdateBar();
            }
            fs.Dispose();
            br1.Dispose();
        }
        private static void arc_v2_pack(string folderPath, string filePath)
        {
            AdvHD_arc_v2_header header = new AdvHD_arc_v2_header();
            List<AdvHD_arc_v2_entry> l = new List<AdvHD_arc_v2_entry>();
            uint sizeToNow = 0;

            //make header
            DirectoryInfo d = new DirectoryInfo(folderPath);
            header.fileCount = (uint)Utilities.GetFileCount_All(folderPath);
            header.entrySize = 0;
            LogUtility.InitBar((int)header.fileCount);

            foreach (FileInfo fi in d.GetFiles())
            {
                int nameLength = fi.Name.Length;
                header.entrySize = header.entrySize + (uint)nameLength * 2 + 2 + 8;
            }

            //make entry
            foreach (FileInfo fi in d.GetFiles())
            {
                AdvHD_arc_v2_entry entry = new AdvHD_arc_v2_entry();
                entry.fileName = fi.Name;
                entry.fileSize = (uint)new FileInfo(fi.FullName).Length;
                entry.offset = sizeToNow;
                sizeToNow += entry.fileSize;
                l.Add(entry);
            }

            //write init
            FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);

            //write header
            bw.Write(header.fileCount);
            bw.Write(header.entrySize);

            //write entry
            foreach (var file in l)
            {
                bw.Write(file.fileSize);
                bw.Write(file.offset);
                bw.Write(Encoding.Unicode.GetBytes(file.fileName));
                bw.Write('\0');
                bw.Write('\0');
            }

            //write data
            foreach (FileInfo fi in d.GetFiles())
            {
                byte[] buffer = File.ReadAllBytes(fi.FullName);
                bw.Write(buffer);
                LogUtility.UpdateBar();
            }
            fs.Dispose();
            bw.Dispose();
        }

        public static void Unpack(string filePath, string folderPath, Encoding encoding)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            br.ReadBytes(6);
            char a = br.ReadChar();
            if (a >= 'A')//extension
            {
                LogUtility.Info("Valid arc v1 archive detected.");
                arc_v1_unpack(filePath, folderPath);
            }
            else
            {
                LogUtility.Info("Valid arc v2 archive detected.");
                arc_v2_unpack(filePath, folderPath);
            }
        }

        public static void Pack(string folderPath, string filePath, string version, Encoding encoding)
        {
            if (version == "1")
            {
                arc_v1_pack(folderPath, filePath);
            }
            else
            {
                arc_v2_pack(folderPath, filePath);
            }
        }
    }
}
