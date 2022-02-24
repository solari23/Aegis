namespace Aegis.Core.FileSystem;

/// <summary>
/// Interface for a utilities that implement the Visitor Pattern on the
/// virtual tree of Aegis archive files
/// </summary>
public interface IVirtualFileTreeVisitor
{
    /// <summary>
    /// Callback executed when starting to visit nodes.
    /// </summary>
    void OnStart();

    /// <summary>
    /// Callback executed when a virtual directory node is visited in a
    /// pre-order traversal of the tree.
    /// </summary>
    /// <param name="directory">The virtual directory that the node represents.</param>
    /// <param name="files">The files available at the node.</param>
    void OnPreOrderVisit(AegisVirtualDirectoryPath directory, ReadOnlySpan<AegisFileInfo> files);

    /// <summary>
    /// Callback executed when a virtual directory node is visited in a
    /// post-order traversal of the tree.
    /// </summary>
    /// <param name="directory">The virtual directory that the node represents.</param>
    /// <param name="files">The files available at the node.</param>
    void OnPostOrderVisit(AegisVirtualDirectoryPath directory, ReadOnlySpan<AegisFileInfo> files);

    /// <summary>
    /// Callback executed when done visiting nodes.
    /// </summary>
    void OnDone();
}
