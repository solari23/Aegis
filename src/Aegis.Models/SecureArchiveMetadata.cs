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
