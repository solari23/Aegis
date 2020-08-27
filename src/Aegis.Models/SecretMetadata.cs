using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Aegis.Models
{
    /// <summary>
    /// Common base for metadata stored about secrets used to derive user keys.
    /// </summary>
    public abstract class SecretMetadata : IValidatableObject
    {
        /// <summary>
        /// Gets the kind of secret represented.
        /// </summary>
        public abstract SecretKind SecretKind { get; }

        /// <inheritdoc />
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext _)
        {
            if (this.SecretKind == SecretKind.Unknown)
            {
                yield return new ValidationResult(
                    $"Property {nameof(this.SecretKind)} has invalid value '{this.SecretKind}'.");
            }
        }
    }
}
