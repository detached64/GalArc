using GalArc.Database;
using GalArc.Logs;
using GalArc.Templates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utility;
using Utility.Extensions;

namespace ArcFormats.AnimeGameSystem
{
    public class DAT : ArcFormat
    {
        public override WidgetTemplate UnpackWidget => UnpackDATWidget.Instance;

        private AGSOptions UnpackOptions => UnpackDATWidget.Instance.Options;

        private AGSScheme Scheme
        {
            get => UnpackDATWidget.Instance.Scheme;
            set => UnpackDATWidget.Instance.Scheme = value;
        }

        private const string Magic = "pack";

        public override void Unpack(string filePath, string folderPath)
        {
            AGSScheme.Key key = null;
            bool isXored = Scheme != null &&
                UnpackOptions.FileMap != null &&
                Scheme.EncryptedArchives.Any(s => StringComparer.OrdinalIgnoreCase
                .Equals(s, Path.GetFileName(filePath))) &&
                UnpackOptions.FileMap.TryGetValue(Path.GetFileName(filePath), out key);
            using (FileStream fs = File.OpenRead(filePath))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    fs.Position = 4;
                    int fileCount = br.ReadUInt16();
                    if (!IsSaneCount(fileCount))
                    {
                        throw new ArgumentOutOfRangeException(nameof(fileCount));
                    }
                    uint indexOffset = 6;
                    uint indexSize = (uint)fileCount * 0x18;
                    var entries = new List<Entry>(fileCount);
                    for (int i = 0; i < fileCount; i++)
                    {
                        var entry = new Entry();
                        entry.Name = br.ReadCString();
                        fs.Position = indexOffset + 0x10;
                        entry.Offset = br.ReadUInt32();
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
                }
            }
        }

        private void Decrypt(byte[] data, byte initial, byte increment)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= initial;
                initial += increment;
            }
        }

        public override void Pack(string folderPath, string filePath)
        {
            FileStream fw = File.Create(filePath);
            BinaryWriter bw = new BinaryWriter(fw);

            FileInfo[] files = new DirectoryInfo(folderPath).GetFiles();
            Logger.InitBar(files.Length);

            bw.Write(Magic);
            bw.Write((ushort)files.Length);

            uint indexOffset = 6;
            uint dataOffset = indexOffset + (uint)(files.Length * 0x18); // index size is fileCount * 0x18

            foreach (FileInfo file in files)
            {
                fw.Position = indexOffset;
                byte[] nameBytes = Utils.GetPaddedBytes(file.Name, 0x10);
                bw.Write(nameBytes);
                bw.Write(0);
                bw.Write((uint)file.Length);
                indexOffset += 0x18;
            }

            indexOffset = 6;
            foreach (FileInfo file in files)
            {
                fw.Position = indexOffset + 0x10;
                bw.Write(dataOffset);
                fw.Position = dataOffset;
                byte[] fileData = File.ReadAllBytes(file.FullName);
                bw.Write(fileData);
                dataOffset += (uint)fileData.Length;
                indexOffset += 0x18;
                Logger.UpdateBar();
                fileData = null;
            }
            fw.Dispose();
            bw.Dispose();
        }

        public override void DeserializeScheme(out string name, out int count)
        {
            Scheme = Deserializer.LoadScheme<AGSScheme>();
            name = "AnimeGameSystem";
            count = Scheme?.KnownSchemes?.Count ?? 0;
        }
    }
}
