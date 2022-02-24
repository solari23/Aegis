using Aegis.Models;

namespace Aegis.Core.FileSystem;

/// <summary>
/// Information about an Aegis archive file.
/// </summary>
public class AegisFileInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AegisFileInfo"/> class.
    /// </summary>
    /// <param name="path">The virtual path to the file.</param>
    /// <param name="indexEntry">The underlying index entry for the file.</param>
    internal AegisFileInfo(AegisVirtualFilePath path, FileIndexEntry indexEntry)
    {
        this.Path = path;
        this.IndexEntry = indexEntry;
    }

    /// <summary>
    /// Gets a unique identifier for the file.
    /// </summary>
    public Guid FileId => this.IndexEntry.FileId;

    /// <summary>
    /// Gets when the file was added to the archive.
    /// </summary>
    public DateTimeOffset AddedTime => this.IndexEntry.AddedTime;

    /// <summary>
    /// Gets when the archived file was last modified.
    /// </summary>
    public DateTimeOffset LastModifiedTime => this.IndexEntry.LastModifiedTime;

    /// <summary>
    /// Gets the virtual path to the file.
    /// </summary>
    public AegisVirtualFilePath Path { get; }

    /// <summary>
    /// Gets the underlying index entry for the file.
    /// </summary>
    internal FileIndexEntry IndexEntry { get; }

    /// <summary>
    /// Gets the name of the file as stored in the secured zip archive.
    /// </summary>
    internal string ArchiveEntryName => this.FileId.ToString();
}
