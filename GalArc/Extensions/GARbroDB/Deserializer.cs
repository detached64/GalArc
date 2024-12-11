using GalArc.Logs;
using GalArc.Strings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;

namespace GalArc.Extensions.GARbroDB
{
    public static class Deserializer
    {
        private static string LoadedContent { get; set; } = null;

        public static void LoadScheme()
        {
            if (!File.Exists(GARbroDBConfig.Path))
            {
                return;
            }
            LoadedContent = File.ReadAllText(GARbroDBConfig.Path);
        }

        /// <summary>
        /// Deserialize the specified scheme.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="jsonEngineName"></param>
        /// <returns></returns>
        public static Scheme Deserialize(Type type, string jsonEngineName)
        {
            if (!ExtensionsConfig.IsEnabled || !GARbroDBConfig.IsEnabled)
            {
                return null;
            }
            if (string.IsNullOrEmpty(LoadedContent))
            {
                LoadScheme();
                if (string.IsNullOrEmpty(LoadedContent))
                {
                    return null;
                }
            }

            try
            {
                JObject jsonObject = JObject.Parse(LoadedContent);
                JToken selectedToken = jsonObject.SelectToken($"['SchemeMap']['{jsonEngineName}']");
                string selectedJson = selectedToken.ToString();
                return JsonConvert.DeserializeObject(selectedJson, type) as Scheme;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to deserialize {jsonEngineName} scheme: {ex.Message}", false);
                return null;
            }
        }

        /// <summary>
        /// Get the GARbroDB information.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetJsonInfo(string path)
        {
            if (!File.Exists(path))
            {
                return SchemeInfos.InfoFileNotFound;
            }
            try
            {
                StringBuilder result = new StringBuilder();

                LoadedContent = File.ReadAllText(path);
                JObject jsonObject = JObject.Parse(LoadedContent);

                // version
                int version = (int)jsonObject["Version"];
                result.AppendFormat(SchemeInfos.InfoVersion, version).AppendLine();

                // contents
                result.AppendLine(SchemeInfos.InfoContents);

                // scheme count
                JObject schemeMap = (JObject)jsonObject["SchemeMap"];
                int SchemeMapCount = schemeMap.Count;
                result.AppendFormat(SchemeInfos.InfoItems, "SchemeMap", SchemeMapCount).AppendLine();
                JObject gameMap = (JObject)jsonObject["GameMap"];
                int GameMapCount = gameMap.Count;
                result.AppendFormat(SchemeInfos.InfoItems, "GameMap", GameMapCount).AppendLine();

                // file size
                FileInfo fileInfo = new FileInfo(path);
                result.AppendFormat(SchemeInfos.InfoSize, fileInfo.Length).AppendLine();

                // last modified time
                DateTime lastModified = File.GetLastWriteTime(path);
                result.AppendFormat(SchemeInfos.InfoLastModified, lastModified).AppendLine();

                // hash
                result.AppendFormat(SchemeInfos.InfoHash, LoadedContent.GetHashCode()).AppendLine();

                jsonObject = null;
                return result.ToString();
            }
            catch
            {
                return SchemeInfos.InfoFileNotFound;
            }
        }
    }
}
