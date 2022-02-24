using Aegis.Core.Crypto;
using Aegis.Models;

namespace Aegis.Core.FileSystem;

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
    public FileIndex(IEnumerable<FileIndexEntry> entries)
    {
        this.FileEntriesById = new Dictionary<Guid, AegisFileInfo>();
        this.FileTree = new VirtualFileTree();

        foreach (var fileInfo in entries.Select(e => new AegisFileInfo(new AegisVirtualFilePath(e.FilePath), e)))
        {
            this.FileEntriesById.Add(fileInfo.FileId, fileInfo);
            this.FileTree.Add(fileInfo);
        }
    }

    /// <summary>
    /// Gets the files indexed by their file ID.
    /// </summary>
    private Dictionary<Guid, AegisFileInfo> FileEntriesById { get; }

    /// <summary>
    /// Gets the file entries arranged in a tree by their virtual paths.
    /// </summary>
    private VirtualFileTree FileTree { get; }

    /// <summary>
    /// Encrypts the <see cref="FileIndex"/> for secure storage.
    /// </summary>
    /// <param name="archiveKey">The encryption key to use.</param>
    /// <param name="securitySettings">The <see cref="SecuritySettings"/> for the secure archive.</param>
    /// <returns>The encrypted <see cref="FileIndex"/> data.</returns>
    public EncryptedPacket Encrypt(ArchiveKey archiveKey, SecuritySettings securitySettings)
        => JsonHelpers.SerializeAndEncrypt<List<FileIndexEntry>>(
            this.FileEntriesById.Values.Select(i => i.IndexEntry).ToList(),
            archiveKey,
            securitySettings);

    /// <summary>
    /// Adds a <see cref="AegisFileInfo"/> to the index.
    /// </summary>
    /// <param name="fileInfo">The <see cref="AegisFileInfo"/> to add.</param>
    public void Add(AegisFileInfo fileInfo)
    {
        var existingFileInfo = this.FileTree.Find(fileInfo.Path);

        if (existingFileInfo != null
            || this.FileEntriesById.ContainsKey(fileInfo.FileId))
        {
            throw new AegisInternalErrorException("File index entry already exists.");
        }

        this.FileEntriesById.TryAdd(fileInfo.FileId, fileInfo);
        this.FileTree.Add(fileInfo);
    }

    /// <summary>
    /// Removes information about a file from the index.
    /// </summary>
    /// <param name="fileId">The unique identifier of the file to remove.</param>
    public void Remove(Guid fileId)
    {
        if (this.FileEntriesById.TryGetValue(fileId, out var fileInfo))
        {
            this.FileEntriesById.Remove(fileInfo.FileId);
            this.FileTree.Remove(fileInfo.Path);
        }
    }

    /// <summary>
    /// Removes information about a file from the index.
    /// </summary>
    /// <param name="filePath">The virtual path of the file to remove.</param>
    public void Remove(AegisVirtualFilePath filePath)
    {
        var fileInfo = this.FileTree.Remove(filePath);

        if (fileInfo is not null)
        {
            this.FileEntriesById.Remove(fileInfo.FileId);
        }
    }

    /// <summary>
    /// Retrieves information about a file in the archive.
    /// </summary>
    /// <param name="fileId">The unique identifier of the file.</param>
    /// <returns>The <see cref="AegisFileInfo"/> about the file, or null if it isn't found.</returns>
    public AegisFileInfo GetFileInfo(Guid fileId)
        => this.FileEntriesById.TryGetValue(fileId, out var fileInfo) ? fileInfo : null;

    /// <summary>
    /// Retrieves information about a file in the archive.
    /// </summary>
    /// <param name="filePath">The virual path to the file.</param>
    /// <returns>The <see cref="AegisFileInfo"/> about the file, or null if it isn't found.</returns>
    public AegisFileInfo GetFileInfo(AegisVirtualFilePath filePath) => this.FileTree.Find(filePath);

    /// <summary>
    /// Executes a traversal of the virtual tree of archived files.
    /// </summary>
    /// <param name="visitorImplementation">The visitor implementation to execute when visiting each node.</param>
    public void TraverseFileTree(IVirtualFileTreeVisitor visitorImplementation)
        => this.FileTree.TraverseNodes(visitorImplementation);
}
