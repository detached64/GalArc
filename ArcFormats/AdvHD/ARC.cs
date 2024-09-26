using Log;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Utility;

namespace ArcFormats.AdvHD
{
    public class ARC
    {
        public static UserControl PackExtraOptions = new Templates.VersionOnly("1/2");

        private struct HeaderV1
        {
            public uint typeCount { get; set; }
            public uint fileCountAll { get; set; }
        }

        private struct TypeHeaderV1
        {
            public string ext { get; set; }
            public uint fileCount { get; set; }
            public uint indexOffset { get; set; }
        }

        private struct EntryV1
        {
            public string fileName { get; set; }
            public uint fileSize { get; set; }
            public uint offset { get; set; }
            public string filePath { get; set; }
        }

        private static void arcV1_unpack(string filePath, string folderPath)
        {
            HeaderV1 header = new HeaderV1();
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            header.fileCountAll = 0;
            header.typeCount = br.ReadUInt32();
            List<TypeHeaderV1> typeHeaders = new List<TypeHeaderV1>();
            Directory.CreateDirectory(folderPath);

            for (int i = 0; i < header.typeCount; i++)
            {
                TypeHeaderV1 typeHeader = new TypeHeaderV1();
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
                    EntryV1 index = new EntryV1();
                    index.fileName = Encoding.ASCII.GetString(br.ReadBytes(13)).Replace("\0", string.Empty) + "." + typeHeaders[i].ext;
                    index.fileSize = br.ReadUInt32();
                    index.offset = br.ReadUInt32();
                    long pos = fs.Position;
                    index.filePath = Path.Combine(folderPath, index.fileName);
                    fs.Seek(index.offset, SeekOrigin.Begin);
                    byte[] buffer = br.ReadBytes((int)index.fileSize);
                    if (Global.ToDecryptScript && IsScriptFile(Path.GetExtension(index.filePath), "1"))
                    {
                        LogUtility.Debug("Try to decrypt script:" + index.fileName);
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
            header.fileCountAll = (uint)Utilities.GetFileCount_All(folderPath);
            LogUtility.InitBar((int)header.fileCountAll);
            string[] ext = Utilities.GetFileExtensions(folderPath);
            Utilities.InsertSort(ext);
            int extCount = ext.Length;

            header.typeCount = (uint)extCount;
            int length = 13;
            DirectoryInfo d = new DirectoryInfo(folderPath);
            List<TypeHeaderV1> typeHeaders = new List<TypeHeaderV1>();
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
                    LogUtility.UpdateBar();
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
            bwhead.Dispose();
            bwtype.Dispose();
            bwentry.Dispose();
            bwdata.Dispose();
        }

        private struct HeaderV2
        {
            public uint fileCount { get; set; }
            public uint entrySize { get; set; }
        }

        private struct EntryV2
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
            LogUtility.InitBar((int)header.fileCount);

            for (int i = 0; i < header.fileCount; i++)
            {
                EntryV2 entry = new EntryV2();
                entry.fileSize = br1.ReadUInt32();
                entry.offset = br1.ReadUInt32() + 8 + header.entrySize;
                entry.fileName = Utilities.ReadCString(br1, Encoding.Unicode);
                entry.filePath = Path.Combine(folderPath, entry.fileName);

                l.Add(entry);
            }

            for (int i = 0; i < header.fileCount; i++)
            {
                byte[] buffer = br1.ReadBytes((int)l[i].fileSize);
                if (Global.ToDecryptScript && IsScriptFile(Path.GetExtension(l[i].filePath), "2"))
                {
                    LogUtility.Debug("Try to decrypt script:" + l[i].fileName);
                    DecryptScript(buffer);
                }
                File.WriteAllBytes(l[i].filePath, buffer);
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
                EntryV2 entry = new EntryV2();
                entry.fileName = fi.Name;
                entry.fileSize = (uint)new FileInfo(fi.FullName).Length;
                entry.offset = sizeToNow;
                sizeToNow += entry.fileSize;
                l.Add(entry);
            }

            //write init
            FileStream fs = File.Create(filePath);
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
                LogUtility.Info("Valid arc v1 archive detected.");
                arcV1_unpack(filePath, folderPath);
            }
            else
            {
                LogUtility.Info("Valid arc v2 archive detected.");
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
            ext = ext.ToLower().TrimStart('.');
            if (version == "1")
            {
                return ext == "scr" || ext == "wsc";
            }
            else if (version == "2")
            {
                return ext == "json" || ext == "ws2";
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
    }
}