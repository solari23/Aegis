using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aegis.Models.JsonConverters
{
    /// <summary>
    /// Helper that provides polymorphic JSON serialization for <see cref="SecretMetadata"/> classes.
    /// </summary>
    public class SecretMetadataPolymorphicConverter : JsonConverter<SecretMetadata>
    {
        /// <summary>
        /// Serialized property that contains the polymorphic type discriminator.
        /// </summary>
        private const string TypeDiscriminatorPropertyName = "$Type";

        /// <summary>
        /// Serialized property that contains the polymorphic object value.
        /// </summary>
        private const string TypeValuePropertyName = "$Value";

        /// <summary>
        /// The set of type discriminators mapped to their types.
        /// </summary>
        internal static ReadOnlyDictionary<SecretKind, Type> TypeDiscriminatorMappings { get; }
            = new ReadOnlyDictionary<SecretKind, Type>(new Dictionary<SecretKind, Type>
            {
                { SecretKind.Password, typeof(PasswordSecretMetadata) }
            });

        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert) => typeof(SecretMetadata).IsAssignableFrom(typeToConvert);

        /// <inheritdoc />
        public override SecretMetadata Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            // First, deserialize the type discriminator.
            if (!reader.Read()
                || reader.TokenType != JsonTokenType.PropertyName
                || !reader.GetString().Equals(TypeDiscriminatorPropertyName, StringComparison.OrdinalIgnoreCase))
            {
                throw new JsonException("Expected first property to be type discriminator.");
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException();
            }

            var secretKindDiscriminator = (SecretKind)reader.GetInt32();

            if (!TypeDiscriminatorMappings.TryGetValue(secretKindDiscriminator, out var serializationType))
            {
                throw new NotSupportedException(
                    $"JSON serialization for metadata for secret kind '{secretKindDiscriminator}' is not supported.");
            }

            // Now deserialize the actual value.
            if (!reader.Read()
                || reader.TokenType != JsonTokenType.PropertyName
                || !reader.GetString().Equals(TypeValuePropertyName, StringComparison.OrdinalIgnoreCase))
            {
                throw new JsonException("Expected second property to be serialized value.");
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var metadata = (SecretMetadata)JsonSerializer.Deserialize(ref reader, serializationType);

            if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException();
            }

            return metadata;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, SecretMetadata value, JsonSerializerOptions options)
        {
            if (!TypeDiscriminatorMappings.TryGetValue(value.SecretKind, out var serializationType))
            {
                throw new NotSupportedException(
                    $"JSON serialization for metadata for secret kind '{value.SecretKind}' is not supported.");
            }

            if (value.GetType() != serializationType)
            {
                throw new InvalidOperationException(
                    $"Metadata for secret kind '{value.SecretKind}' is of unexpected type. Expected: {serializationType.FullName}, got: {value.GetType().FullName}.");
            }

            writer.WriteStartObject();

            // Write the discriminator that allows us to differentiate what type was serialized.
            writer.WriteNumber(TypeDiscriminatorPropertyName, (int)value.SecretKind);

            // Write the actual object value.
            writer.WritePropertyName(TypeValuePropertyName);
            JsonSerializer.Serialize(writer, value, serializationType);

            writer.WriteEndObject();
        }
    }
}
