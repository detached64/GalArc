using Log;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utility;
using Utility.Compression;

namespace ArcFormats.Kirikiri
{
    public class XP3
    {
        public static UserControl PackExtraOptions = new Templates.VersionOnly("1/2");

        private struct Header
        {
            internal static byte[] magic = { 0x58, 0x50, 0x33, 0x0d, 0x0a, 0x20, 0x0a, 0x1a, 0x8b, 0x67, 0x01 };
            internal ulong indexOffset { get; set; }
        }

        private struct Entry
        {
            internal ulong unpackedFileSize { get; set; }
            internal ulong packedFileSize { get; set; }
            internal string relativePath { get; set; }
            internal long dataOffset { get; set; }
            internal bool isCompressed { get; set; }
            internal string path { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);

            if (!br.ReadBytes(11).SequenceEqual(Header.magic))
            {
                LogUtility.Error_NotValidArchive();
            }
            if (br.ReadByte() == 0x17)
            {
                LogUtility.ShowVersion("xp3", 2);
                br.ReadBytes(20);
            }
            else
            {
                LogUtility.ShowVersion("xp3", 1);
                fs.Position--;
            }
            uint indexOffset = br.ReadUInt32();
            fs.Position = indexOffset;
            byte[] Index;
            switch (br.ReadByte())
            {
                case 0:                    //index uncompressed
                    long indexSize = br.ReadInt64();
                    Index = br.ReadBytes((int)indexSize);
                    if (fs.Position != new FileInfo(filePath).Length)
                    {
                        LogUtility.Error("Error:additional bytes beyond index.");
                    }
                    break;

                case 1:                    //index compressed
                    long packedIndexSize = br.ReadInt64();
                    long unpackedIndexSize = br.ReadInt64();
                    byte[] packedIndex = br.ReadBytes((int)packedIndexSize);
                    if (fs.Position != new FileInfo(filePath).Length)
                    {
                        LogUtility.Error("Error:additional bytes beyond index.");
                    }
                    Index = Zlib.DecompressBytes(packedIndex);
                    if (Index.Length != unpackedIndexSize)
                    {
                        LogUtility.Info("Index size fails to match.Try reading……");
                    }
                    break;

                default:
                    LogUtility.Error_NotValidArchive();
                    return;
            }
            MemoryStream ms = new MemoryStream(Index);
            BinaryReader brIndex = new BinaryReader(ms);
            ms.Position = 0;
            while (ms.Position < ms.Length)
            {
                string m1 = Encoding.ASCII.GetString(brIndex.ReadBytes(4));     //"File"
                if (m1 != "File")
                {
                    LogUtility.Error_NotValidArchive();
                }
                Entry entry = new Entry();
                long _remaining = brIndex.ReadInt64();
                long thisPos = brIndex.BaseStream.Position;
                long nextPos = thisPos + _remaining;

                while (_remaining > 0)
                {
                    string _magic = Encoding.ASCII.GetString(brIndex.ReadBytes(4));
                    long sectionLen = brIndex.ReadInt64();
                    _remaining = _remaining - 12 - sectionLen;
                    switch (_magic)
                    {
                        case "info":
                            int flag = brIndex.ReadInt32();
                            if (flag != 0)
                            {
                                LogUtility.Info("Encrypted file detected, skipping...");
                                goto NextEntry;
                            }
                            entry.unpackedFileSize = brIndex.ReadUInt64();
                            entry.packedFileSize = brIndex.ReadUInt64();
                            ushort fileNameLen = brIndex.ReadUInt16();
                            entry.relativePath = Encoding.Unicode.GetString(brIndex.ReadBytes(fileNameLen * 2));
                            entry.path = folderPath + "\\" + entry.relativePath.Replace("/", "\\");
                            break;

                        case "segm":
                            entry.isCompressed = brIndex.ReadInt32() == 1;
                            entry.dataOffset = brIndex.ReadInt64();
                            brIndex.ReadBytes((int)sectionLen - 12);
                            break;

                        case "adlr":
                            brIndex.ReadBytes(4);
                            break;
                    }
                }

                fs.Position = entry.dataOffset;

                Directory.CreateDirectory(Path.GetDirectoryName(entry.path));
                //LogUtility.Debug(entry.packedFileSize.ToString());
                byte[] data = br.ReadBytes((int)entry.packedFileSize);
                if (entry.unpackedFileSize == entry.packedFileSize)
                {
                    File.WriteAllBytes(entry.path, data);
                }
                else
                {
                    File.WriteAllBytes(entry.path, Zlib.DecompressBytes(data));
                }
                LogUtility.Debug(entry.path);

NextEntry:      ms.Position = nextPos;
            }
            ms.Dispose();
            brIndex.Dispose();
            fs.Dispose();
            br.Dispose();
        }

        public void Pack(string folderPath, string filePath)
        {
            Stream xp3Stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(xp3Stream);
            bw.Write(Header.magic);
            if (Global.Version == "2")
            {
                bw.Write((long)0x17);
                bw.Write(1);
                bw.Write((byte)0x80);
                bw.Write((long)0);
            }
            bw.Write((long)0);//index offset to 0x00
            LogUtility.InitBar(Utilities.GetFileCount_All(folderPath));

            DirectoryInfo d = new DirectoryInfo(folderPath);
            MemoryStream ms = new MemoryStream();
            BinaryWriter bwEntry = new BinaryWriter(ms);

            foreach (FileInfo f in d.GetFiles("*", SearchOption.AllDirectories))
            {
                long offset = xp3Stream.Position;
                Zlib.AppendCompressedFile(xp3Stream, f.FullName, out long originalSize, out long compressedSize);
                //File
                bwEntry.Write(Encoding.ASCII.GetBytes("File"));
                string thisFilePath = f.FullName.Substring(folderPath.Length + 1).Replace("\\", "/");
                bwEntry.Write((long)(90 + 2 * thisFilePath.Length));
                //info
                bwEntry.Write(Encoding.ASCII.GetBytes("info"));
                bwEntry.Write((long)(22 + 2 * thisFilePath.Length));
                bwEntry.Write(0);           //no crypt
                bwEntry.Write(f.Length);
                bwEntry.Write(compressedSize);
                bwEntry.Write((ushort)thisFilePath.Length);
                bwEntry.Write(Encoding.Unicode.GetBytes(thisFilePath));
                //segment
                bwEntry.Write(Encoding.ASCII.GetBytes("segm"));
                bwEntry.Write((long)0x1c);  //fixed
                bwEntry.Write(1);
                bwEntry.Write(offset);
                offset += compressedSize;
                bwEntry.Write(f.Length);
                bwEntry.Write(compressedSize);
                //adler
                bwEntry.Write(Encoding.ASCII.GetBytes("adlr"));
                bwEntry.Write((long)4);     //fixed
                bwEntry.Write(0);
                LogUtility.UpdateBar();
            }
            bw.Write((byte)1);          //1     compress index
            long uncomLen = ms.Length;
            byte[] compressedIndex = Zlib.CompressBytes(ms.ToArray());
            long comLen = compressedIndex.Length;
            bw.Write(comLen);           //8
            bw.Write(uncomLen);         //8
            bw.Write(compressedIndex);
            //index offset
            long indexOffset = xp3Stream.Length - 8 - 1 - 8 - comLen;
            xp3Stream.Position = Global.Version == "1" ? Header.magic.Length : 32;
            bw.Write(indexOffset);

            xp3Stream.Dispose();
        }
    }
}