using ArcFormats;
using GalArc.Common;
using GalArc.GUI.Properties;
using GalArc.Logs;
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
            get { return $"ArcFormats.{engineName}.{extension}"; }
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
                    _assembly = Assembly.Load("ArcFormats");
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

        public void UnpackOne()
        {
            string[] selectedInfo = MainWindow.selectedNodeUnpack.FullPath.Split('/');
            engineName = selectedInfo[0];
            extension = selectedInfo[1];

            if (extension.Contains("."))
            {
                if (!string.Equals(extension, Path.GetFileName(input), StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Error(string.Format(Resources.logFileNameFailToMatch, extension));
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
                    Logger.Error(Resources.logErrorUnpackMethodNotFound);
                }
            }
            else
            {
                Logger.Error(Resources.logErrorClassNotFound);
            }
        }

        public void Pack()
        {
            string[] selectedInfo = MainWindow.selectedNodePack.FullPath.Split('/');
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
                    Logger.Error(Resources.logErrorPackMethodNotFound);
                }
            }
            else
            {
                Logger.Error(Resources.logErrorClassNotFound);
            }
        }

        private bool LoadType()
        {
            type = assembly.GetType(typeName);
            return assembly != null && type != null;
        }

        private List<object> instances;

        private bool LoadTypes()
        {
            Type[] types = assembly.GetTypes();
            instances = new List<object>();
            if (assembly == null || types.Length == 0)
            {
                return false;
            }
            foreach (Type type in types)
            {
                if (type.IsSubclassOf(typeof(ArchiveFormat)))
                {
                    instances.Add(Activator.CreateInstance(type));
                }
            }
            return instances.Count > 0;
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
