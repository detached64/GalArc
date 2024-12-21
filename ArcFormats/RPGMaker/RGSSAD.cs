using GalArc.Logs;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Utility;

namespace ArcFormats.RPGMaker
{
    public class RGSSAD : ArchiveFormat
    {
        public static UserControl PackExtraOptions = new PackRGSSOptions("1/3");

        public override void Unpack(string filePath, string folderPath)
        {
            int version;
            using (FileStream fs = File.OpenRead(filePath))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    if (br.ReadUInt32() != 0x53534752) // "RGSS"
                    {
                        Logger.ErrorInvalidArchive();
                    }
                    fs.Position = 7;
                    version = br.ReadByte();
                }
            }

            switch (version)
            {
                case 1:
                    Logger.ShowVersion(Path.GetExtension(filePath).Trim('.'), version);
                    rgssV1_unpack(filePath, folderPath);
                    break;
                case 3:
                    Logger.ShowVersion(Path.GetExtension(filePath).Trim('.'), version);
                    rgssV3_unpack(filePath, folderPath);
                    break;
                default:
                    Logger.Error($"Error: version {version} not recognized.");
                    break;
            }
        }

        public override void Pack(string folderPath, string filePath)
        {
            switch (ArcSettings.Version)
            {
                case "1":
                    rgssV1_pack(folderPath, filePath);
                    break;
                case "3":
                    rgssV3_pack(folderPath, filePath);
                    break;
            }
        }

        private void rgssV1_unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            Directory.CreateDirectory(folderPath);
            KeyGen keygen = new KeyGen(0xDEADCAFE);
            fs.Position = 8;
            long fileCount = 0;
            while (fs.Position < fs.Length)
            {
                uint nameLen = br.ReadUInt32() ^ keygen.Compute();
                byte[] nameBytes = br.ReadBytes((int)nameLen);
                string name = Encoding.UTF8.GetString(DecryptName(nameBytes, keygen));
                string fullPath = Path.Combine(folderPath, name);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                uint size = br.ReadUInt32() ^ keygen.Compute();
                byte[] data = br.ReadBytes((int)size);
                DecryptData(data, new KeyGen(keygen.GetCurrent()));
                File.WriteAllBytes(fullPath, data);
                data = null;
                fileCount++;
            }
            fs.Dispose();
            br.Dispose();
        }

        private void rgssV3_unpack(string filePath, string folderPath)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader br = new BinaryReader(fs);
            fs.Position = 8;
            uint seed = br.ReadUInt32();
            Logger.Debug(string.Format(RPGMaker.logSeed, $"{seed:X8}"));
            uint key = seed * 9 + 3;
            long fileCount = 0;
            bool isFirst = true;
            long maxIndex = 13;
            Directory.CreateDirectory(folderPath);
            while (fs.Position < maxIndex)
            {
                uint dataOffset = br.ReadUInt32() ^ key;
                if (isFirst)
                {
                    maxIndex = dataOffset - 16;
                    isFirst = false;
                }
                uint fileSize = br.ReadUInt32() ^ key;
                uint thisKey = br.ReadUInt32() ^ key;
                uint nameLen = br.ReadUInt32() ^ key;
                string fullPath = Path.Combine(folderPath, Encoding.UTF8.GetString(DecryptName(br.ReadBytes((int)nameLen), key)));
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                long pos = fs.Position;
                fs.Position = dataOffset;
                byte[] data = br.ReadBytes((int)fileSize);
                DecryptData(data, new KeyGen(thisKey));
                File.WriteAllBytes(fullPath, data);
                data = null;
                fs.Position = pos;
                fileCount++;
            }
            fs.Dispose();
            br.Dispose();
        }

        private void rgssV1_pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            bw.Write(Encoding.ASCII.GetBytes("RGSSAD\0"));
            bw.Write((byte)1);
            DirectoryInfo d = new DirectoryInfo(folderPath);
            FileInfo[] files = d.GetFiles("*", SearchOption.AllDirectories);

            Logger.InitBar(files.Length);
            KeyGen keygen = new KeyGen(0xDEADCAFE);
            foreach (FileInfo file in files)
            {
                byte[] relativePath = Encoding.UTF8.GetBytes(file.FullName.Substring(folderPath.Length + 1));
                bw.Write((uint)relativePath.Length ^ keygen.Compute());
                bw.Write(DecryptName(relativePath, keygen));
                byte[] data = File.ReadAllBytes(file.FullName);
                bw.Write((uint)data.Length ^ keygen.Compute());
                DecryptData(data, new KeyGen(keygen.GetCurrent()));
                bw.Write(data);
                data = null;
                Logger.UpdateBar();
            }
            bw.Dispose();
            fw.Dispose();
        }

        private void rgssV3_pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);
            bw.Write(Encoding.ASCII.GetBytes("RGSSAD\0"));
            bw.Write((byte)3);
            uint seed = Convert.ToUInt32(PackRGSSOptions.inputSeedString, 16);
            bw.Write(seed);
            uint key = 9 * seed + 3;
            string[] files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
            uint fileCount = (uint)files.Length;
            uint baseOffset = 16 * fileCount + 12 + 16;

            foreach (string file in files)
            {
                baseOffset += (uint)Encoding.UTF8.GetByteCount(file.Substring(folderPath.Length + 1));
            }

            Logger.InitBar(files.Length);
            foreach (string file in files)
            {
                byte[] relativePath = Encoding.UTF8.GetBytes(file.Substring(folderPath.Length + 1));
                byte[] data = File.ReadAllBytes(file);
                uint thisKey = 0;
                bw.Write(baseOffset ^ key);
                bw.Write((uint)data.Length ^ key);
                bw.Write(thisKey ^ key);
                bw.Write((uint)relativePath.Length ^ key);
                bw.Write(DecryptName(relativePath, key));
                long pos = fw.Position;
                fw.Position = baseOffset;
                DecryptData(data, new KeyGen(thisKey));
                bw.Write(data);
                baseOffset += (uint)data.Length;
                data = null;
                fw.Position = pos;
                Logger.UpdateBar();
            }
            bw.Write(key);
            bw.Write(0);
            bw.Write((long)0);
            bw.Dispose();
            fw.Dispose();
        }

        private byte[] DecryptName(byte[] data, KeyGen keygen)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= (byte)keygen.Compute();
            }
            return data;
        }

        private byte[] DecryptName(byte[] data, uint key)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= (byte)(key >> (i << 3));
            }
            return data;
        }

        private void DecryptData(byte[] data, KeyGen keygen)
        {
            uint key = keygen.Compute();
            for (int i = 0; i < data.Length;)
            {
                data[i] ^= (byte)(key >> (i << 3));
                i++;
                if ((i & 3) == 0)
                {
                    key = keygen.Compute();
                }
            }
        }

        private class KeyGen
        {
            private uint m_seed;

            public KeyGen(uint seed)
            {
                m_seed = seed;
            }

            public uint Compute()
            {
                uint result = m_seed;
                m_seed = m_seed * 7 + 3;
                return result;
            }

            public uint GetCurrent()
            {
                return m_seed;
            }
        }
    }

    public class RGSS2A : RGSSAD
    {
        public static new UserControl PackExtraOptions = RGSSAD.PackExtraOptions;
    }

    public class RGSS3A : RGSSAD
    {
        public static new UserControl PackExtraOptions = RGSSAD.PackExtraOptions;
    }
}