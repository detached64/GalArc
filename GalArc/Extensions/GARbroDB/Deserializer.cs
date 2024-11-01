using GalArc.Logs;
using GalArc.Strings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GalArc.Extensions.GARbroDB
{
    public class Deserializer
    {
        public static string GARbroDBContent { get; internal set; } = null;

        public static void LoadScheme()
        {
            if (GARbroDBConfig.IsGARbroDBEnabled)
            {
                if (!File.Exists(GARbroDBConfig.GARbroDBPath))
                {
                    return;
                }
                GARbroDBContent = File.ReadAllText(GARbroDBConfig.GARbroDBPath);
            }
        }

        public static Dictionary<string, Dictionary<string, Scheme>> Deserialize(Scheme instance)
        {
            if (!ExtensionsConfig.IsEnabled || !GARbroDBConfig.IsGARbroDBEnabled)
            {
                return null;
            }
            var result = new Dictionary<string, Dictionary<string, Scheme>>();

            Type scheme = instance.GetType();
            string jsonEngineName = scheme.GetField("JsonEngineName").GetValue(null).ToString();
            string jsonNodeName = scheme.GetField("JsonNodeName").GetValue(null).ToString();
            result.Add(jsonNodeName, new Dictionary<string, Scheme>());

            if (string.IsNullOrEmpty(GARbroDBContent))
            {
                return null;
            }
            try
            {
                JObject jsonObject = JObject.Parse(GARbroDBContent);
                var selectedToken = jsonObject.SelectToken($"['SchemeMap']['{jsonEngineName}']['{jsonNodeName}']");

                foreach (var token in selectedToken.Children<JProperty>())
                {
                    string schemeName = token.Name;
                    var schemeData = token.Value.ToObject(scheme);
                    result[jsonNodeName][schemeName] = (Scheme)schemeData;
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to deserialize {jsonEngineName} scheme: {ex.Message}", false);
                return null;
            }
        }

        public static string GetJsonInfo(string path)
        {
            if (!File.Exists(path))
            {
                return SchemeInfos.InfoFileNotFound;
            }
            try
            {
                StringBuilder result = new StringBuilder();

                GARbroDBContent = File.ReadAllText(path);
                JObject jsonObject = JObject.Parse(GARbroDBContent);

                // version
                int version = (int)jsonObject["Version"];
                result.AppendLine(string.Format(SchemeInfos.InfoVersion, version));

                // contents
                result.AppendLine(SchemeInfos.InfoContents);

                // scheme count
                JObject schemeMap = (JObject)jsonObject["SchemeMap"];
                int SchemeMapCount = schemeMap.Count;
                result.AppendLine(string.Format(SchemeInfos.InfoItems, "SchemeMap", SchemeMapCount));
                JObject gameMap = (JObject)jsonObject["GameMap"];
                int GameMapCount = gameMap.Count;
                result.AppendLine(string.Format(SchemeInfos.InfoItems, "GameMap", GameMapCount));

                // file size
                FileInfo fileInfo = new FileInfo(path);
                result.AppendLine(string.Format(SchemeInfos.InfoSize, fileInfo.Length));

                // last modified time
                DateTime lastModified = File.GetLastWriteTime(path);
                result.AppendLine(string.Format(SchemeInfos.InfoLastModified, lastModified));

                // hash
                result.AppendLine(string.Format(SchemeInfos.InfoHash, GARbroDBContent.GetHashCode()));

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
