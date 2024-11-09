using GalArc.Extensions.GARbroDB;
using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Utility;
using Utility.Extensions;

namespace ArcFormats.AnimeGameSystem
{
    internal class DAT
    {
        public static UserControl UnpackExtraOptions = new UnpackDATOptions();

        private static readonly byte[] Magic = Utils.HexStringToByteArray("7061636B");  // 'pack'

        internal static AGSScheme ImportedSchemes;

        internal static AGSScheme.AGSFileMap SelectedScheme;

        private class Entry
        {
            public string Name { get; set; }
            public long Offset { get; set; }
            public uint Size { get; set; }
        }

        public void Unpack(string filePath, string folderPath)
        {
            AGSScheme.Key key = null;
            bool isXored = ImportedSchemes != null && SelectedScheme != null && ImportedSchemes.EncryptedArchives.Any(s => StringComparer.OrdinalIgnoreCase.Equals(s, Path.GetFileName(filePath))) && SelectedScheme.FileMap.TryGetValue(Path.GetFileName(filePath), out key);
            using (FileStream fs = File.OpenRead(filePath))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    fs.Position = 4;
                    uint fileCount = br.ReadUInt16();
                    if (fileCount != 0 && fileCount < 4000)
                    {
                        uint indexOffset = 6;
                        uint indexSize = fileCount * 0x18;
                        var entries = new List<Entry>((int)fileCount);
                        for (int i = 0; i < fileCount; i++)
                        {
                            var entry = new Entry();
                            entry.Name = br.ReadCString();
                            fs.Position = indexOffset + 0x10;
                            entry.Offset = br.ReadInt32();
                            fs.Position = indexOffset + 0x14;
                            entry.Size = br.ReadUInt32();
                            indexOffset += 0x18;
                            entries.Add(entry);
                        }

                        Logger.InitBar(fileCount);
                        Directory.CreateDirectory(folderPath);
                        foreach (var entry in entries)
                        {
                            fs.Position = entry.Offset;
                            byte[] data = br.ReadBytes((int)entry.Size);
                            string fileName = Path.Combine(folderPath, entry.Name);
                            if (isXored)
                            {
                                Decrypt(data, (byte)key.Initial, (byte)key.Increment);
                            }
                            File.WriteAllBytes(fileName, data);
                            Logger.UpdateBar();
                            data = null;
                        }
                        Logger.UpdateBar();
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        private void Decrypt(byte[] data, byte initial, byte increment)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ initial);
                initial += increment;
            }
        }

        public void Pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);

            FileInfo[] files = new DirectoryInfo(folderPath).GetFiles();
            Logger.InitBar(files.Length);

            // Write magic and file count
            bw.Write(Magic);
            bw.Write((ushort)files.Length);

            // Set initial index offset (right after magic and file count)
            uint indexOffset = 6;
            uint dataOffset = indexOffset + (uint)(files.Length * 0x18); // index size is fileCount * 0x18

            // Write index entries with placeholder offsets
            foreach (FileInfo file in files)
            {
                // Set position for each index entry
                fw.Position = indexOffset;

                // Write file name (padded to 0x10 bytes)
                byte[] nameBytes = Utils.PaddedBytes(file.Name, 0x10);
                bw.Write(nameBytes);

                // Placeholder for offset (will update after writing data)
                bw.Write(0);

                // Write file size
                bw.Write((uint)file.Length);

                indexOffset += 0x18;
            }

            // Write file data and update offsets in the index
            indexOffset = 6; // Reset to start of index entries
            foreach (FileInfo file in files)
            {
                // Update file offset in the index
                fw.Position = indexOffset + 0x10;
                bw.Write(dataOffset);

                // Write file data
                fw.Position = dataOffset;
                byte[] fileData = File.ReadAllBytes(file.FullName);
                bw.Write(fileData);

                // Advance to the next data offset, aligned to 4 bytes
                dataOffset += (uint)fileData.Length;
                indexOffset += 0x18;
                Logger.UpdateBar();
                fileData = null;
            }
            fw.Dispose();
            bw.Dispose();
        }

    }
}
