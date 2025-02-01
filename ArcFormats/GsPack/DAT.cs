using GalArc.Logs;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Utility.Compression;

namespace ArcFormats.GsPack
{
    public class DAT : PAK
    {
        private static readonly Lazy<UserControl> _lazyUnpackOptions = new Lazy<UserControl>(() => new UnpackPAKOptions());
        public static new UserControl UnpackExtraOptions => _lazyUnpackOptions.Value;

        private readonly string Magic = "GsSYMBOL5BINDATA";

        public override void Unpack(string filePath, string folderPath)
        {
            using (FileStream fs = File.OpenRead(filePath))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    string magic = Encoding.ASCII.GetString(br.ReadBytes(9)).TrimEnd('\0');
                    foreach (string validMagic in ValidMagics)
                    {
                        if (magic.StartsWith(validMagic, StringComparison.OrdinalIgnoreCase))
                        {
                            base.Unpack(filePath, folderPath);
                            return;
                        }
                    }

                    fs.Position = 0;
                    if (!Magic.Equals(Encoding.ASCII.GetString(br.ReadBytes(16))))
                    {
                        Logger.ErrorInvalidArchive();
                    }
                    fs.Position = 0xa4;
                    uint headerSize = br.ReadUInt32();
                    int fileCount = br.ReadInt32();
                    fs.Position += 12;
                    uint indexOffset = br.ReadUInt32();
                    uint indexSize = br.ReadUInt32();
                    uint key = br.ReadUInt32();
                    uint unpackedIndexSize = br.ReadUInt32();
                    uint dataOffset = br.ReadUInt32();
                    if (unpackedIndexSize != 0x18 * fileCount)
                    {
                        Logger.ErrorInvalidArchive();
                    }
                    fs.Position = indexOffset;
                    byte[] packedIndex = br.ReadBytes((int)indexSize);
                    for (int i = 0; i < indexSize; i++)
                    {
                        packedIndex[i] ^= (byte)(i & key);
                    }
                    byte[] index = LzssHelper.Decompress(packedIndex);
                    Logger.InitBar(fileCount);
                    Directory.CreateDirectory(folderPath);
                    using (MemoryStream ms = new MemoryStream(index))
                    {
                        using (BinaryReader indexBr = new BinaryReader(ms))
                        {
                            for (int i = 0; i < fileCount; i++)
                            {
                                string name = i.ToString("D6");
                                uint offset = indexBr.ReadUInt32() + dataOffset;
                                uint size = indexBr.ReadUInt32();
                                ms.Position += 16;
                                fs.Position = offset;
                                byte[] data = br.ReadBytes((int)size);
                                File.WriteAllBytes(Path.Combine(folderPath, name), LzssHelper.Decompress(data));
                                Logger.UpdateBar();
                                data = null;
                            }
                        }
                    }
                }
            }
        }
    }
}
