using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using Utility;

namespace ArcFormats.NextonLikeC
{
    public class LST : ArchiveFormat
    {
        private class NextonEntry : Entry
        {
            public int Type { get; set; }
            //1:script 2,3:image 4,5:audio
            //1:SNX 3:PNG 4,5:OGG
        }

        public override void Unpack(string filePath, string folderPath)
        {
            string arcPath;
            string lstPath = filePath;
            arcPath = Path.ChangeExtension(filePath, string.Empty);
            if (!File.Exists(arcPath))
            {
                Logger.ErrorNeedAnotherFile(Path.GetFileName(arcPath));
            }

            FileStream fsLst = File.OpenRead(lstPath);
            BinaryReader brtemp = new BinaryReader(fsLst);
            fsLst.Position = 3;
            byte lstKey = brtemp.ReadByte();
            fsLst.Dispose();
            brtemp.Dispose();

            byte[] lst = File.ReadAllBytes(lstPath);
            for (int i = 0; i < lst.Length; i++)
            {
                lst[i] ^= lstKey;
            }
            MemoryStream ms = new MemoryStream(lst);
            BinaryReader brLst = new BinaryReader(ms);

            uint fileCount = brLst.ReadUInt32();
            List<NextonEntry> l = new List<NextonEntry>();
            Logger.InitBar(fileCount);

            for (int i = 0; i < (int)fileCount; i++)
            {
                NextonEntry entry = new NextonEntry();
                entry.Offset = brLst.ReadUInt32();
                entry.Size = brLst.ReadUInt32();
                entry.Name = ArcEncoding.Shift_JIS.GetString(brLst.ReadBytes(64)).TrimEnd('\x02');
                entry.Type = brLst.ReadByte() ^ lstKey;//only read one byte to convert to int
                brLst.ReadBytes(3);//000000
                l.Add(entry);
            }
            Directory.CreateDirectory(folderPath);

            FileStream fsArc = File.OpenRead(arcPath);
            BinaryReader brArc = new BinaryReader(fsArc);
            fsArc.Position = 3;
            byte arcKey = brArc.ReadByte();
            fsArc.Position = 0;
            for (int i = 0; i < (int)fileCount; i++)
            {
                switch (l[i].Type)
                {
                    case 1://script
                        byte[] bufferSCR = brArc.ReadBytes((int)l[i].Size);
                        for (int j = 0; j < bufferSCR.Length; j++)
                        {
                            bufferSCR[j] ^= arcKey;
                        }
                        File.WriteAllBytes(Path.Combine(folderPath, l[i].Name + ".SNX"), bufferSCR);
                        break;

                    case 2:
                    case 3://image
                        byte[] bufferIMG = brArc.ReadBytes((int)l[i].Size);
                        File.WriteAllBytes(Path.Combine(folderPath, l[i].Name + ".PNG"), bufferIMG);
                        break;

                    case 4://audio
                        byte[] bufferAUD_WAV = brArc.ReadBytes((int)l[i].Size);
                        File.WriteAllBytes(Path.Combine(folderPath, l[i].Name + ".WAV"), bufferAUD_WAV);
                        break;

                    case 5:
                        byte[] bufferAUD_OGG = brArc.ReadBytes((int)l[i].Size);
                        File.WriteAllBytes(Path.Combine(folderPath, l[i].Name + ".OGG"), bufferAUD_OGG);
                        break;

                    default:
                        Logger.Info("Unrecognized file detected:" + l[i].Name + "Skip." + Environment.NewLine);
                        break;
                }
                Logger.UpdateBar();
            }
            ms.Dispose();
            fsArc.Dispose();
            fsLst.Dispose();
            brArc.Dispose();
            brLst.Dispose();
            brtemp.Dispose();
        }
    }
}