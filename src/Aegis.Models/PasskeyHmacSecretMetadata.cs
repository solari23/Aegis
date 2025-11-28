using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Aegis.Models.JsonConverters;

namespace Aegis.Models;

/// <summary>
/// Metadata for password secrets.
/// </summary>
public class PasskeyHmacSecretMetadata : SecretMetadata
{
    /// <inheritdoc/>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public override SecretKind SecretKind => SecretKind.PasskeyHmacSecret;

    /// <summary>
    /// The passkey's registered credential ID.
    /// </summary>
    [JsonConverter(typeof(ByteArrayBase64JsonConverter))]
    public byte[] CredentialId { get; set; }

    /// <inheritdoc />
    public override IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        if (this.CredentialId is null || this.CredentialId.Length == 0)
        {
            yield return new ValidationResult(
                $"Property {nameof(this.CredentialId)} is empty.");
        }

        foreach (var result in base.Validate(ctx))
        {
            yield return result;
        }
    }
}
