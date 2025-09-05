using System.Text.Json.Serialization;

namespace GalArc.Models.Database.Commons;

[JsonSerializable(typeof(AgsScheme))]
[JsonSerializable(typeof(NitroPlusScheme))]
[JsonSerializable(typeof(Ns2Scheme))]
[JsonSerializable(typeof(VinosScheme))]
[JsonSerializable(typeof(QlieScheme))]
[JsonSerializable(typeof(SeraphScheme))]
[JsonSerializable(typeof(SiglusScheme))]
internal partial class DatabaseSerializationContext : JsonSerializerContext;
