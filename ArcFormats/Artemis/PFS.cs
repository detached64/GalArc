using Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Utility;

namespace ArcFormats.Artemis
{
    public class PFS
    {
        private struct Header
        {
            public string Magic { get; set; }
            public string Version { get; set; }
            public uint IndexSize { get; set; }
            public uint FileCount { get; set; }
            public uint pathLenSum { get; set; }
        }

        private struct Entry
        {
            public string filePath { get; set; }
            public uint Size { get; set; }
            public uint Offset { get; set; }
            public int pathLen { get; set; }
            public string path { get; set; }
        }

        public static void Unpack(string filePath, string folderPath)
        {
            //init
            Header header = new Header()
            {
                Magic = "pf",
                FileCount = 0
            };

            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            if (Encoding.ASCII.GetString(br.ReadBytes(2)) != header.Magic)
            {
                LogUtility.Error_NotValidArchive();
            }
            header.Version = Encoding.ASCII.GetString(br.ReadBytes(1));

            switch (header.Version)
            {
                case "8":
                    header.IndexSize = br.ReadUInt32();
                    LogUtility.ShowVersion("pfs", 8);

                    byte[] xorKey;
                    byte[] headerBytes = br.ReadBytes((int)header.IndexSize);
                    SHA1 sha = SHA1.Create();
                    xorKey = sha.ComputeHash(headerBytes);

                    fs.Seek(-header.IndexSize, SeekOrigin.Current);

                    header.FileCount = br.ReadUInt32();

                    long presPos8 = 0;

                    //process
                    LogUtility.InitBar((int)header.FileCount);

                    for (int i = 0; i < header.FileCount; i++)
                    {
                        Entry entry = new Entry();
                        entry.pathLen = br.ReadInt32();
                        entry.filePath = folderPath + "\\" + Global.UnpackEncoding.GetString(br.ReadBytes(entry.pathLen));
                        br.ReadUInt32(); // skip 4 unused bytes:0x00000000
                        entry.Offset = br.ReadUInt32();
                        entry.Size = br.ReadUInt32();

                        Directory.CreateDirectory(Path.GetDirectoryName(entry.filePath));

                        LogUtility.Debug(entry.filePath);
                        using (FileStream fo = new FileStream(entry.filePath, FileMode.Create, FileAccess.Write))
                        {
                            presPos8 = fs.Position;
                            fs.Seek(entry.Offset, SeekOrigin.Begin);

                            for (int j = 0; j < entry.Size + 1; j += xorKey.Length)
                            {
                                int toRead = (int)Math.Min(entry.Size - j, xorKey.Length);
                                byte[] buffer = br.ReadBytes(toRead);
                                fo.Write(Xor.xor(buffer, xorKey), 0, toRead);
                            }
                            fs.Seek(presPos8, SeekOrigin.Begin);
                        }
                        LogUtility.UpdateBar();
                    }
                    fs.Dispose();
                    br.Dispose();
                    return;

                case "2":
                    header.IndexSize = br.ReadUInt32();
                    LogUtility.ShowVersion("pfs", 2);
                    br.ReadUInt32();//reserve 0x00000000
                    header.FileCount = br.ReadUInt32();
                    long presPos2 = 0;
                    LogUtility.InitBar((int)header.FileCount);

                    for (int i = 0; i < header.FileCount; i++)
                    {
                        Entry entry = new Entry();
                        entry.pathLen = (int)br.ReadUInt32();
                        entry.filePath = folderPath + "/" + Global.UnpackEncoding.GetString(br.ReadBytes(entry.pathLen)).Replace("\\", "/");
                        br.ReadUInt32();//0x10000000
                        br.ReadUInt32();//0x00000000
                        br.ReadUInt32();//0x00000000
                        entry.Offset = br.ReadUInt32();
                        entry.Size = br.ReadUInt32();

                        Directory.CreateDirectory(Path.GetDirectoryName(entry.filePath));

                        presPos2 = fs.Position;
                        fs.Seek(entry.Offset, SeekOrigin.Begin);
                        byte[] buffer = br.ReadBytes((int)entry.Size);
                        File.WriteAllBytes(entry.filePath, buffer);
                        fs.Seek(presPos2, SeekOrigin.Begin);

                        LogUtility.UpdateBar();
                    }
                    fs.Dispose();
                    br.Dispose();
                    return;

                case "6":
                    header.IndexSize = br.ReadUInt32();
                    LogUtility.ShowVersion("pfs", 6);
                    header.FileCount = br.ReadUInt32();
                    long presPos6 = 0;//position at present
                    LogUtility.InitBar((int)header.FileCount);

                    //process
                    for (int i = 0; i < header.FileCount; i++)
                    {
                        Entry entry = new Entry();
                        entry.pathLen = br.ReadInt32();
                        entry.filePath = folderPath + "/" + Global.UnpackEncoding.GetString(br.ReadBytes(entry.pathLen)).Replace("\\", "/");
                        br.ReadUInt32(); // skip 4 unused bytes:0x00000000
                        entry.Offset = br.ReadUInt32();
                        entry.Size = br.ReadUInt32();

                        Directory.CreateDirectory(Path.GetDirectoryName(entry.filePath));

                        presPos6 = fs.Position;
                        fs.Seek(entry.Offset, SeekOrigin.Begin);
                        byte[] buffer = br.ReadBytes((int)entry.Size);
                        File.WriteAllBytes(entry.filePath, buffer);
                        fs.Seek(presPos6, SeekOrigin.Begin);

                        LogUtility.UpdateBar();
                    }
                    fs.Dispose();
                    br.Dispose();
                    return;

                default:
                    throw new NotImplementedException($"pfs v{header.Version} archive temporarily not supported.");
            }
        }

        public static void Pack(string folderPath, string filePath)
        {
            //init
            Header header = new Header()
            {
                Magic = "pf",
                Version = Global.Version,
                pathLenSum = 0
            };
            List<Entry> index = new List<Entry>();

            header.FileCount = (uint)Utilities.GetFileCount_All(folderPath);
            LogUtility.InitBar((int)header.FileCount);

            //new pathString array
            string[] pathString = new string[header.FileCount];

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
            for (int j = 0; j < header.FileCount; j++)
            {
                Entry artemisEntry = new Entry()
                {
                    Size = (uint)new FileInfo(folderPath + "\\" + pathString[j]).Length,
                    path = pathString[j],
                    filePath = folderPath + "\\" + pathString[j],
                    pathLen = Global.PackEncoding.GetByteCount(pathString[j])
                };

                index.Add(artemisEntry);
                header.pathLenSum += (uint)artemisEntry.pathLen;
            }

            switch (Global.Version)
            {
                case "8":
                    //compute indexsize
                    header.IndexSize = 4 + 16 * header.FileCount + header.pathLenSum + 4 + 8 * header.FileCount + 12;
                    //indexsize=(filecount)4byte+(pathlen+0x00000000+offset to begin+file size)16byte*filecount+pathlensum+(file count+1)4byte+8*filecount+(0x00000000)4byte*2+(offsettablebegin-0x7)4byte

                    //write header
                    MemoryStream ms8 = new MemoryStream((int)(header.IndexSize + Marshal.SizeOf<Header>()));
                    BinaryWriter writer8 = new BinaryWriter(ms8);
                    writer8.Write(Encoding.ASCII.GetBytes(header.Magic));
                    writer8.Write(Encoding.ASCII.GetBytes(header.Version));
                    writer8.Write(header.IndexSize);
                    writer8.Write(header.FileCount);

                    //write entry
                    long posIndexStart = ms8.Position - sizeof(uint);//0x7
                    uint offset8 = header.IndexSize + 7;

                    foreach (var file in index)
                    {
                        writer8.Write(file.pathLen);
                        writer8.Write(Global.PackEncoding.GetBytes(file.path));
                        writer8.Write(0); // reserved
                        writer8.Write(offset8);
                        writer8.Write(file.Size);
                        offset8 += file.Size;
                    }

                    long posOffsetTable = ms8.Position;
                    uint offsetCount = header.FileCount + 1;
                    writer8.Write(offsetCount);//filecount + 1
                    uint total = 4;

                    //write table
                    foreach (var file in index)
                    {
                        total = total + 4 + (uint)file.pathLen;
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
                        byte[] xorBuf = ms8.ToArray().Skip((int)posIndexStart).Take((int)header.IndexSize).ToArray();
                        xorKey = sha1.ComputeHash(xorBuf);
                    }
                    FileStream arc = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                    ms8.WriteTo(arc);
                    foreach (var file in index)
                    {
                        byte[] fileData = File.ReadAllBytes(file.filePath);
                        arc.Write(Xor.xor(fileData, xorKey), 0, fileData.Length);
                        LogUtility.UpdateBar();
                    }
                    arc.Dispose();
                    ms8.Dispose();
                    writer8.Dispose();
                    return;

                case "2":
                    header.IndexSize = 8 + 24 * header.FileCount + header.pathLenSum;

                    //write header
                    MemoryStream ms2 = new MemoryStream();
                    BinaryWriter writer2 = new BinaryWriter(ms2);
                    writer2.Write(Encoding.ASCII.GetBytes(header.Magic));
                    writer2.Write(Encoding.ASCII.GetBytes(header.Version));
                    writer2.Write(header.IndexSize);
                    writer2.Write((uint)0);
                    writer2.Write(header.FileCount);
                    uint offset2 = header.IndexSize + 7;

                    //write entry
                    foreach (var file in index)
                    {
                        writer2.Write((uint)file.pathLen);
                        writer2.Write(Global.PackEncoding.GetBytes(file.path));
                        writer2.Write((uint)16);
                        writer2.Write((uint)0);
                        writer2.Write((uint)0);
                        writer2.Write(offset2);
                        writer2.Write(file.Size);
                        offset2 += file.Size;
                    }

                    //write data
                    FileStream arc1 = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                    ms2.WriteTo(arc1);

                    foreach (var file in index)
                    {
                        byte[] fileData = File.ReadAllBytes(file.filePath);
                        arc1.Write(fileData, 0, fileData.Length);

                        LogUtility.UpdateBar();
                    }
                    arc1.Dispose();
                    return;

                case "6":
                    header.IndexSize = 4 + 16 * header.FileCount + header.pathLenSum + 4 + 8 * header.FileCount + 12;
                    //indexsize=(filecount)4byte+(pathlen+0x00000000+offset to begin+file size)16byte*filecount+pathlensum+(file count+1)4byte+8*filecount+(0x00000000)4byte*2+(offsettablebegin-0x7)4byte

                    //write header
                    MemoryStream ms6 = new MemoryStream((int)(header.IndexSize + Marshal.SizeOf<Header>()));
                    BinaryWriter writer6 = new BinaryWriter(ms6);
                    writer6.Write(Encoding.ASCII.GetBytes(header.Magic));
                    writer6.Write(Encoding.ASCII.GetBytes(header.Version));
                    writer6.Write(header.IndexSize);
                    writer6.Write(header.FileCount);

                    //write entry
                    uint offset6 = header.IndexSize + 7;
                    foreach (var file in index)
                    {
                        uint filenameSize = (uint)file.pathLen;//use utf-8 for japanese character in file name
                        writer6.Write(filenameSize);
                        writer6.Write(Global.PackEncoding.GetBytes(file.path));
                        writer6.Write(0); // reserved
                        writer6.Write(offset6);
                        writer6.Write(file.Size);
                        offset6 += file.Size;
                    }

                    long posOffsetTable6 = ms6.Position;
                    uint offsetCount6 = header.FileCount + 1;
                    writer6.Write(offsetCount6);//filecount + 1
                    uint total6 = 4;

                    //write table
                    foreach (var file in index)
                    {
                        total6 = total6 + 4 + (uint)file.pathLen;//use utf-8 for japanese character in file name
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
                        byte[] fileData = File.ReadAllBytes(file.filePath);
                        arc2.Write(fileData, 0, fileData.Length);

                        LogUtility.UpdateBar();
                    }
                    arc2.Close();
                    return;
            }
        }
    }
}