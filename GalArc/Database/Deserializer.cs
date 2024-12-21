using GalArc.Logs;
using GalArc.Strings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GalArc.Database
{
    public static class Deserializer
    {
        /// <summary>
        /// Loaded json strings. Dictionary&lt;engineName, jsonString&gt;.
        /// </summary>
        public static Dictionary<string, string> LoadedJsons = new Dictionary<string, string>();

        private static List<Type> _Schemes;

        /// <summary>
        /// Contains all schemes in the assembly.
        /// </summary>
        internal static List<Type> Schemes
        {
            get
            {
                if (_Schemes == null)
                {
                    _Schemes = new List<Type>();
                    _Schemes.AddRange(Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetCustomAttributes(typeof(DatabaseSchemeAttribute), false).Any()).ToList());
                }
                return _Schemes;
            }
        }

        /// <summary>
        /// Load string from a json file.
        /// </summary>
        /// <param name="type"></param>
        private static void LoadScheme(Type type)
        {
            if (!BaseSettings.Default.IsDatabaseEnabled)
            {
                return;
            }
            string name = type.Name.Remove(type.Name.Length - 6);    // remove "Scheme"
            string path = Path.Combine(DatabaseConfig.Path, name + ".json");
            if (!File.Exists(path))
            {
                return;
            }
            string json = File.ReadAllText(path, Encoding.UTF8);
            if (!string.IsNullOrEmpty(json))
            {
                LoadedJsons[name] = File.ReadAllText(path, Encoding.UTF8);
            }
        }

        /// <summary>
        /// Load all strings from json files.
        /// </summary>
        private static void LoadSchemes()
        {
            if (!BaseSettings.Default.IsDatabaseEnabled)
            {
                return;
            }
            foreach (Type type in Schemes)
            {
                LoadScheme(type);
            }
        }

        /// <summary>
        /// Deserialize json string to a dictionary.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Scheme Deserialize(Type type)
        {
            string name = type.Name.Remove(type.Name.Length - 6);
            if (!BaseSettings.Default.IsDatabaseEnabled)
            {
                return null;
            }
            if (!LoadedJsons.TryGetValue(name, out var json) || string.IsNullOrEmpty(json))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject(json, type) as Scheme;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to deserialize {name} scheme: {ex.Message}", false);
                return null;
            }
        }

        /// <summary>
        /// Read json string and deserialize it to Dictionary&lt;gameName, Scheme&gt;.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Scheme ReadScheme(Type type)
        {
            if (!BaseSettings.Default.IsDatabaseEnabled)
            {
                return null;
            }
            LoadScheme(type);
            return Deserialize(type);
        }

        /// <summary>
        /// Get specific json file information.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string GetInfo(Type type)
        {
            string name = type.Name.Remove(type.Name.Length - 6);    // remove "Scheme"
            string path = Path.Combine(DatabaseConfig.Path, name + ".json");

            try
            {
                StringBuilder result = new StringBuilder();

                var json = LoadedJsons[name];
                JObject jsonObject = JObject.Parse(json);
                // engine name
                result.AppendFormat(SchemeInfos.InfoEngineName, name).AppendLine();

                // version
                int version = (int)jsonObject["Version"];
                result.AppendFormat(SchemeInfos.InfoVersion, version).AppendLine();

                // contents
                result.AppendLine(SchemeInfos.InfoContents);

                // scheme count
                foreach (var jobject in jsonObject)
                {
                    if (jobject.Key == "Version")
                    {
                        continue;
                    }
                    int count = ((JObject)jobject.Value).Count;
                    result.AppendFormat(SchemeInfos.InfoItems, jobject.Key, count).AppendLine();
                }

                // file size
                FileInfo fileInfo = new FileInfo(path);
                result.AppendFormat(SchemeInfos.InfoSize, fileInfo.Length).AppendLine();

                // last modified time
                DateTime lastModified = File.GetLastWriteTime(path);
                result.AppendFormat(SchemeInfos.InfoLastModified, lastModified).AppendLine();

                // hash
                result.AppendFormat(SchemeInfos.InfoHash, json.GetHashCode()).AppendLine();

                jsonObject = null;
                return result.ToString();
            }
            catch (Exception)
            {
                return SchemeInfos.InfoFileNotFound;
            }
        }

        /// <summary>
        /// Get all json files information.
        /// </summary>
        /// <returns></returns>
        public static string GetInfos()
        {
            if (!BaseSettings.Default.IsDatabaseEnabled)
            {
                return SchemeInfos.InfoDisabled;
            }
            LoadSchemes();
            StringBuilder result = new StringBuilder();
            foreach (Type type in Schemes)
            {
                result.AppendLine(GetInfo(type));
                result.AppendLine();
            }
            return result.ToString();
        }
    }
}
