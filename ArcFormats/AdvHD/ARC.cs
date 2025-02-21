using ArcFormats.Properties;
using GalArc.Controls;
using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utility;
using Utility.Extensions;

namespace ArcFormats.AdvHD
{
    public class ARC : ArchiveFormat
    {
        private static readonly Lazy<OptionsTemplate> _lazyUnpackOptions = new Lazy<OptionsTemplate>(() => new UnpackARCOptions());
        private static readonly Lazy<OptionsTemplate> _lazyPackOptions = new Lazy<OptionsTemplate>(() => new PackARCOptions());

        public static OptionsTemplate UnpackExtraOptions => _lazyUnpackOptions.Value;
        public static OptionsTemplate PackExtraOptions => _lazyPackOptions.Value;

        private readonly string[] EncryptedFileExtV1 = { "wsc", "scr" };

        private readonly string[] EncryptedFileExtV2 = { "ws2", "json" };

        private class HeaderV1
        {
            public uint TypeCount { get; set; }
            public uint FileCountAll { get; set; }
        }

        private class TypeHeaderV1
        {
            public string Extension { get; set; }
            public uint FileCount { get; set; }
            public uint IndexOffset { get; set; }
        }

        private void UnpackV1(string filePath, string folderPath)
        {
            HeaderV1 header = new HeaderV1();
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            header.FileCountAll = 0;
            header.TypeCount = br.ReadUInt32();
            List<TypeHeaderV1> typeHeaders = new List<TypeHeaderV1>();
            Directory.CreateDirectory(folderPath);

            for (int i = 0; i < header.TypeCount; i++)
            {
                TypeHeaderV1 typeHeader = new TypeHeaderV1();
                typeHeader.Extension = Encoding.ASCII.GetString(br.ReadBytes(3));
                br.ReadByte();
                typeHeader.FileCount = br.ReadUInt32();
                typeHeader.IndexOffset = br.ReadUInt32();
                typeHeaders.Add(typeHeader);
                header.FileCountAll += typeHeader.FileCount;
            }
            Logger.InitBar(header.FileCountAll);

            for (int i = 0; i < header.TypeCount; i++)
            {
                for (int j = 0; j < typeHeaders[i].FileCount; j++)
                {
                    Entry index = new Entry();
                    index.Name = Encoding.ASCII.GetString(br.ReadBytes(13)).Replace("\0", string.Empty) + "." + typeHeaders[i].Extension;
                    index.Size = br.ReadUInt32();
                    index.Offset = br.ReadUInt32();
                    long pos = fs.Position;
                    index.Path = Path.Combine(folderPath, index.Name);
                    fs.Seek(index.Offset, SeekOrigin.Begin);
                    byte[] buffer = br.ReadBytes((int)index.Size);
                    if (UnpackARCOptions.DecryptScripts && IsScriptFile(Path.GetExtension(index.Path), "1"))
                    {
                        Logger.Debug(string.Format(Resources.logTryDecScr, index.Name));
                        DecryptScript(buffer);
                    }
                    File.WriteAllBytes(index.Path, buffer);
                    buffer = null;
                    fs.Seek(pos, SeekOrigin.Begin);
                    Logger.UpdateBar();
                }
            }
            fs.Dispose();
            br.Dispose();
        }

        private void PackV1(string folderPath, string filePath)
        {
            HashSet<string> uniqueExtension = new HashSet<string>();

            HeaderV1 header = new HeaderV1();
            header.FileCountAll = (uint)Utils.GetFileCount(folderPath);
            Logger.InitBar(header.FileCountAll);
            string[] exts = Utils.GetFileExtensions(folderPath);
            Array.Sort(exts, StringComparer.Ordinal);
            int extCount = exts.Length;

            header.TypeCount = (uint)extCount;
            int length = 13;
            DirectoryInfo d = new DirectoryInfo(folderPath);
            List<TypeHeaderV1> typeHeaders = new List<TypeHeaderV1>();
            int[] type_fileCount = new int[extCount];

            FileStream fw = File.Create(filePath);
            MemoryStream mshead = new MemoryStream();
            MemoryStream mstype = new MemoryStream();
            MemoryStream msentry = new MemoryStream();
            MemoryStream msdata = new MemoryStream();
            BinaryWriter bwhead = new BinaryWriter(mshead);
            BinaryWriter bwtype = new BinaryWriter(mstype);
            BinaryWriter bwentry = new BinaryWriter(msentry);
            BinaryWriter bwdata = new BinaryWriter(msdata);

            bwhead.Write(header.TypeCount);
            uint pos = (uint)(12 * extCount + 4);

            for (int i = 0; i < extCount; i++)
            {
                foreach (FileInfo file in d.GetFiles($"*{exts[i]}"))
                {
                    type_fileCount[i]++;
                    bwentry.Write(Encoding.ASCII.GetBytes(Path.GetFileNameWithoutExtension(file.Name).PadRight(length, '\0')));
                    bwentry.Write((uint)file.Length);
                    bwentry.Write((uint)(4 + 12 * header.TypeCount + 21 * header.FileCountAll + msdata.Length));
                    byte[] buffer = File.ReadAllBytes(file.FullName);
                    if (PackARCOptions.EncryptScripts && IsScriptFile(exts[i], "1"))
                    {
                        Logger.Debug(string.Format(Resources.logTryEncScr, file.Name));
                        EncryptScript(buffer);
                    }
                    bwdata.Write(buffer);
                    buffer = null;
                    Logger.UpdateBar();
                }
                bwtype.Write(Encoding.ASCII.GetBytes(exts[i]));
                bwtype.Write((byte)0);
                bwtype.Write((uint)type_fileCount[i]);
                bwtype.Write(pos);
                pos = (uint)(12 * extCount + 4 + msentry.Length);
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

        private class HeaderV2
        {
            public uint FileCount { get; set; }
            public uint EntrySize { get; set; }
        }

        private void UnpackV2(string filePath, string folderPath)
        {
            //init
            HeaderV2 header = new HeaderV2();
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br1 = new BinaryReader(fs);
            List<Entry> l = new List<Entry>();

            Directory.CreateDirectory(folderPath);

            header.FileCount = br1.ReadUInt32();
            header.EntrySize = br1.ReadUInt32();
            Logger.InitBar(header.FileCount);

            for (int i = 0; i < header.FileCount; i++)
            {
                Entry entry = new Entry();
                entry.Size = br1.ReadUInt32();
                entry.Offset = br1.ReadUInt32() + 8 + header.EntrySize;
                entry.Name = br1.ReadCString(Encoding.Unicode);
                entry.Path = Path.Combine(folderPath, entry.Name);

                l.Add(entry);
            }

            foreach (var entry in l)
            {
                byte[] buffer = br1.ReadBytes((int)entry.Size);
                if (UnpackARCOptions.DecryptScripts && IsScriptFile(Path.GetExtension(entry.Path), "2"))
                {
                    Logger.Debug(string.Format(Resources.logTryDecScr, entry.Name));
                    DecryptScript(buffer);
                }
                File.WriteAllBytes(entry.Path, buffer);
                buffer = null;
                Logger.UpdateBar();
            }
            fs.Dispose();
            br1.Dispose();
        }

        private void PackV2(string folderPath, string filePath)
        {
            HeaderV2 header = new HeaderV2();
            List<Entry> l = new List<Entry>();
            uint sizeToNow = 0;

            //make header
            DirectoryInfo d = new DirectoryInfo(folderPath);
            FileInfo[] files = d.GetFiles();
            header.FileCount = (uint)files.Length;
            header.EntrySize = 0;
            Logger.InitBar(header.FileCount);

            foreach (FileInfo file in files)
            {
                Entry entry = new Entry();
                entry.Name = file.Name;
                entry.Size = (uint)file.Length;
                entry.Offset = sizeToNow;
                sizeToNow += entry.Size;
                l.Add(entry);

                int nameLength = entry.Name.Length;
                header.EntrySize = header.EntrySize + (uint)nameLength * 2 + 2 + 8;
            }

            using (FileStream fs = File.Create(filePath))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    //write header
                    bw.Write(header.FileCount);
                    bw.Write(header.EntrySize);

                    //write entry
                    foreach (var file in l)
                    {
                        bw.Write(file.Size);
                        bw.Write(file.Offset);
                        bw.Write(Encoding.Unicode.GetBytes(file.Name));
                        bw.Write('\0');
                        bw.Write('\0');
                    }

                    //write data
                    foreach (FileInfo file in files)
                    {
                        byte[] buffer = File.ReadAllBytes(file.FullName);
                        if (PackARCOptions.EncryptScripts && IsScriptFile(file.Extension, "2"))
                        {
                            Logger.Debug(string.Format(Resources.logTryEncScr, file.Name));
                            EncryptScript(buffer);
                        }
                        bw.Write(buffer);
                        buffer = null;
                        Logger.UpdateBar();
                    }
                }
            }
        }

        public override void Unpack(string filePath, string folderPath)
        {
            List<Action> actions = new List<Action>()
            {
                () => UnpackV2(filePath, folderPath),
                () => UnpackV1(filePath, folderPath)
            };
            foreach (var action in actions)
            {
                try
                {
                    action();
                    return;
                }
                catch
                { }
            }
            Logger.ErrorInvalidArchive();
        }

        public override void Pack(string folderPath, string filePath)
        {
            switch (PackExtraOptions.Version)
            {
                case "1":
                    PackV1(folderPath, filePath);
                    break;
                case "2":
                    PackV2(folderPath, filePath);
                    break;
                default:
                    Logger.Error("Wrong version specified.");
                    break;
            }
        }

        private bool IsScriptFile(string extension, string version)
        {
            string trimed = extension.TrimStart('.');
            switch (version)
            {
                case "1":
                    return EncryptedFileExtV1.Contains(trimed);
                case "2":
                    return EncryptedFileExtV2.Contains(trimed);
                default:
                    return false;
            }
        }

        private void DecryptScript(byte[] data)
        {
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = Binary.RotByteR(data[i], 2);
            }
        }

        private void EncryptScript(byte[] data)
        {
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = Binary.RotByteL(data[i], 2);
            }
        }
    }
}