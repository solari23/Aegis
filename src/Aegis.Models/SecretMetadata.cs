namespace Aegis.Models
{
    /// <summary>
    /// Common base for metadata stored about secrets used to derive user keys.
    /// </summary>
    public abstract class SecretMetadata
    {
        /// <summary>
        /// Gets the kind of secret represented.
        /// </summary>
        public abstract SecretKind SecretKind { get; }
    }
}
