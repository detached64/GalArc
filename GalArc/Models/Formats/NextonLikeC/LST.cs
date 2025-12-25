using GalArc.I18n;
using GalArc.Infrastructure.Logging;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using GalArc.Models.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace GalArc.Models.Formats.NextonLikeC;

internal class LST : ArcFormat
{
    public override string Name => "LST";
    public override string Description => "NextonLikeC Archive";

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
            Logger.Error(MsgStrings.ErrorSpecifiedFileNotFound, Path.GetFileName(arcPath));
        }

        using FileStream fsLst = File.OpenRead(lstPath);
        using BinaryReader brtemp = new(fsLst);
        fsLst.Position = 3;
        byte lstKey = brtemp.ReadByte();
        fsLst.Dispose();
        brtemp.Dispose();

        byte[] lst = File.ReadAllBytes(lstPath);
        for (int i = 0; i < lst.Length; i++)
        {
            lst[i] ^= lstKey;
        }
        using MemoryStream ms = new(lst);
        using BinaryReader brLst = new(ms);

        int fileCount = brLst.ReadInt32();
        List<NextonEntry> l = [];
        ProgressManager.SetMax(fileCount);

        for (int i = 0; i < fileCount; i++)
        {
            NextonEntry entry = new();
            entry.Offset = brLst.ReadUInt32();
            entry.Size = brLst.ReadUInt32();
            entry.Name = ArcEncoding.Shift_JIS.GetString(brLst.ReadBytes(64)).TrimEnd('\x02');
            entry.Type = brLst.ReadByte() ^ lstKey;//only read one byte to convert to int
            brLst.ReadBytes(3);//000000
            l.Add(entry);
        }

        using FileStream fsArc = File.OpenRead(arcPath);
        using BinaryReader brArc = new(fsArc);
        fsArc.Position = 3;
        byte arcKey = brArc.ReadByte();
        fsArc.Position = 0;
        for (int i = 0; i < fileCount; i++)
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
            ProgressManager.Progress();
        }
    }
}
