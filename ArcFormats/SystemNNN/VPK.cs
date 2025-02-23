using GalArc.Logs;
using System;
using System.IO;
using System.Text;

namespace ArcFormats.SystemNNN
{
    public class VPK : ArchiveFormat
    {
        public override void Unpack(string filePath, string folderPath)
        {
            //init
            string vpkPath;
            string vtbPath;
            vpkPath = filePath;
            vtbPath = Path.ChangeExtension(filePath, ".vtb");

            if (!File.Exists(vtbPath))
            {
                Logger.ErrorNeedAnotherFile(Path.GetFileName(vtbPath));
            }

            int vtbSize = (int)new FileInfo(vtbPath).Length;
            int filecount = (vtbSize / 12) - 1;//file size = 12*file count + 12
            Logger.InitBar(filecount);

            //open&make dir
            FileStream fs1 = File.OpenRead(vtbPath);
            BinaryReader br1 = new BinaryReader(fs1);
            FileStream fs2 = File.OpenRead(vpkPath);
            BinaryReader br2 = new BinaryReader(fs2);
            Directory.CreateDirectory(folderPath);
            //1~n-1
            for (int i = 1; i < filecount; i++)
            {
                Entry entry = new Entry();
                entry.Path = Path.Combine(folderPath, Encoding.UTF8.GetString(br1.ReadBytes(8)) + ".vaw");
                uint size1 = br1.ReadUInt32();
                fs1.Seek(8, SeekOrigin.Current);
                uint size2 = br1.ReadUInt32();
                entry.Size = size2 - size1;
                byte[] buffer = br2.ReadBytes((int)entry.Size);
                File.WriteAllBytes(entry.Path, buffer);
                buffer = null;
                fs1.Seek(12 * i, SeekOrigin.Begin);
                Logger.UpdateBar();
            }
            //the last
            Entry last = new Entry();
            last.Path = Path.Combine(folderPath, Encoding.UTF8.GetString(br1.ReadBytes(8)) + ".vaw");
            uint vpksizeBefore = br1.ReadUInt32();
            fs1.Seek(8, SeekOrigin.Current);//reserve

            uint vpksize = br1.ReadUInt32();
            last.Size = vpksize - vpksizeBefore;
            byte[] buf = br2.ReadBytes((int)last.Size);
            File.WriteAllBytes(last.Path, buf);
            buf = null;
            Logger.UpdateBar();

            fs1.Dispose();
            fs2.Dispose();
            br1.Dispose();
            br2.Dispose();
        }

        public override void Pack(string folderPath, string filePath)
        {
            //init
            int sizeToNow = 0;
            DirectoryInfo d = new DirectoryInfo(folderPath);
            FileInfo[] files = d.GetFiles("*.vaw");
            int filecount = files.Length;
            string vpkPath = filePath;
            string vtbPath = vpkPath.Contains(".vpk") ? vpkPath.Replace(".vpk", ".vtb") : vpkPath + ".vtb";
            FileStream fs1 = File.Create(vtbPath);
            FileStream fs2 = File.Create(vpkPath);
            BinaryWriter writer1 = new BinaryWriter(fs1);
            BinaryWriter writer2 = new BinaryWriter(fs2);
            Logger.InitBar(filecount);

            foreach (FileInfo file in files)
            {
                writer1.Write(Encoding.UTF8.GetBytes(Path.GetFileNameWithoutExtension(file.FullName)));
                writer1.Write(sizeToNow);
                sizeToNow += (int)file.Length;

                byte[] buffer = File.ReadAllBytes(file.FullName);
                writer2.Write(buffer);
                buffer = null;
                Logger.UpdateBar();
            }
            writer1.Write(0);
            writer1.Write(0);
            writer1.Write(sizeToNow);

            fs1.Dispose();
            fs2.Dispose();
        }
    }
}