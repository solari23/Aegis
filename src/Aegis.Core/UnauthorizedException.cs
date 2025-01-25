namespace Aegis.Core;

/// <summary>
/// Exception thrown when an unauthorized user tries to access the <see cref="AegisArchive"/>.
/// </summary>
public class UnauthorizedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
    /// </summary>
    public UnauthorizedException()
        : this("The user is not authorized!", null)
    {
        // Empty.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public UnauthorizedException(string message)
        : this(message, null)
    {
        // Empty.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The cause of the error.</param>
    public UnauthorizedException(string message, Exception innerException)
        : base(message, innerException)
    {
        // Empty.
    }
}
