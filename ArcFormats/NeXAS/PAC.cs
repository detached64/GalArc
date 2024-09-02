using Log;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Utility;
using Utility.Compression;

namespace ArcFormats.NeXAS
{
    public class PAC
    {
        private enum Method
        {
            None,
            Lzss,
            Huffman,
            Zlib,
            Zstd = 7
        }

        private static byte[] signature = { 0x50, 0x41, 0x43 };       //"PAC"

        public static void Unpack(string filePath, string folderPath, Encoding encoding)
        {
            FileStream fs = File.OpenRead(filePath);
            BinaryReader reader = new BinaryReader(fs);
            if (!reader.ReadBytes(3).SequenceEqual(signature))
            {
                LogUtility.Error_NotValidArchive();
            }
            reader.ReadByte();
            long fileSize = reader.BaseStream.Length;
            int fileCount = reader.ReadInt32();
            int methodCount = reader.ReadInt32();
            if (methodCount == 4)           // also zlib
            {
                methodCount = 3;
            }
            Method method = (Method)methodCount;

            fs.Seek(-4, SeekOrigin.End);
            int packedLen = reader.ReadInt32();
            int unpackedLen = fileCount * 76;
            fs.Seek(-4 - packedLen, SeekOrigin.End);
            byte[] packedIndex = Xor.xor(reader.ReadBytes(packedLen), 0xff);
            Directory.CreateDirectory(folderPath);
            LogUtility.InitBar(fileCount);

            byte[] index = Huffman.Decompress(packedIndex, unpackedLen);
            MemoryStream msIndex = new MemoryStream(index);
            BinaryReader readerIndex = new BinaryReader(msIndex);

            for (int i = 0; i < fileCount; i++)
            {
                string fileName = ArcEncoding.Shift_JIS.GetString(readerIndex.ReadBytes(64)).TrimEnd('\0');
                int offset = readerIndex.ReadInt32();
                int unpackedSize = readerIndex.ReadInt32();
                int packedSize = readerIndex.ReadInt32();
                string path = folderPath + "\\" + fileName;

                fs.Seek(offset, SeekOrigin.Begin);
                byte[] fileData = reader.ReadBytes(packedSize);


                if (unpackedSize != packedSize && method != Method.None) // compressed
                {
                    LogUtility.Debug("Packed file detected:" + fileName + ".Try " + method.ToString() + "……");
                    try
                    {
                        switch (method)
                        {
                            case Method.Huffman:
                                File.WriteAllBytes(path, Huffman.Decompress(fileData, unpackedSize));
                                break;
                            case Method.Lzss:
                                File.WriteAllBytes(path, Lzss.Decompress(fileData));
                                break;
                            case Method.Zlib:
                                File.WriteAllBytes(path, Zlib.DecompressBytes(fileData));
                                break;
                            case Method.Zstd:
                                File.WriteAllBytes(path, Zstd.Decompress(fileData));
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogUtility.Error("Decompress" + method.ToString() + " failed:" + fileName);
                        LogUtility.Debug(ex.Message);
                    }
                }
                else                                                    // No compression
                {
                    File.WriteAllBytes(folderPath + "\\" + fileName, fileData);
                }
                LogUtility.UpdateBar();
            }
            fs.Dispose();
            msIndex.Dispose();
            reader.Dispose();
            readerIndex.Dispose();
        }
    }
}
