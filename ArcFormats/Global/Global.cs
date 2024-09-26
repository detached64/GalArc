﻿using Log;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Utility;

namespace ArcFormats
{
    public class Global
    {
        private static string FilePath = null;
        private static string FolderPath = null;
        private static string typeString = null;

        public static Encoding Encoding = ArcEncoding.Shift_JIS;
        public static bool ToDecryptScript = false;

        public static string Version = null;

        public Global(string filePath, string folderPath, string typeStr)
        {
            FilePath = filePath;
            FolderPath = folderPath;
            typeString = typeStr;
        }

        public void Unpack()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type type = assembly.GetType(typeString);

            if (assembly != null && type != null)
            {
                object instance = Activator.CreateInstance(type);
                MethodInfo unpack = type.GetMethod("Unpack", BindingFlags.Instance | BindingFlags.Public);
                object[] parameters = new List<object>
                {
                    FilePath,
                    FolderPath
                }.ToArray();
                LogUtility.InitUnpack(FilePath, FolderPath);
                unpack.Invoke(instance, parameters);
                LogUtility.FinishUnpack();
            }
            else
            {
                throw new Exception("Error:Specified format not found.");
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
                object[] parameters = new List<object>
                {
                    FolderPath,
                    FilePath
                }.ToArray();
                LogUtility.InitPack(FolderPath, FilePath);
                pack.Invoke(instance, parameters);
                LogUtility.FinishPack();
            }
            else
            {
                throw new Exception("Error:Specified format not found.");
            }
        }
    }
}