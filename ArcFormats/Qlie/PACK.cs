using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ArcFormats.Qlie
{
    public class PACK : ArchiveFormat
    {
        public override void Unpack(string input, string output)
        {
            QlieHeader header = new QlieHeader();
            using (FileStream fs = File.OpenRead(input))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    fs.Position = fs.Length - 0x1C;
                    header.Magic = Encoding.ASCII.GetString(br.ReadBytes(16)).TrimEnd('\0');
                    header.FileCount = br.ReadInt32();
                    header.IndexOffset = br.ReadInt64();
                    if (!IsSaneCount(header.FileCount) || header.IndexOffset < 0 || header.IndexOffset >= fs.Length)
                    {
                        Logger.Error("Invalid header");
                    }
                }
            }
            List<QlieEntry> entries = new List<QlieEntry>(header.FileCount);
            switch (header.Magic)
            {
                case HeaderMagic31:
                    Encryption31 encryption31 = new Encryption31(header);
                    encryption31.Unpack(input, output);
                    break;
                case HeaderMagic30:
                    break;
                case HeaderMagic20:
                    Encryption20 encryption20 = new Encryption20(header);
                    encryption20.Unpack(input, output);
                    break;
                case HeaderMagic10:
                    break;
                default:
                    Logger.Error($"Invalid magic: {header.Magic}");
                    break;
            }
        }

        private const string HeaderMagic31 = "FilePackVer3.1"; // length 16, padded with nulls
        private const string HeaderMagic30 = "FilePackVer3.0";
        private const string HeaderMagic20 = "FilePackVer2.0";
        private const string HeaderMagic10 = "FilePackVer1.0";
    }

    internal class QlieHeader        // length: 0x1C
    {
        public string Magic { get; set; }
        public int FileCount { get; set; }
        public long IndexOffset { get; set; }
    }

    internal class QlieKey           // length: 0x20 + 0x4 + 0x400 = 0x424
    {
        public byte[] Magic { get; set; }
        public uint HashSize { get; set; }
        public byte[] Key { get; set; }    // 0x400, 0x100 key + 0x300 padding
    }

    internal class QlieHash          // length: QlieKey.HashSize
    {
        public string Magic { get; set; }   // "HashVer1.4"
        public uint C { get; set; }         // 0x100
        public int FileCount { get; set; }
        public uint IndexSize { get; set; } // 4 * FileCount
        public uint DataSize { get; set; }
        public bool IsCompressed { get; set; }
        public byte[] Unknown { get; set; } // length: 0x20
        public byte[] HashData { get; set; }// length: QlieKey.HashSize - 0x44
    }

    internal class QlieEntry : PackedEntry
    {
        public new long Offset { get; set; }
        public uint Hash { get; set; }       // for check, not necessary
        public uint IsEncrypted { get; set; }
    }
}
