﻿using GalArc.Controls;
using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility;
using Utility.Extensions;

namespace ArcFormats.Majiro
{
    public class ARC : ArchiveFormat
    {
        private static readonly Lazy<OptionsTemplate> _lazyUnpackOptions = new Lazy<OptionsTemplate>(() => new UnpackARCOptions());
        public static OptionsTemplate UnpackExtraOptions => _lazyUnpackOptions.Value;

        private static readonly Lazy<OptionsTemplate> _lazyPackOptions = new Lazy<OptionsTemplate>(() => new PackARCOptions());
        public static OptionsTemplate PackExtraOptions => _lazyPackOptions.Value;

        private const string Magic = "MajiroArcV";
        private const string MagicV1 = "MajiroArcV1.000\x00";
        private const string MagicV2 = "MajiroArcV2.000\x00";
        private const string MagicV3 = "MajiroArcV3.000\x00";
        private const string ScriptMagicDec = "MajiroObjV1.000\x00";
        private const string ScriptMagicEnc = "MajiroObjX1.000\x00";

        private class MajiroEntry : Entry
        {
            public ulong Hash;
        }

        private int UnpackVersion;

        private int PackVersion;

        public override void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            string magic = Encoding.ASCII.GetString(br.ReadBytes(10));
            UnpackVersion = br.ReadByte() - '0';
            fs.Dispose();
            br.Dispose();
            if (magic == Magic)
            {
                Logger.ShowVersion("arc", UnpackVersion);
                switch (UnpackVersion)
                {
                    case 1:
                        UnpackV1(filePath, folderPath);
                        break;
                    case 2:
                    case 3:
                        UnpackV2(filePath, folderPath);
                        break;
                }
            }
            else
            {
                Logger.ErrorInvalidArchive();
            }
        }

        private void UnpackV1(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            fs.Position = 16;
            int fileCount = br.ReadInt32();
            uint nameOffset = br.ReadUInt32();
            uint dataOffset = br.ReadUInt32();

            uint indexLength = 8 * (uint)fileCount + 8;
            List<Entry> entries = new List<Entry>();
            Directory.CreateDirectory(folderPath);
            Logger.InitBar(fileCount);

            using (MemoryStream ms = new MemoryStream(br.ReadBytes((int)indexLength)))
            {
                using (BinaryReader indexReader = new BinaryReader(ms))
                {
                    for (int i = 0; i < fileCount; i++)
                    {
                        Entry entry = new Entry();
                        indexReader.ReadBytes(4);            //skip crc32
                        entry.Offset = indexReader.ReadUInt32();
                        entry.Name = br.ReadCString();
                        entries.Add(entry);
                    }
                    Entry lastEntry = new Entry();
                    indexReader.ReadBytes(4);               //skip 0x00000000
                    lastEntry.Offset = indexReader.ReadUInt32();
                    entries.Add(lastEntry);
                }
                for (int i = 0; i < fileCount; i++)
                {
                    byte[] data = br.ReadBytes((int)(entries[i + 1].Offset - entries[i].Offset));
                    if (UnpackARCOptions.DecryptScripts && Path.GetExtension(entries[i].Name) == ".mjo")
                    {
                        DecryptScript(data);
                    }
                    File.WriteAllBytes(Path.Combine(folderPath, entries[i].Name), data);
                    data = null;
                    Logger.UpdateBar();
                }
            }
            fs.Dispose();
            br.Dispose();
        }

        private void UnpackV2(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            fs.Position = 16;
            int fileCount = br.ReadInt32();
            uint nameOffset = br.ReadUInt32();
            uint dataOffset = br.ReadUInt32();

            uint indexLength = (uint)((UnpackVersion + 1) * 4 * fileCount);
            MemoryStream ms = new MemoryStream(br.ReadBytes((int)indexLength));
            BinaryReader brIndex = new BinaryReader(ms);

            Directory.CreateDirectory(folderPath);
            Logger.InitBar(fileCount);

            for (int i = 0; i < fileCount; i++)
            {
                MajiroEntry entry = new MajiroEntry();
                switch (UnpackVersion)
                {
                    case 2:
                        entry.Hash = brIndex.ReadUInt32();
                        break;
                    case 3:
                        entry.Hash = brIndex.ReadUInt64();
                        break;
                }
                entry.Offset = brIndex.ReadUInt32();
                entry.Size = brIndex.ReadUInt32();
                entry.Name = br.ReadCString();
                long pos = fs.Position;
                fs.Position = entry.Offset;
                byte[] data = br.ReadBytes((int)entry.Size);
                if (UnpackARCOptions.DecryptScripts && Path.GetExtension(entry.Name) == ".mjo")
                {
                    DecryptScript(data);
                }
                File.WriteAllBytes(Path.Combine(folderPath, entry.Name), data);
                data = null;
                fs.Position = pos;
                Logger.UpdateBar();
            }
            fs.Dispose();
            br.Dispose();
            brIndex.Dispose();
            ms.Dispose();
        }

        public override void Pack(string folderPath, string filePath)
        {
            PackVersion = int.Parse(PackExtraOptions.Version);
            switch (PackVersion)
            {
                case 1:
                    PackV1(folderPath, filePath);
                    break;
                case 2:
                case 3:
                    PackV2(folderPath, filePath);
                    break;
            }
        }

        private void PackV1(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            DirectoryInfo d = new DirectoryInfo(folderPath);
            FileInfo[] files = d.GetFiles();
            int fileCount = files.Length;
            Logger.InitBar(fileCount);
            bw.Write(Encoding.ASCII.GetBytes(MagicV1));
            bw.Write(fileCount);
            uint nameOffset = 28 + 8 * ((uint)fileCount + 1);
            uint dataOffset = 0;
            bw.Write(nameOffset);
            bw.Write(dataOffset);       // pos = 24

            // write name
            bw.BaseStream.Position = nameOffset;
            foreach (FileInfo file in files)
            {
                bw.Write(ArcEncoding.Shift_JIS.GetBytes(file.Name));
                bw.Write('\0');
            }
            // write data
            dataOffset = (uint)fw.Position;
            foreach (FileInfo file in files)
            {
                byte[] data = File.ReadAllBytes(file.FullName);
                if (PackARCOptions.EncryptScripts && file.Extension == ".mjo")
                {
                    EncryptScript(data);
                }
                bw.Write(data);
                data = null;
            }
            uint maxOffset = (uint)fw.Position;
            // write index
            bw.BaseStream.Position = 24;
            bw.Write(dataOffset);
            foreach (FileInfo file in files)
            {
                bw.Write(Crc32.Calculate(ArcEncoding.Shift_JIS.GetBytes(file.Name)));
                bw.Write(dataOffset);
                dataOffset += (uint)file.Length;
                Logger.UpdateBar();
            }
            bw.Write(0);
            bw.Write(maxOffset);
            bw.Dispose();
            fw.Dispose();
        }

        private void PackV2(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            DirectoryInfo d = new DirectoryInfo(folderPath);
            FileInfo[] files = d.GetFiles();
            int fileCount = files.Length;
            Logger.InitBar(fileCount, 3);

            bw.Write(Encoding.ASCII.GetBytes(PackVersion == 2 ? MagicV2 : MagicV3));
            bw.Write(fileCount);
            uint nameOffset = 28 + (uint)((PackVersion + 1) * 4 * fileCount);
            uint dataOffset = 0;
            bw.Write(nameOffset);
            bw.Write(dataOffset);   // Reserved

            List<MajiroEntry> entries = new List<MajiroEntry>();
            foreach (FileInfo file in files)
            {
                MajiroEntry entry = new MajiroEntry();
                entry.Name = file.Name;
                entry.Path = file.FullName;
                entry.Size = (uint)file.Length;
                switch (PackVersion)
                {
                    case 2:
                        entry.Hash = Crc32.Calculate(ArcEncoding.Shift_JIS.GetBytes(entry.Name));
                        break;
                    case 3:
                        entry.Hash = CalculateHash(ArcEncoding.Shift_JIS.GetBytes(entry.Name));
                        break;
                }
                entries.Add(entry);
            }
            entries.Sort((a, b) => a.Hash.CompareTo(b.Hash));

            // write name
            bw.BaseStream.Position = nameOffset;
            foreach (MajiroEntry entry in entries)
            {
                bw.Write(ArcEncoding.Shift_JIS.GetBytes(entry.Name));
                bw.Write('\0');
                Logger.UpdateBar();
            }

            // write data
            dataOffset = (uint)fw.Position;
            foreach (MajiroEntry entry in entries)
            {
                byte[] data = File.ReadAllBytes(entry.Path);
                if (PackARCOptions.EncryptScripts && Path.GetExtension(entry.Name) == ".mjo")
                {
                    EncryptScript(data);
                }
                bw.Write(data);
                data = null;
                Logger.UpdateBar();
            }

            // write index
            bw.BaseStream.Position = 24;
            bw.Write(dataOffset);
            foreach (MajiroEntry entry in entries)
            {
                if (PackVersion == 2)
                {
                    bw.Write((uint)entry.Hash);
                }
                else
                {
                    bw.Write(entry.Hash);
                }
                bw.Write(dataOffset);
                bw.Write(entry.Size);
                dataOffset += entry.Size;
                Logger.UpdateBar();
            }

            bw.Dispose();
            fw.Dispose();
        }

        private void DecryptScript(byte[] data)
        {
            if (Encoding.ASCII.GetString(data, 0, 16) != ScriptMagicEnc)
            {
                return;
            }
            data[9] = (byte)'V';
            int start = 0x28, end = data.Length - start;
            for (int i = 0; i < end; i++)
            {
                data[i + start] ^= key[i % key.Length];
            }
        }

        private void EncryptScript(byte[] data)
        {
            if (Encoding.ASCII.GetString(data, 0, 16) != ScriptMagicDec)
            {
                return;
            }
            data[9] = (byte)'X';
            int start = 0x28, end = data.Length - start;
            for (int i = 0; i < end; i++)
            {
                data[i + start] ^= key[i % key.Length];
            }
        }

        private ulong CalculateHash(byte[] bytes)
        {
            const ulong POLY = 0x85E1C3D753D46D27;
            uint a = 0xFFFFFFFF;
            uint b = 0xFFFFFFFF;

            ulong[] table = new ulong[256];
            for (int i = 0; i < 256; i++)
            {
                ulong hash = (ulong)i;
                for (int j = 0; j < 8; j++)
                {
                    if ((hash & 1) != 0)
                    {
                        hash ^= POLY;
                    }
                    hash >>= 1;
                }
                table[i] = hash;
            }

            for (int i = 0; i < bytes.Length; i++)
            {
                int index = (byte)a ^ bytes[i];
                ulong temp = (((ulong)b << 32) | a) >> 8;
                a = (uint)(temp ^ table[index]);
                b = (uint)((temp ^ table[index]) >> 32);
            }
            return ~(((ulong)b << 32) | a);
        }

        private readonly byte[] key =
        {
            0x00, 0x00, 0x00, 0x00, 0x96, 0x30, 0x07, 0x77, 0x2C, 0x61, 0x0E, 0xEE, 0xBA, 0x51, 0x09, 0x99, 0x19, 0xC4, 0x6D, 0x07, 0x8F, 0xF4, 0x6A, 0x70, 0x35, 0xA5, 0x63, 0xE9, 0xA3, 0x95, 0x64, 0x9E,
            0x32, 0x88, 0xDB, 0x0E, 0xA4, 0xB8, 0xDC, 0x79, 0x1E, 0xE9, 0xD5, 0xE0, 0x88, 0xD9, 0xD2, 0x97, 0x2B, 0x4C, 0xB6, 0x09, 0xBD, 0x7C, 0xB1, 0x7E, 0x07, 0x2D, 0xB8, 0xE7, 0x91, 0x1D, 0xBF, 0x90,
            0x64, 0x10, 0xB7, 0x1D, 0xF2, 0x20, 0xB0, 0x6A, 0x48, 0x71, 0xB9, 0xF3, 0xDE, 0x41, 0xBE, 0x84, 0x7D, 0xD4, 0xDA, 0x1A, 0xEB, 0xE4, 0xDD, 0x6D, 0x51, 0xB5, 0xD4, 0xF4, 0xC7, 0x85, 0xD3, 0x83,
            0x56, 0x98, 0x6C, 0x13, 0xC0, 0xA8, 0x6B, 0x64, 0x7A, 0xF9, 0x62, 0xFD, 0xEC, 0xC9, 0x65, 0x8A, 0x4F, 0x5C, 0x01, 0x14, 0xD9, 0x6C, 0x06, 0x63, 0x63, 0x3D, 0x0F, 0xFA, 0xF5, 0x0D, 0x08, 0x8D,
            0xC8, 0x20, 0x6E, 0x3B, 0x5E, 0x10, 0x69, 0x4C, 0xE4, 0x41, 0x60, 0xD5, 0x72, 0x71, 0x67, 0xA2, 0xD1, 0xE4, 0x03, 0x3C, 0x47, 0xD4, 0x04, 0x4B, 0xFD, 0x85, 0x0D, 0xD2, 0x6B, 0xB5, 0x0A, 0xA5,
            0xFA, 0xA8, 0xB5, 0x35, 0x6C, 0x98, 0xB2, 0x42, 0xD6, 0xC9, 0xBB, 0xDB, 0x40, 0xF9, 0xBC, 0xAC, 0xE3, 0x6C, 0xD8, 0x32, 0x75, 0x5C, 0xDF, 0x45, 0xCF, 0x0D, 0xD6, 0xDC, 0x59, 0x3D, 0xD1, 0xAB,
            0xAC, 0x30, 0xD9, 0x26, 0x3A, 0x00, 0xDE, 0x51, 0x80, 0x51, 0xD7, 0xC8, 0x16, 0x61, 0xD0, 0xBF, 0xB5, 0xF4, 0xB4, 0x21, 0x23, 0xC4, 0xB3, 0x56, 0x99, 0x95, 0xBA, 0xCF, 0x0F, 0xA5, 0xBD, 0xB8,
            0x9E, 0xB8, 0x02, 0x28, 0x08, 0x88, 0x05, 0x5F, 0xB2, 0xD9, 0x0C, 0xC6, 0x24, 0xE9, 0x0B, 0xB1, 0x87, 0x7C, 0x6F, 0x2F, 0x11, 0x4C, 0x68, 0x58, 0xAB, 0x1D, 0x61, 0xC1, 0x3D, 0x2D, 0x66, 0xB6,
            0x90, 0x41, 0xDC, 0x76, 0x06, 0x71, 0xDB, 0x01, 0xBC, 0x20, 0xD2, 0x98, 0x2A, 0x10, 0xD5, 0xEF, 0x89, 0x85, 0xB1, 0x71, 0x1F, 0xB5, 0xB6, 0x06, 0xA5, 0xE4, 0xBF, 0x9F, 0x33, 0xD4, 0xB8, 0xE8,
            0xA2, 0xC9, 0x07, 0x78, 0x34, 0xF9, 0x00, 0x0F, 0x8E, 0xA8, 0x09, 0x96, 0x18, 0x98, 0x0E, 0xE1, 0xBB, 0x0D, 0x6A, 0x7F, 0x2D, 0x3D, 0x6D, 0x08, 0x97, 0x6C, 0x64, 0x91, 0x01, 0x5C, 0x63, 0xE6,
            0xF4, 0x51, 0x6B, 0x6B, 0x62, 0x61, 0x6C, 0x1C, 0xD8, 0x30, 0x65, 0x85, 0x4E, 0x00, 0x62, 0xF2, 0xED, 0x95, 0x06, 0x6C, 0x7B, 0xA5, 0x01, 0x1B, 0xC1, 0xF4, 0x08, 0x82, 0x57, 0xC4, 0x0F, 0xF5,
            0xC6, 0xD9, 0xB0, 0x65, 0x50, 0xE9, 0xB7, 0x12, 0xEA, 0xB8, 0xBE, 0x8B, 0x7C, 0x88, 0xB9, 0xFC, 0xDF, 0x1D, 0xDD, 0x62, 0x49, 0x2D, 0xDA, 0x15, 0xF3, 0x7C, 0xD3, 0x8C, 0x65, 0x4C, 0xD4, 0xFB,
            0x58, 0x61, 0xB2, 0x4D, 0xCE, 0x51, 0xB5, 0x3A, 0x74, 0x00, 0xBC, 0xA3, 0xE2, 0x30, 0xBB, 0xD4, 0x41, 0xA5, 0xDF, 0x4A, 0xD7, 0x95, 0xD8, 0x3D, 0x6D, 0xC4, 0xD1, 0xA4, 0xFB, 0xF4, 0xD6, 0xD3,
            0x6A, 0xE9, 0x69, 0x43, 0xFC, 0xD9, 0x6E, 0x34, 0x46, 0x88, 0x67, 0xAD, 0xD0, 0xB8, 0x60, 0xDA, 0x73, 0x2D, 0x04, 0x44, 0xE5, 0x1D, 0x03, 0x33, 0x5F, 0x4C, 0x0A, 0xAA, 0xC9, 0x7C, 0x0D, 0xDD,
            0x3C, 0x71, 0x05, 0x50, 0xAA, 0x41, 0x02, 0x27, 0x10, 0x10, 0x0B, 0xBE, 0x86, 0x20, 0x0C, 0xC9, 0x25, 0xB5, 0x68, 0x57, 0xB3, 0x85, 0x6F, 0x20, 0x09, 0xD4, 0x66, 0xB9, 0x9F, 0xE4, 0x61, 0xCE,
            0x0E, 0xF9, 0xDE, 0x5E, 0x98, 0xC9, 0xD9, 0x29, 0x22, 0x98, 0xD0, 0xB0, 0xB4, 0xA8, 0xD7, 0xC7, 0x17, 0x3D, 0xB3, 0x59, 0x81, 0x0D, 0xB4, 0x2E, 0x3B, 0x5C, 0xBD, 0xB7, 0xAD, 0x6C, 0xBA, 0xC0,
            0x20, 0x83, 0xB8, 0xED, 0xB6, 0xB3, 0xBF, 0x9A, 0x0C, 0xE2, 0xB6, 0x03, 0x9A, 0xD2, 0xB1, 0x74, 0x39, 0x47, 0xD5, 0xEA, 0xAF, 0x77, 0xD2, 0x9D, 0x15, 0x26, 0xDB, 0x04, 0x83, 0x16, 0xDC, 0x73,
            0x12, 0x0B, 0x63, 0xE3, 0x84, 0x3B, 0x64, 0x94, 0x3E, 0x6A, 0x6D, 0x0D, 0xA8, 0x5A, 0x6A, 0x7A, 0x0B, 0xCF, 0x0E, 0xE4, 0x9D, 0xFF, 0x09, 0x93, 0x27, 0xAE, 0x00, 0x0A, 0xB1, 0x9E, 0x07, 0x7D,
            0x44, 0x93, 0x0F, 0xF0, 0xD2, 0xA3, 0x08, 0x87, 0x68, 0xF2, 0x01, 0x1E, 0xFE, 0xC2, 0x06, 0x69, 0x5D, 0x57, 0x62, 0xF7, 0xCB, 0x67, 0x65, 0x80, 0x71, 0x36, 0x6C, 0x19, 0xE7, 0x06, 0x6B, 0x6E,
            0x76, 0x1B, 0xD4, 0xFE, 0xE0, 0x2B, 0xD3, 0x89, 0x5A, 0x7A, 0xDA, 0x10, 0xCC, 0x4A, 0xDD, 0x67, 0x6F, 0xDF, 0xB9, 0xF9, 0xF9, 0xEF, 0xBE, 0x8E, 0x43, 0xBE, 0xB7, 0x17, 0xD5, 0x8E, 0xB0, 0x60,
            0xE8, 0xA3, 0xD6, 0xD6, 0x7E, 0x93, 0xD1, 0xA1, 0xC4, 0xC2, 0xD8, 0x38, 0x52, 0xF2, 0xDF, 0x4F, 0xF1, 0x67, 0xBB, 0xD1, 0x67, 0x57, 0xBC, 0xA6, 0xDD, 0x06, 0xB5, 0x3F, 0x4B, 0x36, 0xB2, 0x48,
            0xDA, 0x2B, 0x0D, 0xD8, 0x4C, 0x1B, 0x0A, 0xAF, 0xF6, 0x4A, 0x03, 0x36, 0x60, 0x7A, 0x04, 0x41, 0xC3, 0xEF, 0x60, 0xDF, 0x55, 0xDF, 0x67, 0xA8, 0xEF, 0x8E, 0x6E, 0x31, 0x79, 0xBE, 0x69, 0x46,
            0x8C, 0xB3, 0x61, 0xCB, 0x1A, 0x83, 0x66, 0xBC, 0xA0, 0xD2, 0x6F, 0x25, 0x36, 0xE2, 0x68, 0x52, 0x95, 0x77, 0x0C, 0xCC, 0x03, 0x47, 0x0B, 0xBB, 0xB9, 0x16, 0x02, 0x22, 0x2F, 0x26, 0x05, 0x55,
            0xBE, 0x3B, 0xBA, 0xC5, 0x28, 0x0B, 0xBD, 0xB2, 0x92, 0x5A, 0xB4, 0x2B, 0x04, 0x6A, 0xB3, 0x5C, 0xA7, 0xFF, 0xD7, 0xC2, 0x31, 0xCF, 0xD0, 0xB5, 0x8B, 0x9E, 0xD9, 0x2C, 0x1D, 0xAE, 0xDE, 0x5B,
            0xB0, 0xC2, 0x64, 0x9B, 0x26, 0xF2, 0x63, 0xEC, 0x9C, 0xA3, 0x6A, 0x75, 0x0A, 0x93, 0x6D, 0x02, 0xA9, 0x06, 0x09, 0x9C, 0x3F, 0x36, 0x0E, 0xEB, 0x85, 0x67, 0x07, 0x72, 0x13, 0x57, 0x00, 0x05,
            0x82, 0x4A, 0xBF, 0x95, 0x14, 0x7A, 0xB8, 0xE2, 0xAE, 0x2B, 0xB1, 0x7B, 0x38, 0x1B, 0xB6, 0x0C, 0x9B, 0x8E, 0xD2, 0x92, 0x0D, 0xBE, 0xD5, 0xE5, 0xB7, 0xEF, 0xDC, 0x7C, 0x21, 0xDF, 0xDB, 0x0B,
            0xD4, 0xD2, 0xD3, 0x86, 0x42, 0xE2, 0xD4, 0xF1, 0xF8, 0xB3, 0xDD, 0x68, 0x6E, 0x83, 0xDA, 0x1F, 0xCD, 0x16, 0xBE, 0x81, 0x5B, 0x26, 0xB9, 0xF6, 0xE1, 0x77, 0xB0, 0x6F, 0x77, 0x47, 0xB7, 0x18,
            0xE6, 0x5A, 0x08, 0x88, 0x70, 0x6A, 0x0F, 0xFF, 0xCA, 0x3B, 0x06, 0x66, 0x5C, 0x0B, 0x01, 0x11, 0xFF, 0x9E, 0x65, 0x8F, 0x69, 0xAE, 0x62, 0xF8, 0xD3, 0xFF, 0x6B, 0x61, 0x45, 0xCF, 0x6C, 0x16,
            0x78, 0xE2, 0x0A, 0xA0, 0xEE, 0xD2, 0x0D, 0xD7, 0x54, 0x83, 0x04, 0x4E, 0xC2, 0xB3, 0x03, 0x39, 0x61, 0x26, 0x67, 0xA7, 0xF7, 0x16, 0x60, 0xD0, 0x4D, 0x47, 0x69, 0x49, 0xDB, 0x77, 0x6E, 0x3E,
            0x4A, 0x6A, 0xD1, 0xAE, 0xDC, 0x5A, 0xD6, 0xD9, 0x66, 0x0B, 0xDF, 0x40, 0xF0, 0x3B, 0xD8, 0x37, 0x53, 0xAE, 0xBC, 0xA9, 0xC5, 0x9E, 0xBB, 0xDE, 0x7F, 0xCF, 0xB2, 0x47, 0xE9, 0xFF, 0xB5, 0x30,
            0x1C, 0xF2, 0xBD, 0xBD, 0x8A, 0xC2, 0xBA, 0xCA, 0x30, 0x93, 0xB3, 0x53, 0xA6, 0xA3, 0xB4, 0x24, 0x05, 0x36, 0xD0, 0xBA, 0x93, 0x06, 0xD7, 0xCD, 0x29, 0x57, 0xDE, 0x54, 0xBF, 0x67, 0xD9, 0x23,
            0x2E, 0x7A, 0x66, 0xB3, 0xB8, 0x4A, 0x61, 0xC4, 0x02, 0x1B, 0x68, 0x5D, 0x94, 0x2B, 0x6F, 0x2A, 0x37, 0xBE, 0x0B, 0xB4, 0xA1, 0x8E, 0x0C, 0xC3, 0x1B, 0xDF, 0x05, 0x5A, 0x8D, 0xEF, 0x02, 0x2D
        };
    }
}