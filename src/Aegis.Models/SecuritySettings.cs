using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Aegis.Models;

/// <summary>
/// Metadata that describes security settings (e.g. crypto algorithm choice) for the archive.
/// </summary>
public class SecuritySettings : IValidatableObject
{
    /// <summary>
    /// The algorithm used to encrypt data in the archive.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EncryptionAlgo EncryptionAlgo { get; set; }

    /// <summary>
    /// The key derivation function (KDF) used to generate user keys.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public KeyDerivationFunction KeyDerivationFunction { get; set; }

    /// <summary>
    /// The work factor parameter (e.g. iteration count) for the KDF.
    /// </summary>
    public int KeyDerivationWorkFactor { get; set; }

    /// <summary>
    /// The size (in bytes) of KeyIds generated for user keys.
    /// </summary>
    public int KeyIdSizeInBytes { get; set; }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if (this.EncryptionAlgo == EncryptionAlgo.Unknown)
        {
            yield return new ValidationResult(
                $"Property {nameof(this.EncryptionAlgo)} has invalid value '{this.EncryptionAlgo}'.");
        }

        if (this.KeyDerivationFunction == KeyDerivationFunction.Unknown)
        {
            yield return new ValidationResult(
                $"Property {nameof(this.KeyDerivationFunction)} has invalid value '{this.KeyDerivationFunction}'.");
        }

        if (this.KeyDerivationWorkFactor <= 0)
        {
            yield return new ValidationResult(
                $"Property {nameof(this.KeyDerivationWorkFactor)} must be greater than zero.");
        }

        if (this.KeyIdSizeInBytes <= 0)
        {
            yield return new ValidationResult(
                $"Property {nameof(this.KeyIdSizeInBytes)} must be greater than zero.");
        }
    }
}
