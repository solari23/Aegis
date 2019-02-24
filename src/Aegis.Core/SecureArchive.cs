namespace Aegis.Core
{
    using System;

    /// <summary>
    /// The core data structure that represents SecureArchive files. The SecureArchive is a
    /// self-contained file that holds the user's encrypted documents
    /// </summary>
    public partial class SecureArchive
    {
        /// <summary>
        /// Creates a new <see cref="SecureArchive"/> that contains no files.
        /// </summary>
        /// <returns>A new <see cref="SecureArchive"/>.</returns>
        public static SecureArchive CreateNew()
        {
            var currentTime = DateTime.UtcNow;

            var archive = new SecureArchive();
            archive.Id = Guid.NewGuid();
            archive.FileVersion = Constants.CurrentAegisSecureArchiveFileVersion;
            archive.CreateTime = currentTime;
            archive.LastModifiedTime = currentTime;

            // TODO: Need to:
            //   - Take in a user secret and encryption algo
            //   - Generate the archive key
            //   - Create the auth canary
            //   - Initialize the first authorized user key

            return archive;
        }
    }
}
