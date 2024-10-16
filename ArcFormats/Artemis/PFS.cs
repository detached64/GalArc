﻿using ArcFormats.Properties;
using ArcFormats.Templates;
using Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Utility;
using Utility.Extensions;

namespace ArcFormats.Artemis
{
    public class PFS
    {
        public static UserControl PackExtraOptions = new VersionOnly("8/6/2");

        private static readonly string[] Versions = { "8", "6", "2" };

        private class Header
        {
            public string magic { get; } = "pf";
            public string version { get; set; }
            public uint indexSize { get; set; }
            public uint fileCount { get; set; }
            public uint pathLenSum { get; set; }
        }

        private struct Entry
        {
            public string fullPath { get; set; }
            public uint size { get; set; }
            public uint offset { get; set; }
            public int relativePathLen { get; set; }
            public string relativePath { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            //init
            Header header = new Header();

            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);

            if (Encoding.ASCII.GetString(br.ReadBytes(2)) != header.magic)
            {
                LogUtility.ErrorInvalidArchive();
            }
            header.version = br.ReadChar().ToString();
            if (!Versions.Contains(header.version))
            {
                LogUtility.Error(string.Format(Resources.logErrorNotSupportedVersion, "pfs", header.version));
            }
            LogUtility.ShowVersion("pfs", header.version);
            // read header
            header.indexSize = br.ReadUInt32();
            if (header.version == "2")
            {
                br.ReadUInt32();
            }
            header.fileCount = br.ReadUInt32();
            LogUtility.InitBar(header.fileCount);
            // compute key
            byte[] key = new byte[20];  // SHA1 hash of index
            if (header.version == "8")
            {
                fs.Position = 7;
                using (SHA1 sha = SHA1.Create())
                {
                    key = sha.ComputeHash(br.ReadBytes((int)header.indexSize));
                }
                fs.Position = 11;
            }
            // read index and save files
            for (int i = 0; i < header.fileCount; i++)
            {
                Entry entry = new Entry();
                entry.relativePathLen = br.ReadInt32();
                string name = Global.Encoding.GetString(br.ReadBytes(entry.relativePathLen));
                if (name.ContainsInvalidChars())
                {
                    throw new Exception(Resources.logErrorContainsInvalid);
                }
                entry.fullPath = Path.Combine(folderPath, name);

                br.ReadUInt32();
                if (header.version == "2")
                {
                    br.ReadBytes(8);
                }
                entry.offset = br.ReadUInt32();
                entry.size = br.ReadUInt32();

                string dir = Path.GetDirectoryName(entry.fullPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                long pos = fs.Position;
                fs.Position = entry.offset;
                byte[] buffer = br.ReadBytes((int)entry.size);
                if (header.version == "8")
                {
                    buffer = Xor.xor(buffer, key);
                }
                File.WriteAllBytes(entry.fullPath, buffer);
                fs.Position = pos;
                LogUtility.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            //init
            Header header = new Header()
            {
                version = Global.Version,
                pathLenSum = 0
            };
            List<Entry> entries = new List<Entry>();
            string[] files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
            string[] relativePaths = Utilities.GetRelativePaths(files, folderPath);
            header.pathLenSum = (uint)Utilities.GetLengthSum(relativePaths, Global.Encoding);
            header.fileCount = (uint)files.Length;
            LogUtility.InitBar(header.fileCount);
            Utilities.InsertSort(relativePaths);

            //add entry
            for (int i = 0; i < header.fileCount; i++)
            {
                Entry entry = new Entry();
                entry.fullPath = Path.Combine(folderPath, relativePaths[i]);
                entry.size = (uint)new FileInfo(entry.fullPath).Length;
                entry.relativePath = relativePaths[i];
                entry.relativePathLen = Global.Encoding.GetByteCount(relativePaths[i]);
                entries.Add(entry);
            }

            switch (Global.Version)
            {
                case "8":
                    header.indexSize = 4 + 16 * header.fileCount + header.pathLenSum + 4 + 8 * header.fileCount + 12;

                    //write header
                    MemoryStream ms8 = new MemoryStream();
                    BinaryWriter writer8 = new BinaryWriter(ms8);
                    writer8.Write(Encoding.ASCII.GetBytes(header.magic));
                    writer8.Write(Encoding.ASCII.GetBytes(header.version));
                    writer8.Write(header.indexSize);
                    writer8.Write(header.fileCount);

                    //write entry
                    uint offset8 = header.indexSize + 7;

                    foreach (var file in entries)
                    {
                        writer8.Write(file.relativePathLen);
                        writer8.Write(Global.Encoding.GetBytes(file.relativePath));
                        writer8.Write(0); // reserved
                        writer8.Write(offset8);
                        writer8.Write(file.size);
                        offset8 += file.size;
                    }

                    long posOffsetTable = ms8.Position;
                    uint offsetCount = header.fileCount + 1;
                    writer8.Write(offsetCount);//filecount + 1
                    uint total = 4;

                    //write table
                    foreach (var entry in entries)
                    {
                        total = total + 4 + (uint)entry.relativePathLen;
                        uint posOffset = total;
                        writer8.Write(posOffset);
                        writer8.Write(0); // reserved
                        total += 12;
                    }
                    writer8.Write(0); // EOF of offset table
                    writer8.Write(0); // EOF of offset table
                    uint tablePos = (uint)(posOffsetTable - 7);
                    writer8.Write(tablePos);

                    //write data
                    byte[] key = new byte[20];
                    byte[] buf = ms8.ToArray();
                    using (SHA1 sha1 = SHA1.Create())
                    {
                        byte[] xorBuf = new byte[buf.Length - 7];
                        Array.Copy(buf, 7, xorBuf, 0, buf.Length - 7);
                        key = sha1.ComputeHash(xorBuf);
                    }
                    FileStream fw8 = File.Create(filePath);
                    fw8.Write(buf, 0, buf.Length);
                    foreach (var entry in entries)
                    {
                        byte[] fileData = File.ReadAllBytes(entry.fullPath);
                        fw8.Write(Xor.xor(fileData, key), 0, fileData.Length);
                        LogUtility.UpdateBar();
                    }
                    fw8.Dispose();
                    ms8.Dispose();
                    writer8.Dispose();
                    return;

                case "2":
                    header.indexSize = 8 + 24 * header.fileCount + header.pathLenSum;

                    //write header
                    MemoryStream ms2 = new MemoryStream();
                    BinaryWriter writer2 = new BinaryWriter(ms2);
                    writer2.Write(Encoding.ASCII.GetBytes(header.magic));
                    writer2.Write(Encoding.ASCII.GetBytes(header.version));
                    writer2.Write(header.indexSize);
                    writer2.Write((uint)0);
                    writer2.Write(header.fileCount);
                    uint offset2 = header.indexSize + 7;

                    //write entry
                    foreach (var file in entries)
                    {
                        writer2.Write((uint)file.relativePathLen);
                        writer2.Write(Global.Encoding.GetBytes(file.relativePath));
                        writer2.Write((uint)16);
                        writer2.Write((uint)0);
                        writer2.Write((uint)0);
                        writer2.Write(offset2);
                        writer2.Write(file.size);
                        offset2 += file.size;
                    }

                    //write data
                    FileStream fw2 = File.Create(filePath);
                    ms2.WriteTo(fw2);

                    foreach (var file in entries)
                    {
                        byte[] fileData = File.ReadAllBytes(file.fullPath);
                        fw2.Write(fileData, 0, fileData.Length);
                        LogUtility.UpdateBar();
                    }
                    fw2.Dispose();
                    ms2.Dispose();
                    writer2.Dispose();
                    return;

                case "6":
                    header.indexSize = 4 + 16 * header.fileCount + header.pathLenSum + 4 + 8 * header.fileCount + 12;
                    //indexsize=(filecount)4byte+(pathlen+0x00000000+offset to begin+file size)16byte*filecount+pathlensum+(file count+1)4byte+8*filecount+(0x00000000)4byte*2+(offsettablebegin-0x7)4byte

                    //write header
                    MemoryStream ms6 = new MemoryStream();
                    BinaryWriter writer6 = new BinaryWriter(ms6);
                    writer6.Write(Encoding.ASCII.GetBytes(header.magic));
                    writer6.Write(Encoding.ASCII.GetBytes(header.version));
                    writer6.Write(header.indexSize);
                    writer6.Write(header.fileCount);

                    //write entry
                    uint offset6 = header.indexSize + 7;
                    foreach (var file in entries)
                    {
                        uint filenameSize = (uint)file.relativePathLen;//use utf-8 for japanese character in file name
                        writer6.Write(filenameSize);
                        writer6.Write(Global.Encoding.GetBytes(file.relativePath));
                        writer6.Write(0); // reserved
                        writer6.Write(offset6);
                        writer6.Write(file.size);
                        offset6 += file.size;
                    }

                    long posOffsetTable6 = ms6.Position;
                    uint offsetCount6 = header.fileCount + 1;
                    writer6.Write(offsetCount6);//filecount + 1
                    uint total6 = 4;

                    //write table
                    foreach (var file in entries)
                    {
                        total6 = total6 + 4 + (uint)file.relativePathLen;//use utf-8 for japanese character in file name
                        uint posOffset = total6;
                        writer6.Write(posOffset);
                        writer6.Write(0); // reserved
                        total6 += 12;
                    }
                    writer6.Write(0); // EOF of offset table
                    writer6.Write(0); // EOF of offset table
                    uint tablePos6 = (uint)(posOffsetTable6 - 7);
                    writer6.Write(tablePos6);

                    //write data
                    FileStream fw6 = File.Create(filePath);
                    ms6.WriteTo(fw6);
                    foreach (var file in entries)
                    {
                        byte[] fileData = File.ReadAllBytes(file.fullPath);
                        fw6.Write(fileData, 0, fileData.Length);
                        LogUtility.UpdateBar();
                    }
                    fw6.Dispose();
                    ms6.Dispose();
                    writer6.Dispose();
                    return;
            }
        }
    }
}