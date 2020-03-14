using Aegis.Models;

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
        /// <param name="userKeyFriendlyName">The friendly name for the first user key.</param>
        /// <param name="userSecret">The secret entered by the user to create the first user key.</param>
        public SecureArchiveCreationParameters(string userKeyFriendlyName, byte[] userSecret)
        {
            ArgCheck.NotEmpty(userKeyFriendlyName, nameof(userKeyFriendlyName));
            ArgCheck.NotEmpty(userSecret, nameof(userSecret));

            this.UserKeyFriendlyName = userKeyFriendlyName;
            this.UserSecret = userSecret;
        }

        /// <summary>
        /// Gets or sets the security settings for the new archive.
        /// </summary>
        public SecuritySettings SecuritySettings { get; set; } = SecuritySettingsFactory.Default;

        /// <summary>
        /// Gets or sets the size (in bytes) of the salt to generate and used in key derivations.
        /// </summary>
        public int KeyDerivationSaltSizeInBytes { get; set; } = AegisConstants.DefaultKeyDerivationSaltSizeInBytes;

        /// <summary>
        /// Gets or sets the friendly name for the first user key.
        /// </summary>
        public string UserKeyFriendlyName { get; set; }

        /// <summary>
        /// Gets the secret entered by the user to create the first user key.
        /// </summary>
        internal byte[] UserSecret { get; }
    }
}
