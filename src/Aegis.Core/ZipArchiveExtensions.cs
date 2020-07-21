using System.IO;
using System.IO.Compression;

namespace Aegis.Core
{
    /// <summary>
    /// A collection of extension methods for interacting with a <see cref="ZipArchive"/>.
    /// </summary>
    internal static class ZipArchiveExtensions
    {
        /// <summary>
        /// Deletes an entry from the <see cref="ZipArchive"/> if such an entry exists.
        /// </summary>
        /// <param name="archive">The archive to delete from.</param>
        /// <param name="entryName">The name of the entry to delete.</param>
        /// <returns>True if an entry was deleted and false otherwise.</returns>
        public static bool DeleteEntryIfExists(this ZipArchive archive, string entryName)
        {
            var zipEntry = archive.GetEntry(entryName);
            if (zipEntry != null)
            {
                zipEntry.Delete();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a stream to write/overwrite an entry in the archive.
        /// </summary>
        /// <param name="archive">The archive to write to.</param>
        /// <param name="entryName">The name of the entry to create/update.</param>
        /// <returns>The write stream, open and ready to be written to.</returns>
        /// <remarks>
        /// The .Net ZipArchive API is fiddly.
        /// It allows for creating duplicate entries with the same name, and has traps when you try to
        /// overwrite existing entries. This method helps get around all that.
        /// </remarks>
        public static Stream GetEntryWriteStream(this ZipArchive archive, string entryName)
        {
            var entry = archive.GetEntry(entryName) ?? archive.CreateEntry(entryName);
            var writeStream = entry.Open();

            // Trap here in the .NET API:
            // By default, the opened stream will contain all the data previously in the entry and will start
            // at position 0. So if you had "FooBar" in the entry previously, and you wanted to overwrite it
            // to be "Baz", you would actually end up with "BazBar" unless you first clear the buffer.
            writeStream.SetLength(0);

            return writeStream;
        }
    }
}
