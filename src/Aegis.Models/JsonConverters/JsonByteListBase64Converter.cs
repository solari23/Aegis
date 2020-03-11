using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aegis.Models.JsonConverters
{
    /// <summary>
    /// Helper that converts between a <see cref="List{byte}"/> and a base64 representation for JSON serialization.
    /// </summary>
    public class JsonByteListBase64Converter : JsonConverter<List<byte>>
    {
        public override List<byte> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => new List<byte>(reader.GetBytesFromBase64());

        public override void Write(Utf8JsonWriter writer, List<byte> value, JsonSerializerOptions options)
            => writer.WriteBase64StringValue(new ReadOnlySpan<byte>(value.ToArray()));
    }
}
