using Log;
using System;
using System.Collections.Generic;
using System.IO;
using Utility;
using Utility.Compression;

namespace ArcFormats.Ai6Win
{
    public class ARC
    {
        private struct Header
        {
            public uint fileCount { get; set; }
        }

        private struct Entry
        {
            public string name { get; set; }
            public uint sizePacked { get; set; }
            public uint sizeUnpacked { get; set; }
            public uint offset { get; set; }

            //additional
            public bool isPacked { get; set; }
        }

        public static void Unpack(string filePath, string folderPath)
        {
            Header header = new Header();
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            header.fileCount = br.ReadUInt32();
            LogUtility.InitBar((int)header.fileCount);

            List<Entry> l = new List<Entry>();
            for (int i = 0; i < header.fileCount; i++)
            {
                Entry entry = new Entry();
                byte[] nameBuf = new byte[260];
                br.Read(nameBuf, 0, 260);
                int nameLen = Array.IndexOf<byte>(nameBuf, 0);
                if (nameLen == -1)
                {
                    nameLen = nameBuf.Length;
                }

                byte key = (byte)(nameLen + 1);
                for (int j = 0; j < nameLen; j++)
                {
                    nameBuf[j] -= key;
                    key--;
                }
                entry.name = ArcEncoding.Shift_JIS.GetString(nameBuf, 0, nameLen);
                entry.sizePacked = BigEndian.Read(br.ReadUInt32());
                entry.sizeUnpacked = BigEndian.Read(br.ReadUInt32());
                entry.offset = BigEndian.Read(br.ReadUInt32());
                entry.isPacked = entry.sizePacked != entry.sizeUnpacked;
                l.Add(entry);
                //LogUtility.Debug(entry.name);
            }
            Directory.CreateDirectory(folderPath);
            for (int i = 0; i < header.fileCount; i++)
            {
                FileStream fw = new FileStream(folderPath + "\\" + l[i].name, FileMode.Create, FileAccess.Write);
                MemoryStream ms = new MemoryStream(br.ReadBytes((int)l[i].sizePacked));
                if (l[i].isPacked)
                {
                    Lzss.Decompress(ms).WriteTo(fw);
                    ms.Dispose();
                    fw.Dispose();
                }
                else
                {
                    ms.WriteTo(fw);
                    ms.Dispose();
                    fw.Dispose();
                }
                LogUtility.UpdateBar();
            }
        }
    }
}