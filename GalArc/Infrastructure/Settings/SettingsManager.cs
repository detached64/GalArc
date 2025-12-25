using GalArc.Infrastructure.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace GalArc.Infrastructure.Settings;

internal static class SettingsManager
{
    public static readonly AppSettings Settings;

    private static readonly string DefaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetExecutingAssembly().GetName().Name, "settings.json");

    static SettingsManager()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(DefaultPath));
        Settings = LoadSettings();
    }

    private static readonly JsonSerializerOptions _options = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        WriteIndented = true
    };

    private static readonly AppSettingsSerializationContext context = new(_options);

    private static AppSettings LoadSettings()
    {
        if (!File.Exists(DefaultPath))
        {
            return new AppSettings();
        }

        try
        {
            string json = File.ReadAllText(DefaultPath);
            return JsonSerializer.Deserialize(json, context.AppSettings) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static async Task SaveSettingsAsync()
    {
        try
        {
            string json = JsonSerializer.Serialize(Settings, context.AppSettings);
            await File.WriteAllTextAsync(DefaultPath, json);
        }
        catch (Exception ex)
        {
            Logger.Error($"Error saving settings: {ex.Message}");
        }
    }
}
