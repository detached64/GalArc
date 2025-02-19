using ArcFormats.Properties;
using GalArc.Controls;
using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using Utility;
using Utility.Compression;
using Utility.Extensions;

namespace ArcFormats.Yuris
{
    public class YPF : ArchiveFormat
    {
        private static readonly Lazy<OptionsTemplate> _lazyUnpackOptions = new Lazy<OptionsTemplate>(() => new UnpackYPFOptions());
        public static OptionsTemplate UnpackExtraOptions => _lazyUnpackOptions.Value;

        private class YpfEntry : PackedEntry
        {
            public int NameLen { get; set; }
            public byte[] Data { get; set; }
        }

        private class Scheme
        {
            public byte Key { get; set; }
            public byte[] Table { get; set; }
            public int ExtraLen { get; set; }
            public byte[] ScriptKeyBytes { get; set; }
            public uint ScriptKey { get; set; }

            public Scheme()
            {
                Key = 0;
                ScriptKeyBytes = new byte[] { };
                Table = Tables[0];
                ExtraLen = ExtraLens[0];
            }
        }

        private List<YpfEntry> entries = new List<YpfEntry>();
        private Scheme scheme = new Scheme();

        private bool isFirstGuessYpf = true;
        private bool isFirstGuessYst = true;
        private string folderPath;

        // most freqently used combination (as far as I guess) : table3 , extraLen = 8
        private static readonly byte[] Table1 = { 0x09, 0x0B, 0x0D, 0x13, 0x15, 0x1B, 0x20, 0x23, 0x26, 0x29, 0x2C, 0x2F, 0x2E, 0x32 };
        private static readonly byte[] Table2 = { 0x0C, 0x10, 0x11, 0x19, 0x1C, 0x1E, 0x09, 0x0B, 0x0D, 0x13, 0x15, 0x1B, 0x20, 0x23, 0x26, 0x29, 0x2C, 0x2F, 0x2E, 0x32 };
        private static readonly byte[] Table3 = { 0x03, 0x48, 0x06, 0x35, 0x0C, 0x10, 0x11, 0x19, 0x1C, 0x1E, 0x09, 0x0B, 0x0D, 0x13, 0x15, 0x1B, 0x20, 0x23, 0x26, 0x29, 0x2C, 0x2F, 0x2E, 0x32 };
        private static readonly byte[] Table0x1F4 = { 0x3, 0xA, 0x6, 0x35, 0xC, 0x10, 0x11, 0x18, 0x1C, 0x1E, 0x9, 0xB, 0xD, 0x13, 0x15, 0x1B, 0x20, 0x23, 0x26, 0x29, 0x2C, 0x2F, 0x14, 0x2E };
        private static readonly List<byte[]> Tables = new List<byte[]> { Table3, Table2, Table1 };
        private static readonly string[] TableNames = { "Table3", "Table2", "Table1" };
        private static readonly List<int> ExtraLens = new List<int> { 8, 4, 0 };

        public override void Unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            if (br.ReadUInt32() != 0x00465059)
            {
                Logger.ErrorInvalidArchive();
            }

            this.folderPath = folderPath;

            uint version = br.ReadUInt32();
            int fileCount = br.ReadInt32();
            uint indexSize = br.ReadUInt32();
            fs.Position += 16;
            Logger.InitBar(fileCount);

            TryReadIndex(br, version, fileCount);

            Directory.CreateDirectory(folderPath);

            foreach (YpfEntry entry in entries)
            {
                fs.Position = entry.Offset;
                entry.Data = br.ReadBytes((int)entry.Size);
                if (entry.IsPacked)
                {
                    entry.Data = ZlibHelper.Decompress(entry.Data);
                }
                Directory.CreateDirectory(Path.GetDirectoryName(entry.Path));
                if (UnpackYPFOptions.toDecryptScripts && Path.GetExtension(entry.Path) == ".ybn" && BitConverter.ToUInt32(entry.Data, 0) == 0x42545359)
                {
                    Logger.Debug(string.Format(Resources.logTryDecScr, entry.Name));
                    entry.Data = TryDecryptScript(entry.Data);
                }
                File.WriteAllBytes(entry.Path, entry.Data);
                entry.Data = null;
                Logger.UpdateBar();
            }

            fs.Dispose();
            br.Dispose();
        }

        private void TryReadIndex(BinaryReader br, uint version, int fileCount)
        {
            foreach (var table in Tables)
            {
                foreach (var length in ExtraLens)
                {
                    scheme.Table = table;
                    scheme.ExtraLen = length;
                    Logger.Debug($"Try {TableNames[Tables.IndexOf(table)]} , Extra Length = {length}……");
                    try
                    {
                        Reset();
                        ReadIndex(br, fileCount);
                        return;
                    }
                    catch
                    { }
                }
            }
            if (version == 0x1F4)
            {
                scheme.Table = Table0x1F4;
                foreach (var length in ExtraLens)
                {
                    scheme.ExtraLen = length;
                    Logger.Debug($"Try special table , Extra Length = {length}……");
                    try
                    {
                        Reset();
                        ReadIndex(br, fileCount);
                        return;
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            Logger.Error("Failed to read index.");
        }

        private void ReadIndex(BinaryReader br, int fileCount)
        {
            br.BaseStream.Position = 32;

            for (int i = 0; i < fileCount; i++)
            {
                br.BaseStream.Position += 4;
                YpfEntry entry = new YpfEntry();
                entry.NameLen = DecryptNameLength((byte)(br.ReadByte() ^ 0xff));
                byte[] name = br.ReadBytes(entry.NameLen);
                DecryptName(name);
                entry.Name = ArcEncoding.Shift_JIS.GetString(name);
                if (entry.Name.ContainsInvalidChars())
                {
                    throw new Exception();
                }
                entry.Path = Path.Combine(folderPath, entry.Name);
                br.BaseStream.Position++;
                entry.IsPacked = br.ReadByte() != 0;
                entry.UnpackedSize = br.ReadUInt32();
                entry.Size = br.ReadUInt32();
                entry.Offset = br.ReadUInt32();
                br.BaseStream.Position += scheme.ExtraLen;
                entries.Add(entry);
            }
        }

        private int DecryptNameLength(byte len)
        {
            int pos = Array.IndexOf(scheme.Table, len);
            if (pos == -1)
            {
                return len;
            }
            else if ((pos & 1) != 0)
            {
                return scheme.Table[pos - 1];
            }
            else
            {
                return scheme.Table[pos + 1];
            }
        }

        private void DecryptName(byte[] name)
        {
            if (isFirstGuessYpf)
            {
                scheme.Key = (byte)(name[name.Length - 4] ^ '.');       // (maybe) all files inside the ypf archive has a 3-letter extension,so……
                isFirstGuessYpf = false;
            }
            for (int i = 0; i < name.Length; i++)
            {
                name[i] ^= scheme.Key;
            }
        }

        private byte[] TryDecryptScript(byte[] script)
        {
            byte[] result;
            try
            {
                FindXorKey(script, false);
                result = DecryptNewScript(script, BitConverter.ToUInt32(script, 12), BitConverter.ToUInt32(script, 16), BitConverter.ToUInt32(script, 20), BitConverter.ToUInt32(script, 24));
            }
            catch
            {
                try
                {
                    FindXorKey(script, true);
                    result = DecryptOldScript(script, BitConverter.ToUInt32(script, 8), BitConverter.ToUInt32(script, 12));
                }
                catch
                {
                    isFirstGuessYst = true;
                    Logger.Error(Resources.logErrorDecScrFailed, false);
                    return script;
                }
            }
            isFirstGuessYst = false;
            return result;
        }

        private void FindXorKey(byte[] script, bool isOld)
        {
            if (isOld)
            {
                if (script.Length < 44 || !isFirstGuessYst)
                {
                    return;
                }
                Buffer.BlockCopy(script, 44, scheme.ScriptKeyBytes, 0, 4);
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
                scheme.ScriptKey = br.ReadUInt32();
                scheme.ScriptKeyBytes = BitConverter.GetBytes(scheme.ScriptKey);
            }
        }

        private byte[] DecryptNewScript(byte[] script, uint len1, uint len2, uint len3, uint len4)
        {
            byte[] result = new byte[script.Length];
            Buffer.BlockCopy(script, 0, result, 0, script.Length);
            uint pos = 32;
            for (uint i = 0; i < len1; i++)
            {
                result[i + pos] ^= scheme.ScriptKeyBytes[i & 3];
            }
            pos += len1;
            for (uint i = 0; i < len2; i++)
            {
                result[i + pos] ^= scheme.ScriptKeyBytes[i & 3];
            }
            pos += len2;
            for (uint i = 0; i < len3; i++)
            {
                result[i + pos] ^= scheme.ScriptKeyBytes[i & 3];
            }
            pos += len3;
            for (uint i = 0; i < len4; i++)
            {
                result[i + pos] ^= scheme.ScriptKeyBytes[i & 3];
            }
            return result;
        }

        private byte[] DecryptOldScript(byte[] script, uint len1, uint len2)
        {
            byte[] result = new byte[script.Length];
            Buffer.BlockCopy(script, 0, result, 0, script.Length);
            uint pos = 32;
            for (uint i = 0; i < len1; i++)
            {
                result[i + pos] ^= scheme.ScriptKeyBytes[i & 3];
            }
            pos += len1;
            for (uint i = 0; i < len2; i++)
            {
                result[i + pos] ^= scheme.ScriptKeyBytes[i & 3];
            }
            return result;
        }

        private void Reset()
        {
            entries.Clear();
            isFirstGuessYpf = true;
            isFirstGuessYst = true;
            scheme.Key = 0;
        }
    }
}
