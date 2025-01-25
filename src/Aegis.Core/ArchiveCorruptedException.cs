namespace Aegis.Core;

/// <summary>
/// Exception thrown when a <see cref="AegisArchive"/> is found to be corrupted.
/// </summary>
public class ArchiveCorruptedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveCorruptedException"/> class.
    /// </summary>
    public ArchiveCorruptedException()
        : this("The archive is corrupted!", null)
    {
        // Empty.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveCorruptedException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ArchiveCorruptedException(string message)
        : this(message, null)
    {
        // Empty.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveCorruptedException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The cause of the error.</param>
    public ArchiveCorruptedException(string message, Exception innerException)
        : base(message, innerException)
    {
        // Empty.
    }
}
