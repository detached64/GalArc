using Avalonia.Styling;
using GalArc.Converters;
using GalArc.Enums;
using GalArc.Infrastructure.Cultures;
using GalArc.Infrastructure.Logging;
using GalArc.Models.Database.Commons;
using GalArc.Models.Formats.Commons;
using System.Globalization;
using System.Text.Json.Serialization;

namespace GalArc.Infrastructure.Settings;

internal sealed class AppSettings
{
    [JsonConverter(typeof(ThemeVariantConverter))]
    public ThemeVariant AppTheme { get; set; } = ThemeVariant.Default;
    [JsonConverter(typeof(CultureInfoConverter))]
    public CultureInfo AppLanguage { get; set; } = CultureManager.InitCulture(CultureInfo.CurrentCulture);
    public string InputPath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter<OperationType>))]
    public OperationType Operation { get; set; } = OperationType.Unpack;
    public int UnpackFormatIndex { get; set; }
    public int PackFormatIndex { get; set; }
    [JsonIgnore]
    public ArcFormat UnpackFormat { get; set; }
    [JsonIgnore]
    public ArcFormat PackFormat { get; set; }
    public bool ContinueOnError { get; set; }
    public bool MatchPaths { get; set; } = true;
    public bool SaveLogs { get; set; } = true;
    public string LogFilePath { get; set; } = Logger.DefaultPath;
    public string DatabasePath { get; set; } = DatabaseManager.DefaultPath;
}
