using Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utility;
using Utility.Compression;

namespace ArcFormats.Yuris
{
    internal class YPF
    {
        internal class Entry
        {
            public int nameLen;
            public string fileName;
            public string filePath;
            public byte[] fileData;
            public uint unpackedSize;
            public uint packedSize;
            public uint offset;
            public bool isPacked;
        }

        internal class Scheme
        {
            public byte key;
            public byte[] table;
            public uint scriptKey;
            public int extraLen;
            public byte[] scriptKeyBytes;
        }

        internal static List<Entry> entries = new List<Entry>();
        internal static Scheme scheme = new Scheme();
        internal static int TablesIndex;
        internal static int ExtraLensIndex;  // most freqently used combination(as far as I guess):table3,extraLen=8

        internal static bool isFirstGuessYpf = true;
        internal static bool isFirstGuessYst = true;
        internal static string FolderPath { get; set; }


        static readonly byte[] Table1 = { 0x09, 0x0B, 0x0D, 0x13, 0x15, 0x1B, 0x20, 0x23, 0x26, 0x29, 0x2C, 0x2F, 0x2E, 0x32 };
        static readonly byte[] Table2 = { 0x0C, 0x10, 0x11, 0x19, 0x1C, 0x1E, 0x09, 0x0B, 0x0D, 0x13, 0x15, 0x1B, 0x20, 0x23, 0x26, 0x29, 0x2C, 0x2F, 0x2E, 0x32 };
        static readonly byte[] Table3 = { 0x03, 0x48, 0x06, 0x35, 0x0C, 0x10, 0x11, 0x19, 0x1C, 0x1E, 0x09, 0x0B, 0x0D, 0x13, 0x15, 0x1B, 0x20, 0x23, 0x26, 0x29, 0x2C, 0x2F, 0x2E, 0x32 };
        static readonly List<byte[]> tables = new List<byte[]> { Table1, Table2, Table3 };
        static readonly List<int> extraLens = new List<int> { 4, 8 };

        public static void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            if (!br.ReadBytes(4).SequenceEqual(new byte[] { 0x59, 0x50, 0x46, 0x00 }))
            {
                LogUtility.Error_NotValidArchive();
            }

            FolderPath = folderPath;
            scheme.key = 0;
            scheme.scriptKeyBytes = new byte[] { };
            TablesIndex = tables.Count - 1;
            ExtraLensIndex = extraLens.Count - 1;
            scheme.table = tables[TablesIndex];
            scheme.extraLen = extraLens[ExtraLensIndex];
            entries.Clear();
            isFirstGuessYpf = true;
            isFirstGuessYst = true;

            uint version = br.ReadUInt32();
            int fileCount = br.ReadInt32();
            uint indexSize = br.ReadUInt32();
            fs.Position += 16;
            LogUtility.InitBar(fileCount);

            TryReadIndex(br, fileCount);

            Directory.CreateDirectory(folderPath);

            foreach (Entry entry in entries)
            {
                fs.Position = entry.offset;
                entry.fileData = br.ReadBytes((int)entry.packedSize);
                if (entry.isPacked)
                {
                    entry.fileData = Zlib.DecompressBytes(entry.fileData);
                }
                if (!Directory.Exists(Path.GetDirectoryName(entry.filePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(entry.filePath));
                }
                if (Global.ToDecryptScript && Path.GetExtension(entry.filePath) == ".ybn" && BitConverter.ToUInt32(entry.fileData, 0) == 0x42545359)
                {
                    LogUtility.Debug("Try to decrypt script:" + entry.fileName);
                    entry.fileData = TryDecryptScript(entry.fileData);
                }
                File.WriteAllBytes(entry.filePath, entry.fileData);
                LogUtility.UpdateBar();
            }

            fs.Dispose();
            br.Dispose();
        }

        private static void TryReadIndex(BinaryReader br, int fileCount)
        {

            while (TablesIndex >= 0 && ExtraLensIndex >= 0)
            {
                scheme.table = tables[TablesIndex];
                scheme.extraLen = extraLens[ExtraLensIndex];
                LogUtility.Debug($"Try Table {TablesIndex + 1} , Extra Length = {scheme.extraLen}……");

                try
                {
                    ReadIndex(br, fileCount);
                    return; // Exit the loop if successful
                }
                catch (Exception)
                {
                    if (ExtraLensIndex > 0)
                    {
                        ExtraLensIndex--;
                    }
                    else
                    {
                        TablesIndex--;
                        ExtraLensIndex = extraLens.Count - 1;
                    }
                }
            }
        }

        private static void ReadIndex(BinaryReader br, int fileCount)
        {
            br.BaseStream.Position = 32;
            entries.Clear();

            for (int i = 0; i < fileCount; i++)
            {
                br.BaseStream.Position += 4;
                Entry entry = new Entry();
                entry.nameLen = DecryptNameLength((byte)(br.ReadByte() ^ 0xff));
                entry.fileName = ArcEncoding.Shift_JIS.GetString(DecryptName(br.ReadBytes(entry.nameLen)));
                entry.filePath = Path.Combine(FolderPath, entry.fileName);
                br.BaseStream.Position++;
                entry.isPacked = br.ReadByte() != 0;
                entry.unpackedSize = br.ReadUInt32();
                entry.packedSize = br.ReadUInt32();
                entry.offset = br.ReadUInt32();
                br.BaseStream.Position += scheme.extraLen;
                entries.Add(entry);
            }
        }

        private static int DecryptNameLength(byte len)
        {
            int pos = Array.IndexOf(scheme.table, len);
            if (pos == -1)
            {
                return len;
            }
            else if ((pos & 1) != 0)
            {
                return scheme.table[pos - 1];
            }
            else
            {
                return scheme.table[pos + 1];
            }
        }

        private static byte[] DecryptName(byte[] name)
        {
            if (isFirstGuessYpf)
            {
                scheme.key = (byte)(name[name.Length - 4] ^ '.');       // (maybe) all files inside the ypf archive has a 3-letter extension,so……
                isFirstGuessYpf = false;
            }
            return Xor.xor(name, scheme.key);
        }

        private static byte[] TryDecryptScript(byte[] script)
        {
            byte[] result = new byte[] { };
            try
            {
                FindXorKey(script, "new");
                result = DecryptNewScript(script, BitConverter.ToUInt32(script, 12), BitConverter.ToUInt32(script, 16), BitConverter.ToUInt32(script, 20), BitConverter.ToUInt32(script, 24));
            }
            catch
            {
                try
                {
                    FindXorKey(script, "old");
                    result = DecryptOldScript(script, BitConverter.ToUInt32(script, 8), BitConverter.ToUInt32(script, 12));
                }
                catch
                {
                    isFirstGuessYst = true;
                    LogUtility.Error("Decryption failed.", false);
                    return script;
                }
            }
            isFirstGuessYst = false;
            return result;
        }
        private static void FindXorKey(byte[] script, string flag)
        {
            if (flag == "old")
            {
                if (script.Length < 44 || !isFirstGuessYst)
                {
                    return;
                }
                Array.Copy(script, 44, scheme.scriptKeyBytes, 0, 4);
            }
            else
            {
                if (!isFirstGuessYst)
                {
                    return;
                }
                MemoryStream ms = new MemoryStream(script);
                BinaryReader br = new BinaryReader(ms);
                ms.Position = 12;
                uint a = br.ReadUInt32();

                ms.Position = a + 40;
                scheme.scriptKey = br.ReadUInt32();
                scheme.scriptKeyBytes = BitConverter.GetBytes(scheme.scriptKey);
            }
        }

        private static byte[] DecryptNewScript(byte[] script, uint len1, uint len2, uint len3, uint len4)
        {
            byte[] result = new byte[script.Length];
            Array.Copy(script, result, script.Length);
            uint pos = 32;
            for (uint i = 0; i < len1; i++)
            {
                result[i + pos] ^= scheme.scriptKeyBytes[i & 3];
            }
            pos += len1;
            for (uint i = 0; i < len2; i++)
            {
                result[i + pos] ^= scheme.scriptKeyBytes[i & 3];
            }
            pos += len2;
            for (uint i = 0; i < len3; i++)
            {
                result[i + pos] ^= scheme.scriptKeyBytes[i & 3];
            }
            pos += len3;
            for (uint i = 0; i < len4; i++)
            {
                result[i + pos] ^= scheme.scriptKeyBytes[i & 3];
            }
            return result;
        }

        private static byte[] DecryptOldScript(byte[] script, uint len1, uint len2)
        {
            byte[] result = new byte[script.Length];
            Array.Copy(script, result, script.Length);
            uint pos = 32;
            for (uint i = 0; i < len1; i++)
            {
                result[i + pos] ^= scheme.scriptKeyBytes[i & 3];
            }
            pos += len1;
            for (uint i = 0; i < len2; i++)
            {
                result[i + pos] ^= scheme.scriptKeyBytes[i & 3];
            }
            return result;
        }
    }
}
