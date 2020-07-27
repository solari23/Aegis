namespace Aegis.Models
{
    /// <summary>
    /// Metadata for password secrets.
    /// </summary>
    public class PasswordSecretMetadata : SecretMetadata
    {
        /// <inheritdoc/>
        public override SecretKind SecretKind => SecretKind.Password;
    }
}
