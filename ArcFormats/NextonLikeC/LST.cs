using Log;
using System;
using System.Collections.Generic;
using System.IO;
using Utility;

namespace ArcFormats.NextonLikeC
{
    public class LST
    {
        private class Entry
        {
            public uint fileOffset { get; set; }
            public uint fileSize { get; set; }
            public string fileName { get; set; }
            public int fileType { get; set; }
            //1:script 2,3:image 4,5:audio
            //1:SNX 3:PNG 4,5:OGG
        }

        public void Unpack(string filePath, string folderPath)
        {
            string arcPath;
            string lstPath;

            //judge
            lstPath = filePath;
            arcPath = Path.ChangeExtension(filePath, string.Empty);
            if (!File.Exists(arcPath))
            {
                LogUtility.ErrorNeedAnotherFile(".lst", string.Empty);
            }

            FileStream fsLst = File.OpenRead(lstPath);
            BinaryReader brtemp = new BinaryReader(fsLst);
            fsLst.Position = 3;
            byte keyLst = brtemp.ReadByte();
            fsLst.Dispose();
            brtemp.Dispose();

            byte[] lst = Xor.xor(File.ReadAllBytes(lstPath), keyLst);
            MemoryStream ms = new MemoryStream(lst);
            BinaryReader brLst = new BinaryReader(ms);

            uint fileCount = brLst.ReadUInt32();
            List<Entry> l = new List<Entry>();
            LogUtility.InitBar(fileCount);

            for (int i = 0; i < (int)fileCount; i++)
            {
                Entry entry = new Entry();
                entry.fileOffset = brLst.ReadUInt32();
                entry.fileSize = brLst.ReadUInt32();
                entry.fileName = ArcEncoding.Shift_JIS.GetString(brLst.ReadBytes(64)).TrimEnd('\x02');
                entry.fileType = brLst.ReadByte() ^ keyLst;//only read one byte to convert to int
                brLst.ReadBytes(3);//000000
                l.Add(entry);
            }
            Directory.CreateDirectory(folderPath);

            FileStream fsArc = File.OpenRead(arcPath);
            BinaryReader brArc = new BinaryReader(fsArc);
            fsArc.Position = 3;
            byte keyArc = brArc.ReadByte();
            fsArc.Position = 0;
            for (int i = 0; i < (int)fileCount; i++)
            {
                switch (l[i].fileType)
                {
                    case 1://script
                        byte[] bufferSCR = Xor.xor(brArc.ReadBytes((int)l[i].fileSize), keyArc);
                        File.WriteAllBytes(folderPath + "\\" + l[i].fileName + ".SNX", bufferSCR);
                        break;

                    case 2:
                    case 3://image
                        byte[] bufferIMG = brArc.ReadBytes((int)l[i].fileSize);
                        File.WriteAllBytes(folderPath + "\\" + l[i].fileName + ".PNG", bufferIMG);
                        break;

                    case 4://audio
                        byte[] bufferAUD_WAV = brArc.ReadBytes((int)l[i].fileSize);
                        File.WriteAllBytes(folderPath + "\\" + l[i].fileName + ".WAV", bufferAUD_WAV);
                        break;

                    case 5:
                        byte[] bufferAUD_OGG = brArc.ReadBytes((int)l[i].fileSize);
                        File.WriteAllBytes(folderPath + "\\" + l[i].fileName + ".SNX", bufferAUD_OGG);
                        break;

                    default:
                        LogUtility.Info("Unrecognized file detected:" + l[i].fileName + "Skip." + Environment.NewLine);
                        break;
                }
                LogUtility.UpdateBar();
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