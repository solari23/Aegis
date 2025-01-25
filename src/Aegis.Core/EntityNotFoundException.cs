namespace Aegis.Core;

/// <summary>
/// Exception thrown when a requested entity isn't found in the <see cref="AegisArchive"/>.
/// </summary>
public class EntityNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
    /// </summary>
    public EntityNotFoundException()
        : this("The requested entity was not found.", null)
    {
        // Empty.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public EntityNotFoundException(string message)
        : this(message, null)
    {
        // Empty.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The cause of the error.</param>
    public EntityNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
        // Empty.
    }
}
