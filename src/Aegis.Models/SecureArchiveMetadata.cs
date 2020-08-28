using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Aegis.Models.JsonConverters;

namespace Aegis.Models
{
    /// <summary>
    /// The core data structure that maintains metadata about the archive, including user key authorizations
    /// and an index of the encrypted documents stored in the archive.
    /// </summary>
    public class SecureArchiveMetadata
    {
        /// <summary>
        /// A unique identifier for the archive.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The security settings for the archive.
        /// </summary>
        public SecuritySettings SecuritySettings { get; set; }

        /// <summary>
        /// When the archive was created.
        /// </summary>
        public DateTimeOffset CreateTime { get; set; }

        /// <summary>
        /// When the archive was last modified.
        /// </summary>
        public DateTimeOffset LastModifiedTime { get; set; }

        /// <summary>
        /// The salt used when deriving keys.
        /// </summary>
        /// <remarks>
        /// It is by-design that there is a single global key derivation salt for the
        /// archive rather than having a different salt per individual key.
        ///
        /// When the user provides their secret, we don't necessarily know which key it
        /// corresponds to. If we had a salt per key then we'd potentially need to compute
        /// the KDF for all of the authorized keys until we found the right one. This could
        /// take a long time for an archive with several keys and an expensive KDF. With a
        /// global salt, we just need to run the KDF once.
        ///
        /// The downside is that, within the scope of one archive, two identical secrets
        /// (e.g. two passwords that happen to be the same) will have the same keyId.
        /// Someone inspecting the archive metadata would see that they are the same.
        ///
        /// This tradeoff is considered acceptable given that it's not expected for a
        /// single archive to be in the custody of more than a few people. With stronger
        /// secret types (e.g. cryptographic keys), this won't be a problem at all.
        /// </remarks>
        [JsonConverter(typeof(JsonByteListBase64Converter))]
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Required for serialization framework.")]
        public List<byte> KeyDerivationSalt { get; set; }

        /// <summary>
        /// The archive ID encrypted with the archive key. This field is used to
        /// test if the presented user key is authorized to unlock the archive.
        /// </summary>
        public EncryptedPacket AuthCanary { get; set; }

        /// <summary>
        /// The list of keys that are authorized to unlock the archive.
        /// </summary>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Required for serialization framework.")]
        public List<UserKeyAuthorization> UserKeyAuthorizations { get; set; }

        /// <summary>
        /// The encrypted file index.
        /// </summary>
        /// <remarks>
        /// The encrypted data is JSON formatted list of <see cref="FileIndexEntry"/> objects.
        /// </remarks>
        public EncryptedPacket EncryptedFileIndex { get; set; }
    }
}
