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

        private static List<Type> schemes;

        private static List<Type> Schemes => schemes ?? (schemes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ArcScheme).IsAssignableFrom(t))
            .OrderBy(t => t.Name)
            .ToList());

        public static int SchemeCount => Schemes.Count;

        public static int ImportedSchemeCount { get; set; }

        private static bool LoadScheme<T>() where T : ArcScheme
        {
            string class_name = typeof(T).Name;
            string name = class_name.Remove(class_name.Length - 6);    // remove "Scheme"
            if (LoadedJsons.ContainsKey(name))
            {
                return true;
            }
            string path = Path.Combine(DatabaseConfig.Path, name + ".json");
            if (!File.Exists(path))
            {
                return false;
            }
            string json = File.ReadAllText(path, Encoding.UTF8);
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }
            LoadedJsons[name] = json;
            return true;
        }

        private static T DeserializeScheme<T>()
        {
            string name = typeof(T).Name.Remove(typeof(T).Name.Length - 6);    // remove "Scheme"
            if (!LoadedJsons.TryGetValue(name, out var json) || string.IsNullOrEmpty(json))
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

        public static T ReadScheme<T>() where T : ArcScheme
        {
            if (!BaseSettings.Default.IsDatabaseEnabled || !LoadScheme<T>())
            {
                return default;
            }
            return DeserializeScheme<T>();
        }

        private static string GetInfo(Type type, out bool isValid)
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

                // contents
                result.AppendLine(SchemeInfos.InfoContents);
                foreach (var property in jsonObject.Properties())
                {
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
                isValid = true;
                return result.ToString();
            }
            catch
            {
                StringBuilder error = new StringBuilder();
                error.AppendFormat(SchemeInfos.InfoEngineName, name).AppendLine();
                error.AppendLine(SchemeInfos.InfoFailedToReadInfos);
                isValid = false;
                return error.ToString();
            }
        }

        public static string GetInfos()
        {
            if (!BaseSettings.Default.IsDatabaseEnabled)
            {
                return SchemeInfos.InfoDisabled;
            }
            foreach (Type type in Schemes)
            {
                string name = type.Name.Remove(type.Name.Length - 6);    // remove "Scheme"
                if (LoadedJsons.ContainsKey(name))
                {
                    continue;
                }
                string path = Path.Combine(DatabaseConfig.Path, name + ".json");
                if (!File.Exists(path))
                {
                    continue;
                }
                string json = File.ReadAllText(path, Encoding.UTF8);
                if (!string.IsNullOrEmpty(json))
                {
                    LoadedJsons[name] = json;
                }
            }
            StringBuilder result = new StringBuilder();
            int count = 0;
            StringBuilder info = new StringBuilder();
            foreach (Type type in Schemes)
            {
                info.AppendLine(GetInfo(type, out bool flag));
                if (flag)
                {
                    count++;
                }
            }
            result.AppendFormat(LogStrings.SchemeCount, SchemeCount).AppendLine();
            result.AppendFormat(LogStrings.ImportedSchemeCount, ImportedSchemeCount).AppendLine();
            result.AppendFormat(LogStrings.ValidDatabaseCount, count).AppendLine().AppendLine();
            result.AppendLine(info.ToString());
            return result.ToString();
        }
    }
}
