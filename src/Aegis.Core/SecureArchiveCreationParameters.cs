namespace Aegis.Core
{
    /// <summary>
    /// Encapsulates the parameters required to create a new <see cref="SecureArchive"/>.
    /// </summary>
    public class SecureArchiveCreationParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecureArchiveCreationParameters"/> class.
        /// </summary>
        /// <param name="userSecret">The secret entered by the user to encrypt the archive data.</param>
        public SecureArchiveCreationParameters(byte[] userSecret)
        {
            ArgCheck.NotEmpty(userSecret, nameof(userSecret));

            this.UserSecret = userSecret;
        }

        /// <summary>
        /// Gets or sets the security settings for the new archive.
        /// </summary>
        public SecuritySettings SecuritySettings { get; set; } = SecuritySettings.Default;

        /// <summary>
        /// Gets or sets the size (in bytes) of the salt to generate and used in key derivations.
        /// </summary>
        public int KeyDerivationSaltSizeInBytes { get; set; } = Constants.DefaultKeyDerivationSaltSizeInBytes;

        /// <summary>
        /// Gets the secret entered by the user to encrypt the archive data.
        /// </summary>
        internal byte[] UserSecret { get; }
    }
}
