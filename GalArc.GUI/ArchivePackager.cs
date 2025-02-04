using ArcFormats;
using GalArc.Common;
using GalArc.GUI.Properties;
using GalArc.Logs;
using GalArc.Strings;
using System;
using System.Collections.Generic;
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

        private object[] param
        {
            get { return new List<object> { input, output }.ToArray(); }
        }

        private Assembly assembly
        {
            get
            {
                if (_assembly == null)
                {
                    _assembly = Assembly.Load(nameof(ArcFormats));
                }
                return _assembly;
            }
        }
        private Assembly _assembly;
        private Type type;
        private object instance;
        private MethodInfo method;

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
                instance = Activator.CreateInstance(type);
                method = type.GetMethod("Unpack", BindingFlags.Instance | BindingFlags.Public);
                if (method != null)
                {
                    Logger.InitUnpack(input, output);
                    method.Invoke(instance, param);
                    Logger.FinishUnpack();
                }
                else
                {
                    Logger.Error(LogStrings.ErrorUnpackMethodNotFound);
                }
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
                instance = Activator.CreateInstance(type);
                method = type.GetMethod("Pack", BindingFlags.Instance | BindingFlags.Public);
                if (method != null)
                {
                    Logger.InitPack(input, output);
                    method.Invoke(instance, param);
                    Logger.FinishPack();
                }
                else
                {
                    Logger.Error(LogStrings.ErrorPackMethodNotFound);
                }
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
            instance = null;
            method = null;
        }
    }
}
