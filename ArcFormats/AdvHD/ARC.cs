using ArcFormats.Properties;
using Log;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Utility;
using Utility.Extensions;

namespace ArcFormats.AdvHD
{
    public class ARC
    {
        public static UserControl UnpackExtraOptions = new UnpackARCOptions();

        public static UserControl PackExtraOptions = new PackARCOptions();

        private class HeaderV1
        {
            public uint typeCount { get; set; }
            public uint fileCountAll { get; set; }
        }

        private class TypeHeaderV1
        {
            public string extension { get; set; }
            public uint fileCount { get; set; }
            public uint indexOffset { get; set; }
        }

        private class EntryV1
        {
            public string fileName { get; set; }
            public uint fileSize { get; set; }
            public uint offset { get; set; }
            public string filePath { get; set; }
        }

        private static void arcV1_unpack(string filePath, string folderPath)
        {
            HeaderV1 header = new HeaderV1();
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            header.fileCountAll = 0;
            header.typeCount = br.ReadUInt32();
            List<TypeHeaderV1> typeHeaders = new List<TypeHeaderV1>();
            Directory.CreateDirectory(folderPath);

            for (int i = 0; i < header.typeCount; i++)
            {
                TypeHeaderV1 typeHeader = new TypeHeaderV1();
                typeHeader.extension = Encoding.ASCII.GetString(br.ReadBytes(3));
                br.ReadByte();
                typeHeader.fileCount = br.ReadUInt32();
                typeHeader.indexOffset = br.ReadUInt32();
                typeHeaders.Add(typeHeader);
                header.fileCountAll += typeHeader.fileCount;
            }
            LogUtility.InitBar(header.fileCountAll);

            for (int i = 0; i < header.typeCount; i++)
            {
                for (int j = 0; j < typeHeaders[i].fileCount; j++)
                {
                    EntryV1 index = new EntryV1();
                    index.fileName = Encoding.ASCII.GetString(br.ReadBytes(13)).Replace("\0", string.Empty) + "." + typeHeaders[i].extension;
                    index.fileSize = br.ReadUInt32();
                    index.offset = br.ReadUInt32();
                    long pos = fs.Position;
                    index.filePath = Path.Combine(folderPath, index.fileName);
                    fs.Seek(index.offset, SeekOrigin.Begin);
                    byte[] buffer = br.ReadBytes((int)index.fileSize);
                    if (UnpackARCOptions.toDecryptScripts && IsScriptFile(Path.GetExtension(index.filePath), "1"))
                    {
                        LogUtility.Debug(string.Format(Resources.logTryDecScr, index.fileName));
                        DecryptScript(buffer);
                    }
                    File.WriteAllBytes(index.filePath, buffer);
                    fs.Seek(pos, SeekOrigin.Begin);
                    LogUtility.UpdateBar();
                }
            }
            fs.Dispose();
            br.Dispose();
        }

        private static void arcV1_pack(string folderPath, string filePath)
        {
            HashSet<string> uniqueExtension = new HashSet<string>();

            HeaderV1 header = new HeaderV1();
            header.fileCountAll = (uint)Utilities.GetFileCount(folderPath);
            LogUtility.InitBar(header.fileCountAll);
            string[] exts = Utilities.GetFileExtensions(folderPath);
            Utilities.InsertSort(exts);
            int extCount = exts.Length;

            header.typeCount = (uint)extCount;
            int length = 13;
            DirectoryInfo d = new DirectoryInfo(folderPath);
            List<TypeHeaderV1> typeHeaders = new List<TypeHeaderV1>();
            int[] type_fileCount = new int[extCount];

            FileStream fw = File.Create(filePath);
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
                foreach (FileInfo file in d.GetFiles("*" + exts[i]))
                {
                    type_fileCount[i]++;
                    bwentry.Write(Encoding.ASCII.GetBytes(file.Name.Replace("." + exts[i], string.Empty).PadRight(length, '\0')));
                    bwentry.Write((uint)file.Length);
                    bwentry.Write((uint)(4 + 12 * header.typeCount + 21 * header.fileCountAll + msdata.Length));
                    byte[] buffer = File.ReadAllBytes(file.FullName);
                    if (PackARCOptions.toEncryptScripts && IsScriptFile(exts[i], "1"))
                    {
                        LogUtility.Debug(string.Format(Resources.logTryEncScr, file.Name));
                        EncryptScript(buffer);
                    }
                    bwdata.Write(buffer);
                    LogUtility.UpdateBar();
                }
                bwtype.Write(Encoding.ASCII.GetBytes(exts[i]));
                bwtype.Write((byte)0);
                bwtype.Write((uint)type_fileCount[i]);
                bwtype.Write(pos);
                pos = (uint)(12 * extCount + 4 + msentry.Length);
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
            bwhead.Dispose();
            bwtype.Dispose();
            bwentry.Dispose();
            bwdata.Dispose();
        }

        private class HeaderV2
        {
            public uint fileCount { get; set; }
            public uint entrySize { get; set; }
        }

        private class EntryV2
        {
            public uint fileSize { get; set; }
            public uint offset { get; set; }
            public string fileName { get; set; }
            public string filePath { get; set; }
        }

        private static void arcV2_unpack(string filePath, string folderPath)
        {
            //init
            HeaderV2 header = new HeaderV2();
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br1 = new BinaryReader(fs);
            List<EntryV2> l = new List<EntryV2>();

            Directory.CreateDirectory(folderPath);

            header.fileCount = br1.ReadUInt32();
            header.entrySize = br1.ReadUInt32();
            LogUtility.InitBar(header.fileCount);

            for (int i = 0; i < header.fileCount; i++)
            {
                EntryV2 entry = new EntryV2();
                entry.fileSize = br1.ReadUInt32();
                entry.offset = br1.ReadUInt32() + 8 + header.entrySize;
                entry.fileName = br1.ReadCString(Encoding.Unicode);
                entry.filePath = Path.Combine(folderPath, entry.fileName);

                l.Add(entry);
            }

            foreach (var entry in l)
            {
                byte[] buffer = br1.ReadBytes((int)entry.fileSize);
                if (UnpackARCOptions.toDecryptScripts && IsScriptFile(Path.GetExtension(entry.filePath), "2"))
                {
                    LogUtility.Debug(string.Format(Resources.logTryDecScr, entry.fileName));
                    DecryptScript(buffer);
                }
                File.WriteAllBytes(entry.filePath, buffer);
                LogUtility.UpdateBar();
            }
            fs.Dispose();
            br1.Dispose();
        }

        private static void arcV2_pack(string folderPath, string filePath)
        {
            HeaderV2 header = new HeaderV2();
            List<EntryV2> l = new List<EntryV2>();
            uint sizeToNow = 0;

            //make header
            DirectoryInfo d = new DirectoryInfo(folderPath);
            FileInfo[] files = d.GetFiles();
            header.fileCount = (uint)files.Length;
            header.entrySize = 0;
            LogUtility.InitBar(header.fileCount);

            foreach (FileInfo file in files)
            {
                EntryV2 entry = new EntryV2();
                entry.fileName = file.Name;
                entry.fileSize = (uint)file.Length;
                entry.offset = sizeToNow;
                sizeToNow += entry.fileSize;
                l.Add(entry);

                int nameLength = entry.fileName.Length;
                header.entrySize = header.entrySize + (uint)nameLength * 2 + 2 + 8;
            }

            using (FileStream fs = File.Create(filePath))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
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
                    foreach (FileInfo file in files)
                    {
                        byte[] buffer = File.ReadAllBytes(file.FullName);
                        if (PackARCOptions.toEncryptScripts && IsScriptFile(file.Extension, "2"))
                        {
                            LogUtility.Debug(string.Format(Resources.logTryEncScr, file.Name));
                            EncryptScript(buffer);
                        }
                        bw.Write(buffer);
                        LogUtility.UpdateBar();
                    }
                }
            }
        }

        public void Unpack(string filePath, string folderPath)
        {
            char a;
            using (FileStream fs = File.OpenRead(filePath))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    fs.Position = 6;
                    a = br.ReadChar();
                }
            }

            if (a >= 'A')   //extension
            {
                LogUtility.ShowVersion("arc", 1);
                arcV1_unpack(filePath, folderPath);
            }
            else
            {
                LogUtility.ShowVersion("arc", 2);
                arcV2_unpack(filePath, folderPath);
            }
        }

        public void Pack(string folderPath, string filePath)
        {
            if (Global.Version == "1")
            {
                arcV1_pack(folderPath, filePath);
            }
            else
            {
                arcV2_pack(folderPath, filePath);
            }
        }

        private static bool IsScriptFile(string ext, string version)
        {
            string trimed = ext.TrimStart('.').ToLower();
            if (version == "1")
            {
                return trimed == "scr" || trimed == "wsc";
            }
            else if (version == "2")
            {
                return trimed == "json" || trimed == "ws2";
            }
            return false;
        }

        private static void DecryptScript(byte[] data)
        {
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = Binary.RotByteR(data[i], 2);
            }
        }

        private static void EncryptScript(byte[] data)
        {
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = Binary.RotByteL(data[i], 2);
            }
        }
    }
}