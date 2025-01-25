namespace Aegis.Core;

/// <summary>
/// Exception thrown when an read or write API call is made on a <see cref="AegisArchive"/> that is locked.
/// </summary>
public class ArchiveLockedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveLockedException"/> class.
    /// </summary>
    public ArchiveLockedException()
        : this("The operation is not possible while the archive is locked.", null)
    {
        // Empty.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveLockedException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ArchiveLockedException(string message)
        : this(message, null)
    {
        // Empty.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveLockedException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The cause of the error.</param>
    public ArchiveLockedException(string message, Exception innerException)
        : base(message, innerException)
    {
        // Empty.
    }
}
