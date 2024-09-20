using Log;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Utility;

namespace ArcFormats.EntisGLS
{
    public class NOA
    {
        private struct EntisGLS_noa_header
        {
            public byte[] magic1 { get; set; }
            public byte[] magic2 { get; set; }
            public uint noaSizeWithout { get; set; }
            public uint reserve { get; set; }

            public static byte[] magic1_valid = { 0x45, 0x6e, 0x74, 0x69, 0x73, 0x1a, 0x00, 0x00, 0x00, 0x04, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00 };

            public static byte[] magic2_valid = { 0x45, 0x52, 0x49, 0x53, 0x41, 0x2d, 0x41, 0x72, 0x63, 0x68, 0x69, 0x76, 0x65, 0x20, 0x66, 0x69, 0x6c, 0x65 };
        }

        private struct EntisGLS_noa_entry_header
        {
            public string magic { get; set; }
            public ulong indexSize { get; set; }
            public uint fileCount { get; set; }
        }

        private class EntisGLS_noa_entry
        {
            public ulong fileSize { get; set; }
            public uint attribute { get; set; }
            public uint encType { get; set; }
            public ulong offset { get; set; }
            public EntisGLS_noa_timeStamp timestamp { get; set; }
            public uint extraInfoLen { get; set; }
            public string extraInfo { get; set; }
            public uint fileNameLen { get; set; }
            public string fileName { get; set; }

            public EntisGLS_noa_entry()
            {
                timestamp = new EntisGLS_noa_timeStamp();
                fileSize = 0;
                attribute = 0;
                encType = 0;
                offset = 0;
                extraInfoLen = 0;
                extraInfo = string.Empty;
                fileNameLen = 0;
                fileName = string.Empty;
            }
        }

        private class EntisGLS_noa_timeStamp
        {
            public byte second { get; set; }
            public byte minute { get; set; }
            public byte hour { get; set; }
            public byte week { get; set; }
            public byte day { get; set; }
            public byte month { get; set; }
            public ushort year { get; set; }
        }

        public static void Unpack(string filePath, string folderPath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            EntisGLS_noa_header header = new EntisGLS_noa_header();

            header.magic1 = br.ReadBytes(16);
            header.magic2 = br.ReadBytes(18);

            if (!header.magic1.SequenceEqual(EntisGLS_noa_header.magic1_valid) || !header.magic2.SequenceEqual(EntisGLS_noa_header.magic2_valid))
            {
                LogUtility.Error_NotValidArchive();
            }

            br.ReadBytes(22);
            header.noaSizeWithout = br.ReadUInt32();
            header.reserve = br.ReadUInt32();

            EntisGLS_noa_entry_header entry_header = new EntisGLS_noa_entry_header();
            entry_header.magic = Encoding.ASCII.GetString(br.ReadBytes(8));
            if (entry_header.magic != "DirEntry")
            {
                LogUtility.Error_NotValidArchive();
            }

            entry_header.indexSize = br.ReadUInt64();
            entry_header.fileCount = br.ReadUInt32();
            Directory.CreateDirectory(folderPath);
            LogUtility.InitBar((int)entry_header.fileCount);
            //List<EntisGLS_noa_timeStamp> l = new List<EntisGLS_noa_timeStamp>();

            long pos = 0;
            for (int i = 0; i < entry_header.fileCount; i++)
            {
                EntisGLS_noa_entry entry = new EntisGLS_noa_entry();
                entry.fileSize = br.ReadUInt64();
                entry.attribute = br.ReadUInt32();
                entry.encType = br.ReadUInt32();
                entry.offset = br.ReadUInt64() + 64;

                if (entry.encType != 0)
                {
                    throw new NotImplementedException("Error:encrypted noa file not supported.");
                }
                br.ReadBytes(8);
                //entry.timestamp.second = br.ReadByte();
                //entry.timestamp.minute = br.ReadByte();
                //entry.timestamp.hour = br.ReadByte();
                //entry.timestamp.week = br.ReadByte();
                //entry.timestamp.day = br.ReadByte();
                //entry.timestamp.month = br.ReadByte();
                //entry.timestamp.year = br.ReadUInt16();
                //l.Add(entry.timestamp);
                //Save timestamp disabled
                entry.extraInfoLen = br.ReadUInt32();
                entry.extraInfo = Encoding.ASCII.GetString(br.ReadBytes((int)entry.extraInfoLen));
                entry.fileNameLen = br.ReadUInt32();
                entry.fileName = Global.UnpackEncoding.GetString(br.ReadBytes((int)(entry.fileNameLen - 1)));
                br.ReadByte();
                pos = fs.Position;
                fs.Seek((long)entry.offset, SeekOrigin.Begin);
                br.ReadBytes(16);
                byte[] buffer = br.ReadBytes((int)entry.fileSize);
                File.WriteAllBytes(folderPath + "\\" + entry.fileName, buffer);
                fs.Seek(pos, SeekOrigin.Begin);
                LogUtility.UpdateBar();
            }
            //string jsonStr = JsonSerializer.Serialize(l);
            //File.WriteAllText(folderPath + "\\" + "TimestampInfo.json", jsonStr);
            fs.Dispose();
        }

        public static void Pack(string folderPath, string filePath)
        {
            //string jsonPath = folderPath + "\\" + "TimestampInfo.json";
            FileStream fw = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fw);
            string[] files = Directory.GetFiles(folderPath);
            //string[] excludedNames = new string[] { "TimestampInfo.json" };
            //var filteredFiles = files.Where(file => !excludedNames.Contains(Path.GetFileName(file)));
            int fileCount = Utilities.GetFileCount_All(folderPath);
            //string jsonStr;
            //List<EntisGLS_noa_timeStamp> time = new();
            //if (File.Exists(jsonPath))
            //{
            //    fileCount = Util.GetFileCount_All(folderPath) - 1;
            //    jsonStr = File.ReadAllText(jsonPath);
            //}
            //else
            //{
            //    fileCount = Util.GetFileCount_All(folderPath);
            //    jsonStr = string.Empty;
            //}
            LogUtility.InitBar(fileCount);
            //if (string.IsNullOrEmpty(jsonStr))
            //{
            //    for (int j = 0; j < fileCount; j++)
            //    {
            //        EntisGLS_noa_timeStamp ts = new();
            //        ts.second = 0x00;
            //        ts.minute = 0x00;
            //        ts.hour = 0x00;
            //        ts.week = 0x00;
            //        ts.day = 0x00;
            //        ts.month = 0x00;
            //        ts.year = 0;
            //        time.Add(ts);
            //    }
            //}
            //else
            //{
            //    time = JsonSerializer.Deserialize<List<EntisGLS_noa_timeStamp>>(jsonStr);
            //}

            //header
            bw.Write(EntisGLS_noa_header.magic1_valid);
            bw.Write(EntisGLS_noa_header.magic2_valid);
            bw.Write(new byte[30]);//skip file size,temporarily
                                   //entry header
            bw.Write(Encoding.ASCII.GetBytes("DirEntry"));
            //compute index size
            ulong indexSize = 4 + (ulong)Utilities.GetNameLenSum(files, Global.PackEncoding) + (ulong)fileCount + (ulong)(40 * fileCount);
            bw.Write(indexSize);
            bw.Write(fileCount);

            ulong offset = 16 + indexSize;
            int i = 0;
            //entry
            foreach (var file in files)
            {
                bw.Write(new FileInfo(file).Length);
                bw.Write((long)0);//attribute&encType
                bw.Write(offset);
                offset += (ulong)new FileInfo(file).Length + 16;//"filedata" + file size

                //bw.Write(time[i].second);
                //bw.Write(time[i].minute);
                //bw.Write(time[i].hour);
                //bw.Write(time[i].week);
                //bw.Write(time[i].day);
                //bw.Write(time[i].month);
                //bw.Write(time[i].year);
                bw.Write((long)0);//timestamp disabled
                bw.Write(0);
                bw.Write(Global.PackEncoding.GetBytes(Path.GetFileName(file)).Length + 1);
                bw.Write(Global.PackEncoding.GetBytes(Path.GetFileName(file)));
                bw.Write('\0');
                i++;
            }
            foreach (var file in files)
            {
                bw.Write(Encoding.ASCII.GetBytes("filedata"));
                bw.Write(new FileInfo(file).Length);
                bw.Write(File.ReadAllBytes(file));
                LogUtility.UpdateBar();
            }
            int size = (int)fw.Position;
            fw.Position = 56;//write size
            bw.Write(size - 64);
            fw.Dispose();
        }
    }
}