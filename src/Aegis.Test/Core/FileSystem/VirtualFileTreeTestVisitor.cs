using Aegis.Core.FileSystem;

namespace Aegis.Test.Core.FileSystem;

public class VirtualFileTreeTestVisitor : IVirtualFileTreeVisitor
{
    public int PreOrderHits { get; private set; } = 0;

    public List<AegisFileInfo> PreOrderVisitFiles { get; } = new List<AegisFileInfo>();

    public int PostOrderHits { get; private set; } = 0;

    public List<AegisFileInfo> PostOrderVisitFiles { get; } = new List<AegisFileInfo>();

    public void OnPreOrderVisit(AegisVirtualDirectoryPath dirPath, ReadOnlySpan<AegisFileInfo> files)
    {
        this.PreOrderHits++;
        this.PreOrderVisitFiles.AddRange(files.ToArray());
    }

    public void OnPostOrderVisit(AegisVirtualDirectoryPath dirPath, ReadOnlySpan<AegisFileInfo> files)
    {
        this.PostOrderHits++;
        this.PostOrderVisitFiles.AddRange(files.ToArray());
    }

    public void OnStart()
    {
        // Empty.
    }

    public void OnDone()
    {
        // Empty.
    }
}