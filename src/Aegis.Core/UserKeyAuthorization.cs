namespace Aegis.Core
{
    using System;

    /// <summary>
    /// Metadata associated with user keys authorized to unlock the SecureArchive.
    /// </summary>
    /// <remarks>
    /// Data members for this class are defined in a Bond schema (see Aegis.bond).
    /// </remarks>
    public partial class UserKeyAuthorization
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserKeyAuthorization"/> class.
        /// </summary>
        /// <param name="friendlyName">A friendly name to help the user identify the key.</param>
        /// <param name="keyId">An identifier derived along with the key.</param>
        /// <param name="encryptedArchiveKey">The archive key, encrypted by the authorized key.</param>
        internal UserKeyAuthorization(
            string friendlyName,
            string keyId,
            EncryptedPacket encryptedArchiveKey)
        {
            ArgCheck.NotEmpty(friendlyName, nameof(friendlyName));
            ArgCheck.NotEmpty(keyId, nameof(keyId));
            ArgCheck.NotNull(encryptedArchiveKey, nameof(encryptedArchiveKey));

            this.FriendlyName = friendlyName;
            this.KeyId = keyId;
            this.TimeAdded = DateTime.UtcNow;
            this.EncryptedArchiveKey = encryptedArchiveKey;
        }
    }
}
