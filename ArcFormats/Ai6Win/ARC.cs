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
        private class Entry
        {
            public string name;
            public string filePath;
            public uint packedSize;
            public uint unpackedSize;
            public uint offset;
            public bool isPacked;
        }

        public static void Unpack(string filePath, string folderPath)
        {
            List<Action> actions = new List<Action>
            {
                () => UnpackAi6Win(filePath, folderPath),
                () => UnpackArcNew(filePath, folderPath),

            };
            int methodIndex = 0;
            while (methodIndex < actions.Count)
            {
                try
                {
                    actions[methodIndex]();
                    return;
                }
                catch
                {
                    methodIndex++;
                }
            }
            LogUtility.Error_NotValidArchive();
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
                entry.name = ArcEncoding.Shift_JIS.GetString(nameBuf, 0, nameLen);
                entry.filePath = Path.Combine(folderPath, entry.name);
                entry.packedSize = BigEndian.Read(br.ReadUInt32());
                entry.unpackedSize = BigEndian.Read(br.ReadUInt32());
                entry.offset = BigEndian.Read(br.ReadUInt32());
                entry.isPacked = entry.packedSize != entry.unpackedSize;
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
                entry.name = ArcEncoding.Shift_JIS.GetString(nameBuf);
                entry.filePath = Path.Combine(folderPath, entry.name);
                entry.packedSize = BigEndian.Read(br.ReadUInt32());
                entry.unpackedSize = BigEndian.Read(br.ReadUInt32());
                entry.offset = BigEndian.Read(br.ReadUInt32());
                entry.isPacked = entry.packedSize != entry.unpackedSize;
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
                byte[] data = br.ReadBytes((int)l[i].packedSize);
                if (l[i].isPacked)
                {
                    File.WriteAllBytes(Path.Combine(l[i].filePath), Lzss.Decompress(data));
                }
                else
                {
                    File.WriteAllBytes(Path.Combine(l[i].filePath), data);
                }
                LogUtility.UpdateBar();
            }
        }
    }
}