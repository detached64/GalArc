using ArcFormats.Properties;
using ArcFormats.Templates;
using GalArc.Logs;
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
    public class PFS : ArchiveFormat
    {
        public static UserControl PackExtraOptions = new VersionOnly("8/6/2");

        private readonly string[] Versions = { "8", "6", "2" };

        private class Header
        {
            public string Magic { get; } = "pf";
            public string Version { get; set; }
            public uint IndexSize { get; set; }
            public uint FileCount { get; set; }
            public uint PathLenSum { get; set; }
        }

        private class PfsEntry : ArcFormats.Entry
        {
            public int RelativePathLen { get; set; }
            public string RelativePath { get; set; }
        }

        public override void Unpack(string filePath, string folderPath)
        {
            //init
            Header header = new Header();

            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);

            if (Encoding.ASCII.GetString(br.ReadBytes(2)) != header.Magic)
            {
                Logger.ErrorInvalidArchive();
            }
            header.Version = br.ReadChar().ToString();
            if (!Versions.Contains(header.Version))
            {
                Logger.Error(string.Format(Resources.logErrorNotSupportedVersion, "pfs", header.Version));
            }
            Logger.ShowVersion("pfs", header.Version);
            // read header
            header.IndexSize = br.ReadUInt32();
            if (header.Version == "2")
            {
                br.ReadUInt32();
            }
            header.FileCount = br.ReadUInt32();
            Logger.InitBar(header.FileCount);
            // compute key
            byte[] key = new byte[20];  // SHA1 hash of index
            if (header.Version == "8")
            {
                fs.Position = 7;
                using (SHA1 sha = SHA1.Create())
                {
                    key = sha.ComputeHash(br.ReadBytes((int)header.IndexSize));
                }
                fs.Position = 11;
            }
            // read index and save files
            for (int i = 0; i < header.FileCount; i++)
            {
                PfsEntry entry = new PfsEntry();
                entry.RelativePathLen = br.ReadInt32();
                string name = ArcSettings.Encoding.GetString(br.ReadBytes(entry.RelativePathLen));
                if (name.ContainsInvalidChars())
                {
                    throw new Exception(Resources.logErrorContainsInvalid);
                }
                entry.Path = Path.Combine(folderPath, name);

                br.ReadUInt32();
                if (header.Version == "2")
                {
                    br.ReadBytes(8);
                }
                entry.Offset = br.ReadUInt32();
                entry.Size = br.ReadUInt32();

                Utils.CreateParentDirectoryIfNotExists(entry.Path);

                long pos = fs.Position;
                fs.Position = entry.Offset;
                byte[] buffer = br.ReadBytes((int)entry.Size);
                if (header.Version == "8")
                {
                    for (int j = 0; j < buffer.Length; j++)
                    {
                        buffer[j] ^= key[j % 20];
                    }
                }
                File.WriteAllBytes(entry.Path, buffer);
                buffer = null;
                fs.Position = pos;
                Logger.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        public override void Pack(string folderPath, string filePath)
        {
            //init
            Header header = new Header()
            {
                Version = ArcSettings.Version,
                PathLenSum = 0
            };
            List<PfsEntry> entries = new List<PfsEntry>();
            string[] files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
            string[] relativePaths = Utils.GetRelativePaths(files, folderPath);
            header.PathLenSum = (uint)Utils.GetLengthSum(relativePaths, ArcSettings.Encoding);
            header.FileCount = (uint)files.Length;
            Logger.InitBar(header.FileCount);
            Utils.Sort(relativePaths);

            //add entry
            for (int i = 0; i < header.FileCount; i++)
            {
                PfsEntry entry = new PfsEntry();
                entry.Path = Path.Combine(folderPath, relativePaths[i]);
                entry.Size = (uint)new FileInfo(entry.Path).Length;
                entry.RelativePath = relativePaths[i];
                entry.RelativePathLen = ArcSettings.Encoding.GetByteCount(relativePaths[i]);
                entries.Add(entry);
            }

            switch (ArcSettings.Version)
            {
                case "8":
                    header.IndexSize = 4 + 16 * header.FileCount + header.PathLenSum + 4 + 8 * header.FileCount + 12;

                    //write header
                    MemoryStream ms8 = new MemoryStream();
                    BinaryWriter writer8 = new BinaryWriter(ms8);
                    writer8.Write(Encoding.ASCII.GetBytes(header.Magic));
                    writer8.Write(Encoding.ASCII.GetBytes(header.Version));
                    writer8.Write(header.IndexSize);
                    writer8.Write(header.FileCount);

                    //write entry
                    uint offset8 = header.IndexSize + 7;

                    foreach (var file in entries)
                    {
                        writer8.Write(file.RelativePathLen);
                        writer8.Write(ArcSettings.Encoding.GetBytes(file.RelativePath));
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
                    foreach (var entry in entries)
                    {
                        total = total + 4 + (uint)entry.RelativePathLen;
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
                        byte[] fileData = File.ReadAllBytes(entry.Path);
                        for (int i = 0; i < fileData.Length; i++)
                        {
                            fileData[i] ^= key[i % 20];
                        }
                        fw8.Write(fileData, 0, fileData.Length);
                        fileData = null;
                        Logger.UpdateBar();
                    }
                    fw8.Dispose();
                    ms8.Dispose();
                    writer8.Dispose();
                    return;

                case "2":
                    header.IndexSize = 8 + 24 * header.FileCount + header.PathLenSum;

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
                    foreach (var file in entries)
                    {
                        writer2.Write((uint)file.RelativePathLen);
                        writer2.Write(ArcSettings.Encoding.GetBytes(file.RelativePath));
                        writer2.Write((uint)16);
                        writer2.Write((uint)0);
                        writer2.Write((uint)0);
                        writer2.Write(offset2);
                        writer2.Write(file.Size);
                        offset2 += file.Size;
                    }

                    //write data
                    FileStream fw2 = File.Create(filePath);
                    ms2.WriteTo(fw2);

                    foreach (var file in entries)
                    {
                        byte[] fileData = File.ReadAllBytes(file.Path);
                        fw2.Write(fileData, 0, fileData.Length);
                        fileData = null;
                        Logger.UpdateBar();
                    }
                    fw2.Dispose();
                    ms2.Dispose();
                    writer2.Dispose();
                    return;

                case "6":
                    header.IndexSize = 4 + 16 * header.FileCount + header.PathLenSum + 4 + 8 * header.FileCount + 12;
                    //indexsize=(filecount)4byte+(pathlen+0x00000000+offset to begin+file size)16byte*filecount+pathlensum+(file count+1)4byte+8*filecount+(0x00000000)4byte*2+(offsettablebegin-0x7)4byte

                    //write header
                    MemoryStream ms6 = new MemoryStream();
                    BinaryWriter writer6 = new BinaryWriter(ms6);
                    writer6.Write(Encoding.ASCII.GetBytes(header.Magic));
                    writer6.Write(Encoding.ASCII.GetBytes(header.Version));
                    writer6.Write(header.IndexSize);
                    writer6.Write(header.FileCount);

                    //write entry
                    uint offset6 = header.IndexSize + 7;
                    foreach (var file in entries)
                    {
                        uint filenameSize = (uint)file.RelativePathLen;//use utf-8 for japanese character in file name
                        writer6.Write(filenameSize);
                        writer6.Write(ArcSettings.Encoding.GetBytes(file.RelativePath));
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
                    foreach (var file in entries)
                    {
                        total6 = total6 + 4 + (uint)file.RelativePathLen;//use utf-8 for japanese character in file name
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
                        byte[] fileData = File.ReadAllBytes(file.Path);
                        fw6.Write(fileData, 0, fileData.Length);
                        fileData = null;
                        Logger.UpdateBar();
                    }
                    fw6.Dispose();
                    ms6.Dispose();
                    writer6.Dispose();
                    return;
            }
        }
    }
}