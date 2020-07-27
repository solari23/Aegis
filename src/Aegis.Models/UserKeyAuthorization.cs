using System;

namespace Aegis.Models
{
    /// <summary>
    /// Metadata associated with user keys authorized to unlock the SecureArchive.
    /// </summary>
    /// <remarks>
    /// User keys are derived from secrets input by the user at runtime to unlock the archive.
    /// The keys and their associated keyId are derived using key derivation functions.
    /// The algorithm is:
    ///   let N be the number of bytes for the key
    ///   let M be the number of bytes for the keyId
    ///   keyMatter := Derive N + M bytes from the secret using the KDF
    ///   key := keyMatter[0..N-1]
    ///   keyId := base64url(keyMatter[N..])
    /// </remarks>
    public class UserKeyAuthorization
    {
        /// <summary>
        /// A friendly name to help the user identify the key.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// An identifier derived along with the key. It is the base64url encoding of some extra
        /// bytes produced by the key derivation function while deriving the user key.
        /// </summary>
        /// <remarks>
        /// Matching on this value should *never* be used to positively authenticate a user.
        /// </remarks>
        public string KeyId { get; set; }

        /// <summary>
        /// Gets or sets metadata about the secret used to derive the user key.
        /// </summary>
        public SecretMetadata SecretMetadata { get; set; }

        /// <summary>
        /// Timestamp of when the key was authorized.
        /// </summary>
        public DateTimeOffset TimeAdded { get; set; }

        /// <summary>
        /// The archive key, encrypted by the authorized key.
        /// </summary>
        /// <remarks>
        /// Authenticated cyphers must also check integrity of Utf8Bytes(string.Concat(FriendlyName, KeyId))
        /// </remarks>
        public EncryptedPacket EncryptedArchiveKey { get; set; }
    }
}
