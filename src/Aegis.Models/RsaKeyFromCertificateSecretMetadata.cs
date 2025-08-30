using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Aegis.Models;

/// <summary>
/// Metadata for secrets derived from certificate RSA keys.
/// </summary>
public class RsaKeyFromCertificateSecretMetadata : SecretMetadata
{
    /// <inheritdoc/>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public override SecretKind SecretKind => SecretKind.RsaKeyFromCertificate;

    /// <summary>
    /// The thumbprint of the certificate.
    /// </summary>
    public string Thumbprint { get; set; }

    /// <inheritdoc />
    public override IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        if (string.IsNullOrWhiteSpace(this.Thumbprint))
        {
            yield return new ValidationResult(
                $"Property {nameof(this.Thumbprint)} is empty.");
        }

        foreach (var result in base.Validate(ctx))
        {
            yield return result;
        }
    }
}
