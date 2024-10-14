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
        private class Header
        {
            public byte[] magic1 { get; set; }
            public byte[] magic2 { get; set; }
            public uint noaSizeWithout { get; set; }
            public uint reserve { get; set; }

            public static readonly byte[] magic1Valid = Utilities.HexStringToByteArray("456e7469731a00000004000200000000");
            public static readonly byte[] magic2Valid = Utilities.HexStringToByteArray("45524953412d417263686976652066696c65");
        }

        private class EntryHeader
        {
            public string magic { get; set; }
            public ulong indexSize { get; set; }
            public uint fileCount { get; set; }
        }

        private class Entry
        {
            public ulong size { get; set; }
            public uint attribute { get; set; }
            public uint encType { get; set; }
            public ulong offset { get; set; }
            public TimeStamp timestamp { get; set; }
            public uint extraInfoLen { get; set; }
            public string extraInfo { get; set; }
            public uint nameLen { get; set; }
            public string name { get; set; }

            public Entry()
            {
                timestamp = new TimeStamp();
                size = 0;
                attribute = 0;
                encType = 0;
                offset = 0;
                extraInfoLen = 0;
                extraInfo = string.Empty;
                nameLen = 0;
                name = string.Empty;
            }
        }

        private class TimeStamp
        {
            public byte second { get; set; }
            public byte minute { get; set; }
            public byte hour { get; set; }
            public byte week { get; set; }
            public byte day { get; set; }
            public byte month { get; set; }
            public ushort year { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            Header header = new Header();

            header.magic1 = br.ReadBytes(16);
            header.magic2 = br.ReadBytes(18);

            if (!header.magic1.SequenceEqual(Header.magic1Valid) || !header.magic2.SequenceEqual(Header.magic2Valid))
            {
                LogUtility.ErrorInvalidArchive();
            }

            br.ReadBytes(22);
            header.noaSizeWithout = br.ReadUInt32();
            header.reserve = br.ReadUInt32();

            EntryHeader entry_header = new EntryHeader();
            entry_header.magic = Encoding.ASCII.GetString(br.ReadBytes(8));
            if (entry_header.magic != "DirEntry")
            {
                LogUtility.ErrorInvalidArchive();
            }

            entry_header.indexSize = br.ReadUInt64();
            entry_header.fileCount = br.ReadUInt32();
            Directory.CreateDirectory(folderPath);
            LogUtility.InitBar(entry_header.fileCount);
            //List<EntisGLS_noa_timeStamp> l = new List<EntisGLS_noa_timeStamp>();

            long pos = 0;
            for (int i = 0; i < entry_header.fileCount; i++)
            {
                Entry entry = new Entry();
                entry.size = br.ReadUInt64();
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
                entry.nameLen = br.ReadUInt32();
                entry.name = Global.Encoding.GetString(br.ReadBytes((int)(entry.nameLen - 1)));
                br.ReadByte();
                pos = fs.Position;
                fs.Seek((long)entry.offset, SeekOrigin.Begin);
                br.ReadBytes(16);
                byte[] buffer = br.ReadBytes((int)entry.size);
                File.WriteAllBytes(Path.Combine(folderPath, entry.name), buffer);
                fs.Position = pos;
                LogUtility.UpdateBar();
            }
            //string jsonStr = JsonSerializer.Serialize(l);
            //File.WriteAllText(folderPath + "\\" + "TimestampInfo.json", jsonStr);
            fs.Dispose();
            br.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            //string jsonPath = folderPath + "\\" + "TimestampInfo.json";
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            string[] files = Directory.GetFiles(folderPath);
            //string[] excludedNames = new string[] { "TimestampInfo.json" };
            //var filteredFiles = files.Where(file => !excludedNames.Contains(Path.GetFileName(file)));
            int fileCount = Utilities.GetFileCount(folderPath);
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
            bw.Write(Header.magic1Valid);
            bw.Write(Header.magic2Valid);
            bw.Write(new byte[30]);//skip file size,temporarily
                                   //entry header
            bw.Write(Encoding.ASCII.GetBytes("DirEntry"));
            //compute index size
            ulong indexSize = 4 + (ulong)Utilities.GetNameLengthSum(files, Global.Encoding) + (ulong)fileCount + (ulong)(40 * fileCount);
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
                bw.Write(Global.Encoding.GetBytes(Path.GetFileName(file)).Length + 1);
                bw.Write(Global.Encoding.GetBytes(Path.GetFileName(file)));
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
            bw.Dispose();
        }
    }
}