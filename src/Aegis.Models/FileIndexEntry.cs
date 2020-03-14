using System;

namespace Aegis.Models
{
    /// <summary>
    /// Metadata about a file stored in the secure archive.
    /// </summary>
    public class FileIndexEntry
    {
        /// <summary>
        /// A unique identifier for the file within the archive.
        /// </summary>
        public Guid FileId { get; set; }

        /// <summary>
        /// A virtual file path. These must be unique within the archive.
        /// </summary>
        /// <remarks>
        /// File paths/names in Aegis are case-insensitive.
        /// </remarks>
        public string FilePath { get; set; }

        /// <summary>
        /// When the file was added to the archive.
        /// </summary>
        public DateTimeOffset AddedTime { get; set; }

        /// <summary>
        /// When the archived file was last modified.
        /// </summary>
        public DateTimeOffset LastModifiedTime { get; set; }
    }
}
