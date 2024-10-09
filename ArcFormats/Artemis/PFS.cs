using ArcFormats.Properties;
using ArcFormats.Templates;
using Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
                LogUtility.Error_NotValidArchive();
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
                SHA1 sha = SHA1.Create();
                key = sha.ComputeHash(br.ReadBytes((int)header.indexSize));
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
            List<Entry> index = new List<Entry>();

            header.fileCount = (uint)Utilities.GetFileCount_All(folderPath);
            LogUtility.InitBar(header.fileCount);

            //new pathString array
            string[] pathString = new string[header.fileCount];

            //get path len and restore file info to pathString
            DirectoryInfo d = new DirectoryInfo(folderPath);
            int i = 0;
            foreach (FileInfo file in d.GetFiles("*.*", SearchOption.AllDirectories))
            {
                pathString[i] = file.FullName.Replace(folderPath + "\\", string.Empty);
                i++;
            }
            Utilities.InsertSort(pathString);

            //add entry
            for (int j = 0; j < header.fileCount; j++)
            {
                Entry artemisEntry = new Entry()
                {
                    size = (uint)new FileInfo(folderPath + "\\" + pathString[j]).Length,
                    relativePath = pathString[j],
                    fullPath = folderPath + "\\" + pathString[j],
                    relativePathLen = Global.Encoding.GetByteCount(pathString[j])
                };

                index.Add(artemisEntry);
                header.pathLenSum += (uint)artemisEntry.relativePathLen;
            }

            switch (Global.Version)
            {
                case "8":
                    //compute indexsize
                    header.indexSize = 4 + 16 * header.fileCount + header.pathLenSum + 4 + 8 * header.fileCount + 12;
                    //indexsize=(filecount)4byte+(pathlen+0x00000000+offset to begin+file size)16byte*filecount+pathlensum+(file count+1)4byte+8*filecount+(0x00000000)4byte*2+(offsettablebegin-0x7)4byte

                    //write header
                    MemoryStream ms8 = new MemoryStream((int)(header.indexSize + Marshal.SizeOf<Header>()));
                    BinaryWriter writer8 = new BinaryWriter(ms8);
                    writer8.Write(Encoding.ASCII.GetBytes(header.magic));
                    writer8.Write(Encoding.ASCII.GetBytes(header.version));
                    writer8.Write(header.indexSize);
                    writer8.Write(header.fileCount);

                    //write entry
                    long posIndexStart = ms8.Position - sizeof(uint);//0x7
                    uint offset8 = header.indexSize + 7;

                    foreach (var file in index)
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
                    foreach (var file in index)
                    {
                        total = total + 4 + (uint)file.relativePathLen;
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
                    byte[] xorKey = new byte[20];
                    using (SHA1 sha1 = SHA1.Create())
                    {
                        byte[] xorBuf = ms8.ToArray().Skip((int)posIndexStart).Take((int)header.indexSize).ToArray();
                        xorKey = sha1.ComputeHash(xorBuf);
                    }
                    FileStream arc = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                    ms8.WriteTo(arc);
                    foreach (var file in index)
                    {
                        byte[] fileData = File.ReadAllBytes(file.fullPath);
                        arc.Write(Xor.xor(fileData, xorKey), 0, fileData.Length);
                        LogUtility.UpdateBar();
                    }
                    arc.Dispose();
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
                    foreach (var file in index)
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
                    FileStream arc1 = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                    ms2.WriteTo(arc1);

                    foreach (var file in index)
                    {
                        byte[] fileData = File.ReadAllBytes(file.fullPath);
                        arc1.Write(fileData, 0, fileData.Length);

                        LogUtility.UpdateBar();
                    }
                    arc1.Dispose();
                    return;

                case "6":
                    header.indexSize = 4 + 16 * header.fileCount + header.pathLenSum + 4 + 8 * header.fileCount + 12;
                    //indexsize=(filecount)4byte+(pathlen+0x00000000+offset to begin+file size)16byte*filecount+pathlensum+(file count+1)4byte+8*filecount+(0x00000000)4byte*2+(offsettablebegin-0x7)4byte

                    //write header
                    MemoryStream ms6 = new MemoryStream((int)(header.indexSize + Marshal.SizeOf<Header>()));
                    BinaryWriter writer6 = new BinaryWriter(ms6);
                    writer6.Write(Encoding.ASCII.GetBytes(header.magic));
                    writer6.Write(Encoding.ASCII.GetBytes(header.version));
                    writer6.Write(header.indexSize);
                    writer6.Write(header.fileCount);

                    //write entry
                    uint offset6 = header.indexSize + 7;
                    foreach (var file in index)
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
                    foreach (var file in index)
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
                    FileStream arc2 = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                    ms6.WriteTo(arc2);
                    foreach (var file in index)
                    {
                        byte[] fileData = File.ReadAllBytes(file.fullPath);
                        arc2.Write(fileData, 0, fileData.Length);

                        LogUtility.UpdateBar();
                    }
                    arc2.Close();
                    return;
            }
        }
    }
}