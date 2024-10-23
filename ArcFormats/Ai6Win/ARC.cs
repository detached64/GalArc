using Log;
using System;
using System.Collections.Generic;
using System.IO;
using Utility;
using Utility.Compression;
using Utility.Extensions;

namespace ArcFormats.Ai6Win
{
    public class ARC
    {
        private class Entry
        {
            public string Name { get; set; }
            public string FilePath { get; set; }
            public uint PackedSize { get; set; }
            public uint UnpackedSize { get; set; }
            public uint Offset { get; set; }
            public bool IsPacked { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            List<Action> actions = new List<Action>
            {
                () => UnpackAi6Win(filePath, folderPath),
                () => UnpackArcNew(filePath, folderPath),

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
            LogUtility.ErrorInvalidArchive();
        }

        private static void UnpackAi6Win(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            int fileCount = br.ReadInt32();

            List<Entry> l = new List<Entry>();
            for (int i = 0; i < fileCount; i++)
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
                entry.Name = ArcEncoding.Shift_JIS.GetString(nameBuf, 0, nameLen);
                if (entry.Name.ContainsInvalidChars())
                {
                    throw new Exception();
                }
                entry.FilePath = Path.Combine(folderPath, entry.Name);
                entry.PackedSize = BigEndian.Convert(br.ReadUInt32());
                entry.UnpackedSize = BigEndian.Convert(br.ReadUInt32());
                entry.Offset = BigEndian.Convert(br.ReadUInt32());
                entry.IsPacked = entry.PackedSize != entry.UnpackedSize;
                l.Add(entry);
            }

            LogUtility.InitBar(fileCount);
            Directory.CreateDirectory(folderPath);
            ExtractData(l, br);

            fs.Dispose();
            br.Dispose();
        }

        private static void UnpackArcNew(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            int indexSize = br.ReadInt32();
            List<Entry> l = new List<Entry>();

            while (fs.Position < 4 + indexSize)
            {
                int nameLen = br.ReadByte();
                Entry entry = new Entry();
                byte[] nameBuf = br.ReadBytes(nameLen);
                DecryptName(nameBuf, (byte)nameLen);
                entry.Name = ArcEncoding.Shift_JIS.GetString(nameBuf);
                if (entry.Name.ContainsInvalidChars())
                {
                    throw new Exception();
                }
                entry.FilePath = Path.Combine(folderPath, entry.Name);
                entry.PackedSize = BigEndian.Convert(br.ReadUInt32());
                entry.UnpackedSize = BigEndian.Convert(br.ReadUInt32());
                entry.Offset = BigEndian.Convert(br.ReadUInt32());
                entry.IsPacked = entry.PackedSize != entry.UnpackedSize;
                l.Add(entry);
            }

            LogUtility.InitBar(l.Count);
            Directory.CreateDirectory(folderPath);
            ExtractData(l, br);

            fs.Dispose();
            br.Dispose();
        }

        private static void DecryptName(byte[] name, byte key)
        {
            for (int i = 0; i < name.Length; i++)
            {
                name[i] += key--;
            }
        }

        private static void ExtractData(List<Entry> l, BinaryReader br)
        {
            for (int i = 0; i < l.Count; i++)
            {
                byte[] data = br.ReadBytes((int)l[i].PackedSize);
                if (l[i].IsPacked)
                {
                    File.WriteAllBytes(Path.Combine(l[i].FilePath), Lzss.Decompress(data));
                }
                else
                {
                    File.WriteAllBytes(Path.Combine(l[i].FilePath), data);
                }
                data = null;
                LogUtility.UpdateBar();
            }
        }
    }
}