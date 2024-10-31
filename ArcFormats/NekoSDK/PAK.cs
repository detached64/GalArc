using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utility;
using Utility.Compression;

namespace ArcFormats.NekoSDK
{
    public class PAK
    {
        private static readonly string Magic = "NEKOPACK";

        private class Entry
        {
            public string Name { get; set; }
            public uint Offset { get; set; }
            public uint Size { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            if (Encoding.ASCII.GetString(br.ReadBytes(8)) != Magic)
            {
                Logger.ErrorInvalidArchive();
            }
            string version = Encoding.ASCII.GetString(br.ReadBytes(2));
            uint dataOffset;
            switch (version)
            {
                case "4A":
                    dataOffset = 10 + br.ReadUInt32();
                    break;
                case "4S":
                    dataOffset = 16 + (uint)br.ReadUInt16();
                    break;
                default:
                    Logger.Error("Unknown version: " + version);
                    return;
            }
            List<Entry> entries = new List<Entry>();

            while (fs.Position < dataOffset)
            {
                Entry entry = new Entry();
                int nameLen = br.ReadInt32();
                byte[] nameBuf = br.ReadBytes(nameLen);
                entry.Name = ArcEncoding.Shift_JIS.GetString(nameBuf).TrimEnd('\0');

                int key = 0;
                for (int i = 0; i < nameLen; ++i)
                {
                    key += (sbyte)nameBuf[i];
                }
                entry.Offset = br.ReadUInt32() ^ (uint)key;
                entry.Size = br.ReadUInt32() ^ (uint)key;
                entries.Add(entry);
            }
            Logger.InitBar(entries.Count);
            Directory.CreateDirectory(folderPath);

            foreach (var entry in entries)
            {
                fs.Position = entry.Offset;
                byte[] header = br.ReadBytes(4);
                byte[] data = br.ReadBytes((int)(entry.Size - 8));
                uint key = entry.Size / 8 + 0x22;
                for (int i = 0; i < 4; i++)
                {
                    header[i] ^= (byte)key;
                    key <<= 3;
                }
                byte[] fileData = Zlib.DecompressBytes(header.Concat(data).ToArray());
                File.WriteAllBytes(Path.Combine(folderPath, entry.Name), fileData);
                fileData = null;
                data = null;
                Logger.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(Encoding.ASCII.GetBytes(Magic));
            writer.Write(Encoding.ASCII.GetBytes("4A"));
            DirectoryInfo d = new DirectoryInfo(folderPath);
            FileInfo[] files = d.GetFiles();

            Logger.InitBar(files.Length);
            uint baseOffset = (uint)files.Length * 12 + 18;

            foreach (var file in files)
            {
                baseOffset += (uint)ArcEncoding.Shift_JIS.GetByteCount(file.Name) + 1;
            }
            writer.Write(baseOffset);
            fw.Position = baseOffset;

            foreach (var file in files)
            {
                string name = file.Name;
                byte[] nameBuf = ArcEncoding.Shift_JIS.GetBytes(name);
                writer.Write(nameBuf.Length + 1);
                writer.Write(nameBuf);
                writer.Write((byte)0);
                int key = 0;
                for (int i = 0; i < nameBuf.Length; ++i)
                {
                    key += (sbyte)nameBuf[i];
                }
                writer.Write(baseOffset ^ (uint)key);

                byte[] data = Zlib.CompressFile(file.FullName);
                uint size = (uint)data.Length + 4;
                uint dataKey = size / 8 + 0x22;
                for (int i = 0; i < 4; i++)
                {
                    data[i] ^= (byte)dataKey;
                    dataKey <<= 3;
                }
                bw.Write(data);
                data = null;
                bw.Write((uint)file.Length);

                writer.Write(size ^ (uint)key);
                baseOffset += size;
                Logger.UpdateBar();
            }
            writer.Write(0);
            fw.Position = 0;
            stream.WriteTo(fw);

            fw.Dispose();
            bw.Dispose();
            stream.Dispose();
            writer.Dispose();
        }
    }
}
