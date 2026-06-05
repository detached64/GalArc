using GalArc.Infrastructure.Logging;
using GalArc.Infrastructure.Settings;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace GalArc.Models.Database.Commons;

internal static class DatabaseManager
{
    public static readonly string DefaultPath = Path.Combine(Environment.CurrentDirectory, "Database");

    public static T LoadScheme<T>(JsonTypeInfo<T> typeInfo) where T : ArcScheme
    {
        try
        {
            string fileName = $"{typeof(T).Name[..^6]}.json";
            string filePath = Path.Combine(SettingsManager.Settings.DatabasePath, fileName);
            if (!File.Exists(filePath))
            {
                Logger.Error($"File not found: {filePath}. Try default path...");
                filePath = Path.Combine(DefaultPath, fileName);
            }
            T scheme = JsonSerializer.Deserialize(File.ReadAllText(filePath), typeInfo);
            if (scheme == null)
            {
                Logger.Error($"Failed to deserialize {filePath} into {typeof(T).Name}. Use default scheme.");
            }
            return scheme;
        }
        catch (Exception ex)
        {
            Logger.Error($"Error loading scheme {typeof(T).Name}: {ex.Message}");
            return default;
        }
    }

    public static void LoadList(string name, Action<string> action)
    {
        try
        {
            string fileName = $"{name}.lst";
            string filePath = Path.Combine(SettingsManager.Settings.DatabasePath, fileName);
            if (!File.Exists(filePath))
            {
                Logger.Error($"File not found: {filePath}. Try default path...");
                filePath = Path.Combine(DefaultPath, fileName);
            }
            using StreamReader sr = new(filePath);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                action(line);
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error loading list {name}: {ex.Message}");
        }
    }
}
