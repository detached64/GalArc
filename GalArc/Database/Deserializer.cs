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
        public static Dictionary<string, string> LoadedJsons = new Dictionary<string, string>();

        private static List<Type> Schemes => Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsClass && typeof(IScheme).IsAssignableFrom(t))
            .ToList();

        private static void LoadScheme(Type type)
        {
            if (!BaseSettings.Default.IsDatabaseEnabled)
            {
                return;
            }
            string name = type.Name.Remove(type.Name.Length - 6);    // remove "Scheme"
            if (LoadedJsons.ContainsKey(name))
            {
                return;
            }
            string path = Path.Combine(DatabaseConfig.Path, name + ".json");
            if (!File.Exists(path))
            {
                return;
            }
            string json = File.ReadAllText(path, Encoding.UTF8);
            if (!string.IsNullOrEmpty(json))
            {
                LoadedJsons[name] = json;
            }
        }

        private static void LoadSchemes()
        {
            LoadedJsons.Clear();
            foreach (Type type in Schemes)
            {
                LoadScheme(type);
            }
        }

        private static T Deserialize<T>()
        {
            string name = typeof(T).Name.Remove(typeof(T).Name.Length - 6);    // remove "Scheme"
            if (!BaseSettings.Default.IsDatabaseEnabled || !LoadedJsons.TryGetValue(name, out var json) || string.IsNullOrEmpty(json))
            {
                return default;
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to deserialize {name} scheme: {ex.Message}", false);
                return default;
            }
        }

        public static T ReadScheme<T>()
        {
            if (!BaseSettings.Default.IsDatabaseEnabled)
            {
                return default;
            }
            LoadScheme(typeof(T));
            return Deserialize<T>();
        }

        private static string GetInfo(Type type)
        {
            string name = type.Name.Remove(type.Name.Length - 6);    // remove "Scheme"
            string path = Path.Combine(DatabaseConfig.Path, name + ".json");

            StringBuilder result = new StringBuilder();
            // engine name
            result.AppendFormat(SchemeInfos.InfoEngineName, name).AppendLine();

            try
            {
                var json = LoadedJsons[name];
                JObject jsonObject = JObject.Parse(json);

                // version
                int version = (int)jsonObject["Version"];
                result.AppendFormat(SchemeInfos.InfoVersion, version).AppendLine();

                // contents
                result.AppendLine(SchemeInfos.InfoContents);

                // scheme count
                foreach (var property in jsonObject.Properties())
                {
                    if (property.Name == "Version")
                    {
                        continue;
                    }
                    int count = 0;
                    switch (property.Value.Type)
                    {
                        case JTokenType.Array:
                            count = property.Value.Count();
                            break;
                        case JTokenType.Object:
                            count = property.Value.Children().Count();
                            break;
                    }
                    result.Append("  ").AppendFormat(SchemeInfos.InfoItems, property.Name, count).AppendLine();
                }

                // file size
                FileInfo fileInfo = new FileInfo(path);
                result.AppendFormat(SchemeInfos.InfoSize, fileInfo.Length).AppendLine();

                // last modified time
                DateTime lastModified = File.GetLastWriteTime(path);
                result.AppendFormat(SchemeInfos.InfoLastModified, lastModified).AppendLine();

                jsonObject = null;
                return result.ToString();
            }
            catch
            {
                StringBuilder error = new StringBuilder();
                error.AppendFormat(SchemeInfos.InfoEngineName, name).AppendLine();
                error.Append(SchemeInfos.InfoFailedToReadInfos).AppendLine();
                return error.ToString();
            }
        }

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
            }
            return result.ToString();
        }
    }
}
