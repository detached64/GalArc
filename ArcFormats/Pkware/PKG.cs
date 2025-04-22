using GalArc.Database;
using GalArc.Logs;
using GalArc.Templates;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ArcFormats.Pkware
{
    public class PKG : ArcFormat
    {
        public override WidgetTemplate UnpackWidget => UnpackPkwareWidget.Instance;

        private PkwareOptions Options => UnpackPkwareWidget.Instance.Options;

        private PkwareScheme Scheme
        {
            get => UnpackPkwareWidget.Instance.Scheme;
            set => UnpackPkwareWidget.Instance.Scheme = value;
        }

        private readonly EncodingSetting PkgEncoding = new EncodingSetting("PkwarePkgEncoding");

        public override IEnumerable<ArcSetting> Settings => new[] { PkgEncoding };

        public override void Unpack(string filePath, string folderPath)
        {
            ZipStrings.CodePage = PkgEncoding.Get<Encoding>().CodePage;
            ZipFile file = new ZipFile(filePath);
            List<ZipEntry> entries = file.Cast<ZipEntry>().Where(e => !e.IsDirectory).ToList();
            bool is_encrypted = entries.Any(e => e.IsCrypted);
            if (is_encrypted)
            {
                switch (Path.GetFileName(filePath))
                {
                    case "data.pkg":
                        file.Password = Options.ContentKey;
                        break;
                    case "player.pkg":
                        file.Password = Options.PlayerKey;
                        break;
                }
            }
            Directory.CreateDirectory(folderPath);
            Logger.InitBar(entries.Count);
            foreach (ZipEntry entry in entries)
            {
                string name = entry.Name;
                string dir = Path.GetDirectoryName(name);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(Path.Combine(folderPath, dir));
                }
                string path = Path.Combine(folderPath, name);
                using (FileStream fs = File.Create(path))
                {
                    file.GetInputStream(entry).CopyTo(fs);
                }
                Logger.UpdateBar();
            }
            file.Close();
        }

        public override void DeserializeScheme(out string name, out int count)
        {
            Scheme = Deserializer.LoadScheme<PkwareScheme>();
            name = "Pkware";
            count = Scheme?.KnownKeys?.Count ?? 0;
        }
    }
}
