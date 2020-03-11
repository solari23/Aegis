using System.Collections.Generic;
using System.Linq;

using Aegis.Core.Crypto;
using Aegis.Models;

namespace Aegis.Core
{
    /// <summary>
    /// Data structure that represents the index of files in the secure archive.
    /// </summary>
    internal sealed class FileIndex
    {
        /// <summary>
        /// Decrypts the <see cref="FileIndex"/> data structure contained in the given <see cref="EncryptedPacket"/>.
        /// </summary>
        /// <param name="encryptedPacket">The encrypted data to decrypt the <see cref="FileIndex"/> from.</param>
        /// <param name="archiveKey">The key used for decryption.</param>
        /// <param name="securitySettings">The <see cref="SecuritySettings"/> for the secure archive.</param>
        /// <returns>The decrypted <see cref="FileIndex"/>.</returns>
        public static FileIndex DecryptFrom(
            EncryptedPacket encryptedPacket,
            ArchiveKey archiveKey,
            SecuritySettings securitySettings)
        {
            var indexEntries = JsonHelpers.DecryptAndDeserialize<List<FileIndexEntry>>(
                encryptedPacket,
                archiveKey,
                securitySettings);

            return new FileIndex(indexEntries);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileIndex"/> class.
        /// </summary>
        public FileIndex() : this(new List<FileIndexEntry>())
        {
            // Empty.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileIndex"/> class.
        /// </summary>
        /// <param name="entries">The list of entries in the index.</param>
        private FileIndex(IEnumerable<FileIndexEntry> entries)
        {
            // TODO: Build more complex index data structures based on the entries.
            this.Entries = entries.ToList();
        }

        /// <summary>
        /// Gets the list of entries in the index.
        /// </summary>
        private List<FileIndexEntry> Entries { get; }

        /// <summary>
        /// Encrypts the <see cref="FileIndex"/> for secure storage.
        /// </summary>
        /// <param name="archiveKey">The encryption key to use.</param>
        /// <param name="securitySettings">The <see cref="SecuritySettings"/> for the secure archive.</param>
        /// <returns>The encrypted <see cref="FileIndex"/> data.</returns>
        public EncryptedPacket Encrypt(ArchiveKey archiveKey, SecuritySettings securitySettings)
            => JsonHelpers.SerializeAndEncrypt(this.Entries, archiveKey, securitySettings);
    }
}
