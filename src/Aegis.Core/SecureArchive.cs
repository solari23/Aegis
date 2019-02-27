namespace Aegis.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using Aegis.Core.Crypto;

    /// <summary>
    /// The core data structure that represents SecureArchive files. The SecureArchive is a
    /// self-contained file that holds the user's encrypted documents.
    /// </summary>
    /// <remarks>
    /// Data members for this class are defined in a Bond schema (see Aegis.bond).
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1047:DoNotDeclareProtectedMembersInSealedTypes", Justification = "The protected ctor is code generated.")]
    public sealed partial class SecureArchive
    {
        /// <summary>
        /// Creates a new <see cref="SecureArchive"/> that contains no files.
        /// </summary>
        /// <param name="creationParameters">The <see cref="SecureArchiveCreationParameters"/> to use when creating the archive.</param>
        /// <returns>A new <see cref="SecureArchive"/>.</returns>
        public static SecureArchive CreateNew(SecureArchiveCreationParameters creationParameters)
        {
            // TODO: Validate the input creation parameters

            var currentTime = DateTime.UtcNow;
            var archiveId = Guid.NewGuid();
            var keyDerivationSalt = CryptoHelpers.GetRandomBytes(creationParameters.KeyDerivationSaltSizeInBytes);

            // TODO: Generate the archive key
            // TOOD: Create the auth canary
            // TODO: Derive and authorize the first user key

            var archive = new SecureArchive
            {
                Id = archiveId,
                FileVersion = Constants.CurrentAegisSecureArchiveFileVersion,
                SecuritySettings = creationParameters.SecuritySettings,
                CreateTime = currentTime,
                LastModifiedTime = currentTime,
                KeyDerivationSalt = new List<byte>(keyDerivationSalt),
            };

            return archive;
        }
    }
}
