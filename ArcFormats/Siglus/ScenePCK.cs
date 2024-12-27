using GalArc.Database.Siglus;
using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ArcFormats.Siglus
{
    public class ScenePCK : ArchiveFormat
    {
        public static UserControl UnpackExtraOptions = new UnpackPCKOptions();

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

        internal static SiglusScheme ImportedSchemes;

        internal static Tuple<string, byte[]> SelectedScheme;

        internal static bool TryEachKey;

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

            byte[] key = header.UseExtraKey ? (TryEachKey ? TryAllSchemes(entries[0], 0) : (SelectedScheme?.Item2)) : null;
            foreach (ScenePckEntry entry in entries)
            {
                SiglusUtils.DecryptWithKey(entry.Data, key);
                SiglusUtils.Decrypt(entry.Data, 0);

                entry.PackedLength = BitConverter.ToUInt32(entry.Data, 0);
                if (entry.PackedLength != entry.Data.Length)
                {
                    Logger.Error(Siglus.logWrongKey);
                }
                entry.UnpackedLength = BitConverter.ToUInt32(entry.Data, 4);
                byte[] input = new byte[entry.PackedLength - 8];
                Array.Copy(entry.Data, 8, input, 0, input.Length);
                try
                {
                    entry.Data = SiglusUtils.Decompress(input, entry.UnpackedLength);
                }
                catch
                {
                    Logger.Error(Siglus.logWrongKey);
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
            Array.Copy(data, backup, data.Length);

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
                Array.Copy(backup, 8, bytes, 0, bytes.Length);
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
            foreach (var scheme in ImportedSchemes.KnownSchemes.Values)
            {
                if (scheme.KnownKey.Length != 16)
                {
                    continue;
                }
                if (IsRightKey(entry.Data, scheme.KnownKey, type))
                {
                    Logger.Info(string.Format(Siglus.logFound, scheme.KnownKey));
                    Logger.Info(string.Format(Siglus.logMatchedGame, FindKeyFromValue(scheme.KnownKey)));
                    return scheme.KnownKey;
                }
            }
            Logger.Info(string.Format(Siglus.logNotFound));
            return null;
        }

        protected string FindKeyFromValue(byte[] key)
        {
            foreach (var dic in ImportedSchemes.KnownSchemes)
            {
                if (key.SequenceEqual(dic.Value.KnownKey))
                {
                    return dic.Key;
                }
            }
            return null;
        }
    }
}
