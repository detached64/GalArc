using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Utility;

namespace ArcFormats
{
    public abstract class ArchiveFormat
    {
        public abstract void Unpack(string filePath, string folderPath);

        public virtual void Pack(string folderPath, string filePath)
        {
            throw new NotImplementedException();
        }
    }

    public class Entry
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 绝对路径
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// 绝对偏移
        /// </summary>
        public uint Offset { get; set; }
        /// <summary>
        /// 文件大小，等同于PackedSize
        /// </summary>
        public uint Size { get; set; }
    }

    public class PackedEntry : Entry
    {
        public bool IsPacked { get; set; }
        public uint UnpackedSize { get; set; }
    }

    public class ArcSettings
    {
        public static Encoding Encoding = ArcEncoding.Shift_JIS;

        public static string Version = null;

        public static object[] Instances
        {
            get
            {
                if (instances == null)
                {
                    GetTypes();
                }
                return instances.ToArray();
            }
        }

        private static List<object> instances;

        public static void GetTypes()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] types = assembly.GetTypes();
            instances = new List<object>();
            foreach (Type type in types)
            {
                if (type.IsSubclassOf(typeof(ArchiveFormat)))
                {
                    instances.Add(Activator.CreateInstance(type));
                }
            }
        }
    }
}
