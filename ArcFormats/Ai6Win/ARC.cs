using GalArc.Controls;
using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using Utility;
using Utility.Compression;
using Utility.Extensions;

namespace ArcFormats.Ai6Win
{
    public class ARC : ArchiveFormat
    {
        public override OptionsTemplate PackExtraOptions => PackARCOptions.Instance;

        private class ArcEntry : PackedEntry
        {
            public string FullPath { get; set; }
        }

        public override void Unpack(string filePath, string folderPath)
        {
            List<Action> actions = new List<Action>
            {
                () => UnpackV3(filePath, folderPath),
                () => UnpackV2(filePath, folderPath),
                () => UnpackV1(filePath, folderPath),
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
                case "3":
                    PackV3(folderPath, filePath);
                    break;
            }
        }

        private void UnpackV1(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            int fileCount = br.ReadInt32();

            List<ArcEntry> l = new List<ArcEntry>();
            for (int i = 0; i < fileCount; i++)
            {
                ArcEntry entry = new ArcEntry();
                entry.Name = ArcEncoding.Shift_JIS.GetString(br.ReadBytes(32)).TrimEnd('\0');
                if (entry.Name.ContainsInvalidChars())
                {
                    throw new Exception();
                }
                entry.FullPath = Path.Combine(folderPath, entry.Name);
                entry.Offset = br.ReadUInt32();
                entry.Size = br.ReadUInt32();
                entry.IsPacked = false;
                l.Add(entry);
            }

            Logger.InitBar(fileCount);
            Directory.CreateDirectory(folderPath);
            Logger.ShowVersion("arc", 1);

            ExtractData(l, br);

            fs.Dispose();
            br.Dispose();
        }

        private void UnpackV2(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            int fileCount = br.ReadInt32();

            List<ArcEntry> l = new List<ArcEntry>();
            for (int i = 0; i < fileCount; i++)
            {
                ArcEntry entry = new ArcEntry();
                byte[] nameBuf = br.ReadBytes(260);
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
                entry.FullPath = Path.Combine(folderPath, entry.Name);
                entry.Size = BigEndian.Convert(br.ReadUInt32());
                entry.UnpackedSize = BigEndian.Convert(br.ReadUInt32());
                entry.Offset = BigEndian.Convert(br.ReadUInt32());
                entry.IsPacked = entry.Size != entry.UnpackedSize;
                l.Add(entry);
            }

            Logger.InitBar(fileCount);
            Directory.CreateDirectory(folderPath);
            Logger.ShowVersion("arc", 2);

            ExtractData(l, br);

            fs.Dispose();
            br.Dispose();
        }

        private void UnpackV3(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            int indexSize = br.ReadInt32();
            List<ArcEntry> l = new List<ArcEntry>();

            while (fs.Position < 4 + indexSize)
            {
                int nameLen = br.ReadByte();
                ArcEntry entry = new ArcEntry();
                byte[] nameBuf = br.ReadBytes(nameLen);
                byte key = (byte)nameLen;
                for (int i = 0; i < nameBuf.Length; i++)
                {
                    nameBuf[i] += key--;
                }
                entry.Name = ArcEncoding.Shift_JIS.GetString(nameBuf);
                if (entry.Name.ContainsInvalidChars())
                {
                    throw new Exception();
                }
                entry.FullPath = Path.Combine(folderPath, entry.Name);
                entry.Size = BigEndian.Convert(br.ReadUInt32());
                entry.UnpackedSize = BigEndian.Convert(br.ReadUInt32());
                entry.Offset = BigEndian.Convert(br.ReadUInt32());
                entry.IsPacked = entry.Size != entry.UnpackedSize;
                l.Add(entry);
            }

            Logger.InitBar(l.Count);
            Directory.CreateDirectory(folderPath);
            Logger.ShowVersion("arc", 3);

            ExtractData(l, br);

            fs.Dispose();
            br.Dispose();
        }

        private void ExtractData(List<ArcEntry> l, BinaryReader br)
        {
            for (int i = 0; i < l.Count; i++)
            {
                byte[] data = br.ReadBytes((int)l[i].Size);
                if (l[i].IsPacked)
                {
                    data = LzssHelper.Decompress(data);
                }
                File.WriteAllBytes(Path.Combine(l[i].FullPath), data);
                data = null;
                Logger.UpdateBar();
            }
        }

        private void PackV1(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            FileInfo[] files = new DirectoryInfo(folderPath).GetFiles();
            int fileCount = files.Length;
            bw.Write(fileCount);
            uint baseOffset = 4 + 40 * (uint)fileCount;
            Logger.InitBar(fileCount);
            foreach (FileInfo file in files)
            {
                bw.WritePaddedString(file.Name, 32);
                bw.Write(baseOffset);
                uint size = (uint)file.Length;
                bw.Write(size);
                baseOffset += size;
            }
            foreach (FileInfo file in files)
            {
                byte[] data = File.ReadAllBytes(file.FullName);
                bw.Write(data);
                data = null;
                Logger.UpdateBar();
            }
            fw.Dispose();
            bw.Dispose();
        }

        private void PackV2(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            FileInfo[] files = new DirectoryInfo(folderPath).GetFiles();
            int fileCount = files.Length;
            bw.Write(fileCount);
            uint dataOffset = 4 + 272 * (uint)fileCount;
            Logger.InitBar(fileCount);
            foreach (FileInfo file in files)
            {
                byte[] nameBuffer = Utils.GetPaddedBytes(file.Name, 260);
                int nameLen = Array.IndexOf<byte>(nameBuffer, 0);
                if (nameLen == -1)
                {
                    nameLen = nameBuffer.Length;
                }

                byte key = (byte)(nameLen + 1);
                for (int i = 0; i < nameLen; i++)
                {
                    nameBuffer[i] += key--;
                }
                bw.Write(nameBuffer);
                uint unpackedSize = (uint)file.Length;
                byte[] data = File.ReadAllBytes(file.FullName);
                if (PackARCOptions.CompressContents)
                {
                    data = LzssHelper.Compress(data);
                }
                uint packedSize = (uint)data.Length;
                bw.Write(BigEndian.Convert(packedSize));
                bw.Write(BigEndian.Convert(unpackedSize));
                bw.Write(BigEndian.Convert(dataOffset));
                long pos = fw.Position;
                fw.Position = dataOffset;
                bw.Write(data);
                fw.Position = pos;
                dataOffset += packedSize;
                data = null;
                Logger.UpdateBar();
            }
            fw.Dispose();
            bw.Dispose();
        }

        private void PackV3(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            FileInfo[] files = new DirectoryInfo(folderPath).GetFiles();
            int fileCount = files.Length;
            Logger.InitBar(fileCount);
            uint dataOffset = (uint)(4 + Utils.GetNameLengthSum(files, ArcEncoding.Shift_JIS) + 13 * fileCount);
            bw.Write(dataOffset - 4);

            foreach (FileInfo file in files)
            {
                byte[] nameBuf = ArcEncoding.Shift_JIS.GetBytes(file.Name);
                byte key = (byte)nameBuf.Length;
                bw.Write(key);
                for (int i = 0; i < nameBuf.Length; i++)
                {
                    nameBuf[i] -= key--;
                }
                bw.Write(nameBuf);
                uint unpackedSize = (uint)file.Length;
                byte[] data = File.ReadAllBytes(file.FullName);
                if (PackARCOptions.CompressContents)
                {
                    data = LzssHelper.Compress(data);
                }
                uint packedSize = (uint)data.Length;
                bw.Write(BigEndian.Convert(packedSize));
                bw.Write(BigEndian.Convert(unpackedSize));
                bw.Write(BigEndian.Convert(dataOffset));
                long pos = fw.Position;
                fw.Position = dataOffset;
                bw.Write(data);
                fw.Position = pos;
                dataOffset += packedSize;
                data = null;
                Logger.UpdateBar();
            }
            bw.Dispose();
            fw.Dispose();
        }
    }
}