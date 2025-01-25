namespace Aegis.Core.CredentialsInterface;

/// <summary>
/// Exception thrown when none of the authorized user keys registered on an archive
/// can currently be used to unlock the archive.
/// </summary>
public class NoKeyAvailableException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoKeyAvailableException"/> class.
    /// </summary>
    public NoKeyAvailableException()
    {
        // Empty.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoKeyAvailableException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public NoKeyAvailableException(string message)
        : this(message, null)
    {
        // Empty.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoKeyAvailableException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The cause of the error.</param>
    public NoKeyAvailableException(string message, Exception innerException)
        : base(message, innerException)
    {
        // Empty.
    }
}
