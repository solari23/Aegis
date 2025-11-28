using System.ComponentModel.DataAnnotations;

using Aegis.Core.CredentialsInterface;
using Aegis.Models;

namespace Aegis.Core;

/// <summary>
/// Encapsulates the parameters required to create a new <see cref="SecureArchive"/>.
/// </summary>
public sealed class SecureArchiveCreationParameters : IDisposable, IValidatableObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecureArchiveCreationParameters"/> class.
    /// </summary>
    /// <param name="firstKeyAuthorizationParam">The parameters to authorize the first user key.</param>
    /// <param name="archiveId">The unique identifier for the new archive (optional; if not provided, a random new Guid is generated).</param>
    public SecureArchiveCreationParameters(UserKeyAuthorizationParameters firstKeyAuthorizationParam, Guid? archiveId = null)
    {
        ArgCheck.IsValid(firstKeyAuthorizationParam);

        this.FirstKeyAuthorizationParams = firstKeyAuthorizationParam;
        this.ArchiveId = archiveId ?? Guid.NewGuid();
    }

    /// <summary>
    /// Gets the unique identifier for the new archive.
    /// </summary>
    public Guid ArchiveId { get; }

    /// <summary>
    /// Gets or sets the security settings for the new archive.
    /// </summary>
    public SecuritySettings SecuritySettings { get; set; } = SecuritySettingsFactory.Default;

    /// <summary>
    /// Gets or sets the size (in bytes) of the salt to generate and used in key derivations.
    /// </summary>
    public int KeyDerivationSaltSizeInBytes { get; set; } = AegisConstants.DefaultKeyDerivationSaltSizeInBytes;

    /// <summary>
    /// Gets the parameters to authorize the first user key
    /// </summary>
    internal UserKeyAuthorizationParameters FirstKeyAuthorizationParams { get; }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if (this.KeyDerivationSaltSizeInBytes <= 0)
        {
            yield return new ValidationResult(
                $"Property {nameof(this.KeyDerivationSaltSizeInBytes)} must be greater than zero.");
        }

        var validationResults = new List<ValidationResult>();

        if (!Validator.TryValidateProperty(
            this.SecuritySettings,
            new ValidationContext(this) { MemberName = nameof(this.SecuritySettings) },
            validationResults))
        {
            foreach (var result in validationResults)
            {
                yield return new ValidationResult(
                    $"Property {nameof(this.SecuritySettings)} is not valid. Issue: {result.ErrorMessage}");
            }

            validationResults.Clear();
        }
    }

    #region IDisposable Support

    /// <summary>
    /// Flag to detect redundant calls to dispose the object.
    /// </summary>
    private bool isDisposed = false;

    /// <summary>
    /// Disposes the current object when it is no longer required.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the current object when it is no longer required.
    /// </summary>
    /// <param name="disposing">Whether or not the operation is coming from Dispose() (as opposed to a finalizer).</param>
    protected void Dispose(bool disposing)
    {
        if (!this.isDisposed)
        {
            if (disposing)
            {
                this.FirstKeyAuthorizationParams.Dispose();
            }

            this.isDisposed = true;
        }
    }

    #endregion
}
