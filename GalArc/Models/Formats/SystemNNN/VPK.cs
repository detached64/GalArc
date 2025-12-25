using GalArc.I18n;
using GalArc.Infrastructure.Logging;
using GalArc.Infrastructure.Progress;
using GalArc.Models.Formats.Commons;
using System.IO;
using System.Text;

namespace GalArc.Models.Formats.SystemNNN;

internal class VPK : ArcFormat
{
    public override string Name => "VPK";
    public override string Description => "SystemNNN VPK Archive";
    public override bool CanWrite => true;

    public override void Unpack(string filePath, string folderPath)
    {
        string vpkPath;
        string vtbPath;
        vpkPath = filePath;
        vtbPath = Path.ChangeExtension(filePath, ".vtb");

        if (!File.Exists(vtbPath))
        {
            Logger.Error(MsgStrings.ErrorSpecifiedFileNotFound, Path.GetFileName(vtbPath));
        }

        int vtbSize = (int)new FileInfo(vtbPath).Length;
        int filecount = (vtbSize / 12) - 1;
        ProgressManager.SetMax(filecount);

        FileStream fs1 = File.OpenRead(vtbPath);
        BinaryReader br1 = new(fs1);
        FileStream fs2 = File.OpenRead(vpkPath);
        BinaryReader br2 = new(fs2);
        for (int i = 1; i < filecount; i++)
        {
            Entry entry = new();
            entry.Path = Path.Combine(folderPath, Encoding.UTF8.GetString(br1.ReadBytes(8)) + ".vaw");
            uint size1 = br1.ReadUInt32();
            fs1.Seek(8, SeekOrigin.Current);
            uint size2 = br1.ReadUInt32();
            entry.Size = size2 - size1;
            byte[] buffer = br2.ReadBytes((int)entry.Size);
            File.WriteAllBytes(entry.Path, buffer);
            buffer = null;
            fs1.Seek(12 * i, SeekOrigin.Begin);
            ProgressManager.Progress();
        }
        Entry last = new();
        last.Path = Path.Combine(folderPath, Encoding.UTF8.GetString(br1.ReadBytes(8)) + ".vaw");
        uint vpksizeBefore = br1.ReadUInt32();
        fs1.Seek(8, SeekOrigin.Current);

        uint vpksize = br1.ReadUInt32();
        last.Size = vpksize - vpksizeBefore;
        byte[] buf = br2.ReadBytes((int)last.Size);
        File.WriteAllBytes(last.Path, buf);
        buf = null;
        ProgressManager.Progress();

        fs1.Dispose();
        fs2.Dispose();
        br1.Dispose();
        br2.Dispose();
    }

    public override void Pack(string folderPath, string filePath)
    {
        int sizeToNow = 0;
        DirectoryInfo d = new(folderPath);
        FileInfo[] files = d.GetFiles("*.vaw");
        int filecount = files.Length;
        string vpkPath = filePath;
        string vtbPath = vpkPath.Contains(".vpk") ? vpkPath.Replace(".vpk", ".vtb") : vpkPath + ".vtb";
        using FileStream fs1 = File.Create(vtbPath);
        using FileStream fs2 = File.Create(vpkPath);
        using BinaryWriter writer1 = new(fs1);
        using BinaryWriter writer2 = new(fs2);
        ProgressManager.SetMax(filecount);

        foreach (FileInfo file in files)
        {
            writer1.Write(Encoding.UTF8.GetBytes(Path.GetFileNameWithoutExtension(file.FullName)));
            writer1.Write(sizeToNow);
            sizeToNow += (int)file.Length;

            byte[] buffer = File.ReadAllBytes(file.FullName);
            writer2.Write(buffer);
            buffer = null;
            ProgressManager.Progress();
        }
        writer1.Write(0);
        writer1.Write(0);
        writer1.Write(sizeToNow);
    }
}
