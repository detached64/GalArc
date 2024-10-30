using ArcFormats.Properties;
using GalArc.Logs;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ArcFormats
{
    public class Worker
    {
        private static string FilePath = null;
        private static string FolderPath = null;
        private static string typeString = null;

        public Worker(string filePath, string folderPath, string typeStr)
        {
            FilePath = filePath;
            FolderPath = folderPath;
            typeString = typeStr;
        }

        public Worker(string filePath, string folderPath) : this(filePath, folderPath, null) { }

        public void UnpackOne()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type type = assembly.GetType(typeString);

            if (assembly != null && type != null)
            {
                object instance = Activator.CreateInstance(type);
                MethodInfo unpack = type.GetMethod("Unpack", BindingFlags.Instance | BindingFlags.Public);
                if (unpack != null)
                {
                    object[] parameters = new List<object>
                    {
                        FilePath,
                        FolderPath
                    }.ToArray();
                    Logger.InitUnpack(FilePath, FolderPath);
                    unpack.Invoke(instance, parameters);
                    Logger.FinishUnpack();
                }
                else
                {
                    Logger.Error(Resources.logErrorUnpackMethodNotFound);
                }
            }
            else
            {
                throw new Exception(Resources.logErrorClassNotFound);
            }
        }

        public void Pack()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type type = assembly.GetType(typeString);
            if (assembly != null && type != null)
            {
                object instance = Activator.CreateInstance(type);
                MethodInfo pack = type.GetMethod("Pack", BindingFlags.Instance | BindingFlags.Public);
                if (pack != null)
                {
                    object[] parameters = new List<object>
                    {
                        FolderPath,
                        FilePath
                    }.ToArray();
                    Logger.InitPack(FolderPath, FilePath);
                    pack.Invoke(instance, parameters);
                    Logger.FinishPack();
                }
                else
                {
                    Logger.Error(Resources.logErrorPackMethodNotFound);
                }
            }
            else
            {
                throw new Exception(Resources.logErrorClassNotFound);
            }
        }

    }
}
