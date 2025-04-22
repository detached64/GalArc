using GalArc.Controls;
using GalArc.Database;
using GalArc.Logs;
using GalArc.Strings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility.Exceptions;

namespace ArcFormats.Siglus
{
    public class ScenePCK : ArcFormat
    {
        public override OptionsTemplate UnpackExtraOptions => UnpackPCKOptions.Instance;

        protected SiglusOptions Options => UnpackPCKOptions.Instance.Options;

        protected SiglusScheme Scheme
        {
            get => UnpackPCKOptions.Instance.Scheme;
            set => UnpackPCKOptions.Instance.Scheme = value;
        }

        private class ScenePckHeader
        {
            public int FileCount { get; set; }
            public uint NameTableOffset { get; set; }
            public uint NameOffset { get; set; }
            public uint DataTableOffset { get; set; }
            public uint DataOffset { get; set; }
            public bool UseExtraKey { get; set; }
        }

        protected class ScenePckEntry
        {
            public uint NameOffset { get; set; }
            public int NameLength { get; set; }
            public string Name { get; set; }
            public uint DataOffset { get; set; }
            public uint DataLength { get; set; }
            public byte[] Data { get; set; }
            public uint PackedLength { get; set; }
            public uint UnpackedLength { get; set; }
        }

        public override void Unpack(string filePath, string folderPath)
        {
            string name = Path.GetFileName(filePath);
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            fs.Position = 52;
            ScenePckHeader header = new ScenePckHeader();
            header.NameTableOffset = br.ReadUInt32();
            header.FileCount = br.ReadInt32();
            header.NameOffset = br.ReadUInt32();
            fs.Position += 4;
            header.DataTableOffset = br.ReadUInt32();
            fs.Position += 4;
            header.DataOffset = br.ReadUInt32();
            fs.Position += 4;
            header.UseExtraKey = br.ReadByte() != 0;

            Logger.InitBar(header.FileCount);
            List<ScenePckEntry> entries = new List<ScenePckEntry>(header.FileCount);
            fs.Position = header.NameTableOffset;
            for (int i = 0; i < header.FileCount; i++)
            {
                ScenePckEntry entry = new ScenePckEntry();
                entry.NameOffset = br.ReadUInt32();
                entry.NameLength = br.ReadInt32();
                entries.Add(entry);
            }
            fs.Position = header.NameOffset;
            foreach (ScenePckEntry entry in entries)
            {
                entry.Name = Encoding.Unicode.GetString(br.ReadBytes(entry.NameLength * 2)) + ".ss";
            }
            fs.Position = header.DataTableOffset;
            foreach (ScenePckEntry entry in entries)
            {
                entry.DataOffset = br.ReadUInt32();
                entry.DataLength = br.ReadUInt32();
            }
            fs.Position = header.DataOffset;
            foreach (ScenePckEntry entry in entries)
            {
                fs.Position = header.DataOffset + entry.DataOffset;
                entry.Data = br.ReadBytes((int)entry.DataLength);
            }
            Directory.CreateDirectory(folderPath);

            byte[] key = header.UseExtraKey ? (Options.TryEachKey ? TryAllSchemes(entries[0], 0) : Options.Key) : null;
            foreach (ScenePckEntry entry in entries)
            {
                SiglusUtils.DecryptWithKey(entry.Data, key);
                SiglusUtils.Decrypt(entry.Data, 0);

                entry.PackedLength = BitConverter.ToUInt32(entry.Data, 0);
                if (entry.PackedLength != entry.Data.Length)
                {
                    throw new InvalidSchemeException();
                }
                entry.UnpackedLength = BitConverter.ToUInt32(entry.Data, 4);
                byte[] input = new byte[entry.PackedLength - 8];
                Buffer.BlockCopy(entry.Data, 8, input, 0, input.Length);
                try
                {
                    entry.Data = SiglusUtils.Decompress(input, entry.UnpackedLength);
                }
                catch
                {
                    throw new InvalidSchemeException();
                }
            }

            foreach (ScenePckEntry entry in entries)
            {
                string entryPath = Path.Combine(folderPath, entry.Name);
                File.WriteAllBytes(entryPath, entry.Data);
                Logger.UpdateBar();
                entry.Data = null;
            }
            entries.Clear();
            fs.Dispose();
            br.Dispose();
        }

        protected bool IsRightKey(byte[] data, byte[] key, int type)
        {
            byte[] backup = new byte[data.Length];
            Buffer.BlockCopy(data, 0, backup, 0, data.Length);

            SiglusUtils.DecryptWithKey(backup, key);
            SiglusUtils.Decrypt(backup, type);
            if (BitConverter.ToUInt32(backup, 0) != backup.Length)
            {
                backup = null;
                return false;
            }
            try
            {
                byte[] bytes = new byte[backup.Length - 8];
                Buffer.BlockCopy(backup, 8, bytes, 0, bytes.Length);
                SiglusUtils.Decompress(bytes, BitConverter.ToUInt32(backup, 4));
            }
            catch
            {
                return false;
            }
            return true;
        }

        protected byte[] TryAllSchemes(ScenePckEntry entry, int type)
        {
            foreach (var scheme in Scheme.KnownKeys)
            {
                byte[] key = scheme.Value;
                if (key.Length != 16)
                {
                    continue;
                }
                if (IsRightKey(entry.Data, key, type))
                {
                    Logger.Info(string.Format(LogStrings.KeyFound, BitConverter.ToString(key)));
                    Logger.Info(string.Format(LogStrings.MatchedGame, scheme.Key));
                    return key;
                }
            }
            Logger.Info(LogStrings.KeyNotFound);
            return null;
        }

        public override void DeserializeScheme(out string name, out int count)
        {
            Scheme = Deserializer.LoadScheme<SiglusScheme>();
            name = "Siglus";
            count = Scheme?.KnownKeys?.Count ?? 0;
        }
    }
}
