using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aegis.Models.JsonConverters;

/// <summary>
/// Helper that converts between a <see cref="byte[]"/> and a base64 representation for JSON serialization.
/// </summary>
public class ByteArrayBase64JsonConverter : JsonConverter<byte[]>
{
    public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetBytesFromBase64();

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        => writer.WriteBase64StringValue(value.ToArray());
}
