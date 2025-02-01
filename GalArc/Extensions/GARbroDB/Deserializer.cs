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
        private static string LoadedContent { get; set; }

        public static void LoadScheme()
        {
            if (!File.Exists(GARbroDBConfig.Path))
            {
                return;
            }
            LoadedContent = File.ReadAllText(GARbroDBConfig.Path);
        }

        public static T Deserialize<T>() where T : IGARbroScheme
        {
            if (!BaseSettings.Default.IsExtensionsEnabled || !BaseSettings.Default.IsGARbroDBEnabled)
            {
                return default;
            }
            if (string.IsNullOrEmpty(LoadedContent))
            {
                LoadScheme();
                if (string.IsNullOrEmpty(LoadedContent))
                {
                    return default;
                }
            }
            string jsonEngineName = typeof(T).GetField("JsonEngineName").GetValue(null).ToString();

            try
            {
                JObject jsonObject = JObject.Parse(LoadedContent);
                JToken selectedToken = jsonObject.SelectToken($"['SchemeMap']['{jsonEngineName}']");
                string selectedJson = selectedToken.ToString();
                return JsonConvert.DeserializeObject<T>(selectedJson);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to deserialize {jsonEngineName} scheme: {ex.Message}", false);
                return default;
            }
        }

        public static string GetInfo(string path)
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
                result.Append("  ").AppendFormat(SchemeInfos.InfoItems, "SchemeMap", SchemeMapCount).AppendLine();
                JObject gameMap = (JObject)jsonObject["GameMap"];
                int GameMapCount = gameMap.Count;
                result.Append("  ").AppendFormat(SchemeInfos.InfoItems, "GameMap", GameMapCount).AppendLine();

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
                return SchemeInfos.InfoFileNotFound;
            }
        }
    }
}
