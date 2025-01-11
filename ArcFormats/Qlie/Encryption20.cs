using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utility;

namespace ArcFormats.Qlie
{
    internal class Encryption20 : QlieEncryption
    {
        public Encryption20(QlieHeader qheader)
        {
            header = qheader;
            entries = new List<QlieEntry>(header.FileCount);
        }
        private List<QlieEntry> entries;
        private QlieHeader header;

        public override void Unpack(string input, string output)
        {
            FileStream fs = File.OpenRead(input);
            BinaryReader br = new BinaryReader(fs);
            fs.Position = fs.Length - 0x440;
            QlieKey qkey = new QlieKey()
            {
                Magic = br.ReadBytes(32),
                HashSize = br.ReadUInt32(),
                Key = br.ReadBytes(0x400)
            };
            if (qkey.HashSize > fs.Length || qkey.HashSize < 0x44)
            {
                Logger.Error("Invalid key");
            }
            Decrypt(qkey.Magic, 32, 0);
            if (!string.Equals(Encoding.ASCII.GetString(qkey.Magic), KeyMagic))
            {
                Logger.Error("Invalid key magic");
            }

            fs.Position = header.IndexOffset;
            for (int i = 0; i < header.FileCount; i++)
            {
                QlieEntry entry = new QlieEntry();
                int length = br.ReadInt16();
                entry.Name = DecryptName(br.ReadBytes(length));
                entry.Path = Path.Combine(output, entry.Name);
                entry.Offset = br.ReadInt64();
                entry.Size = br.ReadUInt32();
                entry.UnpackedSize = br.ReadUInt32();
                entry.IsPacked = br.ReadInt32() != 0 && entry.Size != entry.UnpackedSize;
                entry.IsEncrypted = br.ReadUInt32();
                entry.Hash = br.ReadUInt32();
                entries.Add(entry);
            }
            Logger.InitBar(header.FileCount);
            foreach (QlieEntry entry in entries)
            {
                fs.Position = entry.Offset;
                byte[] data = br.ReadBytes((int)entry.Size);
                if (entry.IsEncrypted != 0)
                {
                    Decrypt(data, data.Length, 0);
                }
                if (entry.IsPacked)
                {
                    data = Decompress(data);
                }
                Directory.CreateDirectory(Path.GetDirectoryName(entry.Path));
                File.WriteAllBytes(entry.Path, data);
                Logger.UpdateBar();
            }
        }

        private string DecryptName(byte[] input)
        {
            uint name_length = (uint)input.Length;
            uint key = 0xFA + name_length;        // 0xC4 ^ 0x3E
            for (int i = 1; i <= name_length; i++)
            {
                input[i - 1] ^= (byte)((key ^ i) + i);
            }
            return ArcEncoding.Shift_JIS.GetString(input);
        }
    }
}
