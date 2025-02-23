using ArcFormats;
using GalArc.Common;
using GalArc.GUI.Properties;
using GalArc.Logs;
using GalArc.Strings;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace GalArc.GUI
{
    internal class ArchivePackager : IDisposable
    {
        private string input;
        private string output;
        private string engineName;
        private string extension;

        private string typeName
        {
            get { return $"{nameof(ArcFormats)}.{engineName}.{extension}"; }
        }

        private Assembly _assembly = null;
        private Assembly assembly => _assembly ?? (_assembly = Assembly.Load(nameof(ArcFormats)));
        private Type type;

        public ArchivePackager(string input, string output)
        {
            this.input = input;
            this.output = output;

            if (!string.IsNullOrEmpty(Settings.Default.DefaultEncoding))
            {
                ArcSettings.Encoding = Encoding.GetEncoding(Encodings.CodePages[Settings.Default.DefaultEncoding]);
            }
            else
            {
                ArcSettings.Encoding = Encoding.UTF8;
                Settings.Default.DefaultEncoding = "UTF-8";
                Settings.Default.Save();
            }
        }

        public void Work(OperationMode mode)
        {
            switch (mode)
            {
                case OperationMode.Unpack:
                    UnpackOne();
                    break;
                case OperationMode.Pack:
                    Pack();
                    break;
                case OperationMode.None:
                    Logger.Error(LogStrings.ErrorNeedSelectOperation);
                    return;
            }
        }

        private void UnpackOne()
        {
            string[] selectedInfo = MainWindow.SelectedNodeUnpack.FullPath.Split('/');
            engineName = selectedInfo[0];
            extension = selectedInfo[1];

            if (extension.Contains("."))
            {
                if (!string.Equals(extension, Path.GetFileName(input), StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Error(string.Format(LogStrings.ErrorFileNameFailToMatch, extension));
                }
                extension = extension.Replace(".", string.Empty);
            }

            if (LoadType())
            {
                Logger.InitUnpack(input, output);
                ArchiveFormat instance = Activator.CreateInstance(type) as ArchiveFormat;
                instance.Unpack(input, output);
                Logger.FinishUnpack();
            }
            else
            {
                Logger.Error(LogStrings.ErrorClassNotFound);
            }
        }

        private void Pack()
        {
            string[] selectedInfo = MainWindow.SelectedNodePack.FullPath.Split('/');
            engineName = selectedInfo[0];
            extension = selectedInfo[1];

            if (LoadType())
            {
                Logger.InitPack(input, output);
                ArchiveFormat instance = Activator.CreateInstance(type) as ArchiveFormat;
                instance.Pack(input, output);
                Logger.FinishPack();
            }
            else
            {
                Logger.Error(LogStrings.ErrorClassNotFound);
            }
        }

        private bool LoadType()
        {
            type = assembly.GetType(typeName);
            return assembly != null && type != null;
        }

        public void Dispose()
        {
            _assembly = null;
            type = null;
        }
    }
}
