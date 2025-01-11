using GalArc.Logs;
using System;

namespace ArcFormats.Qlie
{
    internal class QlieHash : QlieEncryption
    {
        private byte[] Hash;
        private string Version;

        public QlieHash(string version)
        {
            Version = version;
        }

        public byte[] GetHash(byte[] raw)
        {
            Hash = raw;
            switch (Version)
            {
                case HashMagic1_3:
                    return GetHash1_3();
                case HashMagic1_4:
                    return GetHash1_4();
                default:
                    Logger.Error("Invalid version");
                    return null;
            }
        }

        private byte[] GetHash1_3()
        {
            QlieHash1_3 qhash = new QlieHash1_3()
            {
                Const = BitConverter.ToUInt32(Hash, 0),
                FileCount = BitConverter.ToInt32(Hash, 4),
                Unknown = BitConverter.ToUInt32(Hash, 8),
                DataSize = BitConverter.ToUInt32(Hash, 12),
                HashData = new byte[Hash.Length - 16]
            };
            Array.Copy(Hash, 16, qhash.HashData, 0, qhash.HashData.Length);
            if (qhash.Const != 0x100 || !IsSaneCount(qhash.FileCount) || qhash.DataSize != qhash.HashData.Length)
            {
                Logger.Error("Invalid hash ver 1.3.", false);
                return null;
            }
            Decrypt(qhash.HashData, qhash.HashData.Length, 0x428);
            return qhash.HashData;
        }

        private byte[] GetHash1_4()
        {
            QlieHash1_4 qhash = new QlieHash1_4()
            {
                Const = BitConverter.ToUInt32(Hash, 0),
                FileCount = BitConverter.ToInt32(Hash, 4),
                IndexSize = BitConverter.ToUInt32(Hash, 8),
                HashDataSize = BitConverter.ToUInt32(Hash, 12),
                IsCompressed = BitConverter.ToUInt32(Hash, 16) != 0,
                Unknown = new byte[32],
                HashData = new byte[Hash.Length - 52]
            };
            Array.Copy(Hash, 20, qhash.Unknown, 0, qhash.Unknown.Length);
            Array.Copy(Hash, 52, qhash.HashData, 0, qhash.HashData.Length);
            if (qhash.Const != 0x100 || !IsSaneCount(qhash.FileCount) || qhash.IndexSize != qhash.FileCount * 4 || qhash.HashDataSize != qhash.HashData.Length)
            {
                Logger.Error("Invalid hash ver 1.4.", false);
                return null;
            }
            Decrypt(qhash.HashData, qhash.HashData.Length, 0x428);
            return qhash.HashData;
        }

        private const string HashMagic1_4 = "HashVer1.4";
        private const string HashMagic1_3 = "HashVer1.3";
    }
}
