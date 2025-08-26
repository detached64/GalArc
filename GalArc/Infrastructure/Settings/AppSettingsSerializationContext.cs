using System.Text.Json.Serialization;

namespace GalArc.Infrastructure.Settings;

[JsonSerializable(typeof(AppSettings))]
internal partial class AppSettingsSerializationContext : JsonSerializerContext;
