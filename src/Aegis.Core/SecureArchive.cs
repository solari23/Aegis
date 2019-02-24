namespace Aegis.Core
{
    using System;
    using System.Diagnostics.CodeAnalysis;

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
        /// <param name="encryptionAlgo">The data encryption algorithm to use. Default is <see cref="Constants.DefaultEncryptionAlgorithm"/>.</param>
        /// <returns>A new <see cref="SecureArchive"/>.</returns>
        public static SecureArchive CreateNew(EncryptionAlgo encryptionAlgo = Constants.DefaultEncryptionAlgorithm)
        {
            ArgCheck.IsNot(EncryptionAlgo.Unknown, encryptionAlgo, nameof(encryptionAlgo));

            var currentTime = DateTime.UtcNow;

            var archive = new SecureArchive
            {
                Id = Guid.NewGuid(),
                FileVersion = Constants.CurrentAegisSecureArchiveFileVersion,
                EncryptionAlgo = encryptionAlgo,
                CreateTime = currentTime,
                LastModifiedTime = currentTime,
            };

            // TODO: Need to:
            //   - Take in a user secret
            //   - Generate the archive key
            //   - Create the auth canary
            //   - Initialize the first authorized user key

            return archive;
        }
    }
}
