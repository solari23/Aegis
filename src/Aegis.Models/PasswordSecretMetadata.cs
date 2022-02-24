using System.Text.Json.Serialization;

namespace Aegis.Models;

/// <summary>
/// Metadata for password secrets.
/// </summary>
public class PasswordSecretMetadata : SecretMetadata
{
    /// <inheritdoc/>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public override SecretKind SecretKind => SecretKind.Password;
}
