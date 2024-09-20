using Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility;
using Utility.Compression;

namespace ArcFormats.Ai6Win
{
    public class ARC
    {
        private struct Silky_arc_header
        {
            public uint fileCount { get; set; }
        }

        private struct Silky_arc_entry
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
            Silky_arc_header header = new Silky_arc_header();
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            header.fileCount = br.ReadUInt32();
            LogUtility.InitBar((int)header.fileCount);

            List<Silky_arc_entry> l = new List<Silky_arc_entry>();
            for (int i = 0; i < header.fileCount; i++)
            {
                Silky_arc_entry entry = new Silky_arc_entry();
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

        //public static int arc_pack(string folderPath)
        //{
        //    string filePath = Path.GetDirectoryName(folderPath) + "\\" + Path.GetFileName(folderPath) + ".arc.new";
        //    FileStream fw = new(filePath, FileMode.Create, FileAccess.Write);
        //    int fileCount = Util.GetFileCount_All(folderPath);
        //    Interface.DisplayFace.displayPack(fileCount);
        //    BinaryWriter bw = new(fw);
        //    bw.Write((uint)fileCount);

        //    List<Silky_arc_entry> l = new();
        //    DirectoryInfo d = new(folderPath);
        //    int baseOffset = 272 * fileCount + 4;
        //    foreach (FileInfo fi in d.GetFiles())
        //    {
        //        byte[] thisName = ArcEncoding.Encodings(932).GetBytes(fi.Name);
        //        byte key = (byte)(thisName.Length + 1);
        //        for (int j = 0; j < thisName.Length; j++)
        //        {
        //            thisName[j] += key;
        //            key--;
        //        }
        //        bw.Write(thisName);
        //        bw.Write(new byte[260 - thisName.Length]);
        //        bw.Write(0);
        //        bw.Write(BigEndian.IntToBEBytes(File.ReadAllBytes(fi.FullName).Length));
        //        bw.Write(BigEndian.IntToBEBytes(baseOffset));
        //        baseOffset += File.ReadAllBytes(fi.FullName).Length;
        //    }//lzss压缩未成功；程序无响应？

        //    //foreach (FileInfo fi in d.GetFiles())
        //    //{
        //    //    MemoryStream compressed = new MemoryStream(LzssPackBytes(File.ReadAllBytes(fi.FullName)));
        //    //    compressed.WriteTo(fw);
        //    //    compressed.Close();
        //    //    main.Main.bar.PerformStep();
        //    //}

        //    bw.Close();
        //    fw.Close();
        //    return 0;
        //}
    }
}