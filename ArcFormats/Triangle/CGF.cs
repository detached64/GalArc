﻿using ArcFormats.Templates;
using Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Utility;

namespace ArcFormats.Triangle
{
    public class CGF
    {
        public static UserControl PackExtraOptions = new VersionOnly("1");

        private struct Entry
        {
            internal string fileName;
            internal uint offset;
        }

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            int fileCount = br.ReadInt32();

            fs.Position = 20;
            uint offset1 = br.ReadUInt32();
            fs.Position = 32;
            uint offset2 = br.ReadUInt32();
            fs.Dispose();
            br.Dispose();

            if (offset1 == 4 + 20 * (uint)fileCount)
            {
                cgfV1_unpack(filePath, folderPath);
            }
            else if ((offset2 & ~0xc0000000) == 4 + 32 * (uint)fileCount)
            {
                LogUtility.Error("cgf v2 archive not implemented.");
            }
            else
            {
                LogUtility.ErrorInvalidArchive();
            }
        }

        public void Pack(string folderPath, string filePath)
        {
            if (Global.Version == "1")
            {
                cgfV1_pack(folderPath, filePath);
            }
            else if (Global.Version == "2")
            {

            }
        }

        private static void cgfV1_unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            int fileCount = br.ReadInt32();
            Directory.CreateDirectory(folderPath);
            LogUtility.InitBar(fileCount);
            List<Entry> entries = new List<Entry>();

            for (int i = 0; i < fileCount; i++)
            {
                Entry entry = new Entry();
                long pos = fs.Position;
                entry.fileName = Utilities.ReadCString(br, ArcEncoding.Shift_JIS);
                fs.Position = pos + 16;
                entry.offset = br.ReadUInt32();
                entries.Add(entry);
            }

            for (int i = 0; i < fileCount - 1; i++)
            {
                File.WriteAllBytes(Path.Combine(folderPath, entries[i].fileName), br.ReadBytes((int)(entries[i + 1].offset - entries[i].offset)));
                LogUtility.UpdateBar();
            }
            File.WriteAllBytes(Path.Combine(folderPath, entries[fileCount - 1].fileName), br.ReadBytes((int)(fs.Length - entries[fileCount - 1].offset)));
            LogUtility.UpdateBar();
            fs.Dispose();
            br.Dispose();
        }

        private static void cgfV1_pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            DirectoryInfo d = new DirectoryInfo(folderPath);
            FileInfo[] files = d.GetFiles();
            int fileCount = files.Length;
            bw.Write(fileCount);
            uint baseOffset = 4 + 20 * (uint)fileCount;
            LogUtility.InitBar(fileCount);
            foreach (FileInfo file in files)
            {
                bw.Write(ArcEncoding.Shift_JIS.GetBytes(file.Name.PadRight(16, '\0')));
                bw.Write(baseOffset);
                baseOffset += (uint)file.Length;
            }
            foreach (FileInfo file in files)
            {
                byte[] data = File.ReadAllBytes(file.FullName);
                bw.Write(data);
                LogUtility.UpdateBar();
            }
            fw.Dispose();
            bw.Dispose();
        }
    }
}
