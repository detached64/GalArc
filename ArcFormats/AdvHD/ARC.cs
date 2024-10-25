using ArcFormats.Properties;
using Log;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private static readonly string[] EncryptedFileExtV1 = { "wsc", "scr" };

        private static readonly string[] EncryptedFileExtV2 = { "ws2", "json" };

        private class HeaderV1
        {
            public uint TypeCount { get; set; }
            public uint FileCountAll { get; set; }
        }

        private class TypeHeaderV1
        {
            public string Extension { get; set; }
            public uint FileCount { get; set; }
            public uint IndexOffset { get; set; }
        }

        private class EntryV1
        {
            public string FileName { get; set; }
            public uint FileSize { get; set; }
            public uint Offset { get; set; }
            public string FilePath { get; set; }
        }

        private static void arcV1_unpack(string filePath, string folderPath)
        {
            HeaderV1 header = new HeaderV1();
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            header.FileCountAll = 0;
            header.TypeCount = br.ReadUInt32();
            List<TypeHeaderV1> typeHeaders = new List<TypeHeaderV1>();
            Directory.CreateDirectory(folderPath);

            for (int i = 0; i < header.TypeCount; i++)
            {
                TypeHeaderV1 typeHeader = new TypeHeaderV1();
                typeHeader.Extension = Encoding.ASCII.GetString(br.ReadBytes(3));
                br.ReadByte();
                typeHeader.FileCount = br.ReadUInt32();
                typeHeader.IndexOffset = br.ReadUInt32();
                typeHeaders.Add(typeHeader);
                header.FileCountAll += typeHeader.FileCount;
            }
            LogUtility.InitBar(header.FileCountAll);

            for (int i = 0; i < header.TypeCount; i++)
            {
                for (int j = 0; j < typeHeaders[i].FileCount; j++)
                {
                    EntryV1 index = new EntryV1();
                    index.FileName = Encoding.ASCII.GetString(br.ReadBytes(13)).Replace("\0", string.Empty) + "." + typeHeaders[i].Extension;
                    index.FileSize = br.ReadUInt32();
                    index.Offset = br.ReadUInt32();
                    long pos = fs.Position;
                    index.FilePath = Path.Combine(folderPath, index.FileName);
                    fs.Seek(index.Offset, SeekOrigin.Begin);
                    byte[] buffer = br.ReadBytes((int)index.FileSize);
                    if (UnpackARCOptions.toDecryptScripts && IsScriptFile(Path.GetExtension(index.FilePath), "1"))
                    {
                        LogUtility.Debug(string.Format(Resources.logTryDecScr, index.FileName));
                        DecryptScript(buffer);
                    }
                    File.WriteAllBytes(index.FilePath, buffer);
                    buffer = null;
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
            header.FileCountAll = (uint)Utils.GetFileCount(folderPath);
            LogUtility.InitBar(header.FileCountAll);
            string[] exts = Utils.GetFileExtensions(folderPath);
            Utils.Sort(exts);
            int extCount = exts.Length;

            header.TypeCount = (uint)extCount;
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

            bwhead.Write(header.TypeCount);
            uint pos = (uint)(12 * extCount + 4);

            for (int i = 0; i < extCount; i++)
            {
                foreach (FileInfo file in d.GetFiles($"*{exts[i]}"))
                {
                    type_fileCount[i]++;
                    bwentry.Write(Encoding.ASCII.GetBytes(Path.GetFileNameWithoutExtension(file.Name).PadRight(length, '\0')));
                    bwentry.Write((uint)file.Length);
                    bwentry.Write((uint)(4 + 12 * header.TypeCount + 21 * header.FileCountAll + msdata.Length));
                    byte[] buffer = File.ReadAllBytes(file.FullName);
                    if (PackARCOptions.toEncryptScripts && IsScriptFile(exts[i], "1"))
                    {
                        LogUtility.Debug(string.Format(Resources.logTryEncScr, file.Name));
                        EncryptScript(buffer);
                    }
                    bwdata.Write(buffer);
                    buffer = null;
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
            public uint FileCount { get; set; }
            public uint EntrySize { get; set; }
        }

        private class EntryV2
        {
            public uint FileSize { get; set; }
            public uint Offset { get; set; }
            public string FileName { get; set; }
            public string FilePath { get; set; }
        }

        private static void arcV2_unpack(string filePath, string folderPath)
        {
            //init
            HeaderV2 header = new HeaderV2();
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br1 = new BinaryReader(fs);
            List<EntryV2> l = new List<EntryV2>();

            Directory.CreateDirectory(folderPath);

            header.FileCount = br1.ReadUInt32();
            header.EntrySize = br1.ReadUInt32();
            LogUtility.InitBar(header.FileCount);

            for (int i = 0; i < header.FileCount; i++)
            {
                EntryV2 entry = new EntryV2();
                entry.FileSize = br1.ReadUInt32();
                entry.Offset = br1.ReadUInt32() + 8 + header.EntrySize;
                entry.FileName = br1.ReadCString(Encoding.Unicode);
                entry.FilePath = Path.Combine(folderPath, entry.FileName);

                l.Add(entry);
            }

            foreach (var entry in l)
            {
                byte[] buffer = br1.ReadBytes((int)entry.FileSize);
                if (UnpackARCOptions.toDecryptScripts && IsScriptFile(Path.GetExtension(entry.FilePath), "2"))
                {
                    LogUtility.Debug(string.Format(Resources.logTryDecScr, entry.FileName));
                    DecryptScript(buffer);
                }
                File.WriteAllBytes(entry.FilePath, buffer);
                buffer = null;
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
            header.FileCount = (uint)files.Length;
            header.EntrySize = 0;
            LogUtility.InitBar(header.FileCount);

            foreach (FileInfo file in files)
            {
                EntryV2 entry = new EntryV2();
                entry.FileName = file.Name;
                entry.FileSize = (uint)file.Length;
                entry.Offset = sizeToNow;
                sizeToNow += entry.FileSize;
                l.Add(entry);

                int nameLength = entry.FileName.Length;
                header.EntrySize = header.EntrySize + (uint)nameLength * 2 + 2 + 8;
            }

            using (FileStream fs = File.Create(filePath))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    //write header
                    bw.Write(header.FileCount);
                    bw.Write(header.EntrySize);

                    //write entry
                    foreach (var file in l)
                    {
                        bw.Write(file.FileSize);
                        bw.Write(file.Offset);
                        bw.Write(Encoding.Unicode.GetBytes(file.FileName));
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
                        buffer = null;
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
            if (Config.Version == "1")
            {
                arcV1_pack(folderPath, filePath);
            }
            else
            {
                arcV2_pack(folderPath, filePath);
            }
        }

        private static bool IsScriptFile(string extension, string version)
        {
            string trimed = extension.TrimStart('.');
            switch (version)
            {
                case "1":
                    return EncryptedFileExtV1.Contains(trimed);
                case "2":
                    return EncryptedFileExtV2.Contains(trimed);
                default:
                    return false;
            }
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