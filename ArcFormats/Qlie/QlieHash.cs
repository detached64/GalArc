using GalArc.Logs;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ArcFormats.Qlie
{
    internal interface IQlieHash
    {
        string Magic { get; set; }
        byte[] HashData { get; set; }
    }

    internal class QlieHash12 : IQlieHash      // The hash bytes lie in a separate file "datax.hash".
    {
        public string Magic { get; set; }
        public uint Const { get; set; }         // 0x200
        public int FileCount { get; set; }
        public uint IndexSize { get; set; }     // 2 * FileCount
        public uint HashDataSize { get; set; }
        public byte[] HashData { get; set; }    // length: QlieKey.HashSize - 32
    }

    internal class QlieHash13 : QlieHash12
    {
        public new uint Const { get; set; }     // 0x100
    }

    internal class QlieHash14 : IQlieHash
    {
        public string Magic { get; set; }
        public uint Const { get; set; }         // 0x100
        public int FileCount { get; set; }
        public uint IndexSize { get; set; }     // 4 * FileCount
        public uint HashDataSize { get; set; }
        public bool IsCompressed { get; set; }
        public byte[] Unknown { get; set; }     // length: 0x20
        public byte[] HashData { get; set; }    // length: QlieKey.HashSize - 0x44
    }

    internal class QlieHashReader
    {
        private readonly string HashMagicPattern = "^HashVer[0-9]\\.[0-9]$";

        private byte[] Hash;
        public int HashVersion { get; }

        public QlieHashReader(byte[] hash)
        {
            Hash = hash;
            string magic = Encoding.ASCII.GetString(Hash, 0, 16).TrimEnd('\0');
            if (!Regex.IsMatch(magic, HashMagicPattern))
            {
                throw new InvalidDataException("Invalid hash magic.");
            }
            int major = int.Parse(magic.Substring(7, 1));
            int minor = int.Parse(magic.Substring(9, 1));
            HashVersion = major * 10 + minor;
        }

        public byte[] GetHashData()
        {
            IQlieHash hash = GetHash();
            if (hash != null)
            {
                byte[] result = hash.HashData;
                if (result != null)
                {
                    return QlieEncryption.Decompress(result);
                }
            }
            Logger.Error("Failed to get hash data.");
            return null;
        }

        public IQlieHash GetHash()
        {
            switch (HashVersion)
            {
                case 12:
                    return GetHash12();
                case 13:
                    return GetHash13();
                case 14:
                    return GetHash14();
                default:
                    throw new ArgumentException("Unsupported hash version.");
            }
        }

        private IQlieHash GetHash12()
        {
            QlieHash12 qhash = new QlieHash12()
            {
                Const = BitConverter.ToUInt32(Hash, 16),
                FileCount = BitConverter.ToInt32(Hash, 20),
                IndexSize = BitConverter.ToUInt32(Hash, 24),
                HashDataSize = BitConverter.ToUInt32(Hash, 28),
                HashData = new byte[Hash.Length - 32]
            };
            Buffer.BlockCopy(Hash, 32, qhash.HashData, 0, qhash.HashData.Length);
            if (qhash.Const != 0x200 || !IsSaneCount(qhash.FileCount) || qhash.HashDataSize != qhash.HashData.Length)
            {
                throw new ArgumentException("Invalid hash ver 1.2.");
            }
            QlieEncryption.Decrypt(qhash.HashData, qhash.HashData.Length, 0x428);
            return qhash;
        }

        private IQlieHash GetHash13()
        {
            QlieHash13 qhash = new QlieHash13()
            {
                Const = BitConverter.ToUInt32(Hash, 16),
                FileCount = BitConverter.ToInt32(Hash, 20),
                IndexSize = BitConverter.ToUInt32(Hash, 24),
                HashDataSize = BitConverter.ToUInt32(Hash, 28),
                HashData = new byte[Hash.Length - 32]
            };
            Buffer.BlockCopy(Hash, 32, qhash.HashData, 0, qhash.HashData.Length);
            if (qhash.Const != 0x100 || !IsSaneCount(qhash.FileCount) || qhash.HashDataSize != qhash.HashData.Length)
            {
                throw new ArgumentException("Invalid hash ver 1.3.");
            }
            QlieEncryption.Decrypt(qhash.HashData, qhash.HashData.Length, 0x428);
            return qhash;
        }

        private IQlieHash GetHash14()
        {
            QlieHash14 qhash = new QlieHash14()
            {
                Const = BitConverter.ToUInt32(Hash, 16),
                FileCount = BitConverter.ToInt32(Hash, 20),
                IndexSize = BitConverter.ToUInt32(Hash, 24),
                HashDataSize = BitConverter.ToUInt32(Hash, 28),
                IsCompressed = BitConverter.ToUInt32(Hash, 32) != 0,
                Unknown = new byte[32],
                HashData = new byte[Hash.Length - 68]
            };
            Buffer.BlockCopy(Hash, 36, qhash.Unknown, 0, qhash.Unknown.Length);
            Buffer.BlockCopy(Hash, 68, qhash.HashData, 0, qhash.HashData.Length);
            if (qhash.Const != 0x100 || !IsSaneCount(qhash.FileCount) || qhash.IndexSize != qhash.FileCount * 4 && qhash.IndexSize != qhash.FileCount * 2 || qhash.HashDataSize != qhash.HashData.Length)
            {
                throw new ArgumentException("Invalid hash ver 1.4.");
            }
            QlieEncryption.Decrypt(qhash.HashData, qhash.HashData.Length, 0x428);
            return qhash;
        }

        private bool IsSaneCount(int count)
        {
            return count > 0 && count < 0x10000;
        }
    }
}
