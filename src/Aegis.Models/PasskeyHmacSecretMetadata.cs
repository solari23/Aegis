using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Aegis.Models;

/// <summary>
/// Metadata for password secrets.
/// </summary>
public class PasskeyHmacSecretMetadata : SecretMetadata
{
    /// <inheritdoc/>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public override SecretKind SecretKind => SecretKind.PasskeyHmacSecret;

    // TODO: properties

    /// <inheritdoc />
    public override IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        // TODO: validation for properties

        foreach (var result in base.Validate(ctx))
        {
            yield return result;
        }
    }
}
