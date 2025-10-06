using Avalonia.Styling;
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GalArc.Converters;

internal sealed class ThemeVariantJsonConverter : JsonConverter<ThemeVariant>
{
    public override ThemeVariant Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString() switch
        {
            "Light" => ThemeVariant.Light,
            "Dark" => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };
    }

    public override void Write(Utf8JsonWriter writer, ThemeVariant value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Key.ToString());
    }
}

internal sealed class CultureInfoJsonConverter : JsonConverter<CultureInfo>
{
    public override CultureInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new CultureInfo(reader.GetString() ?? "en-US");
    }

    public override void Write(Utf8JsonWriter writer, CultureInfo value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Name);
    }
}
