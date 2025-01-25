namespace Aegis.Core.Crypto;

/// <summary>
/// An exception thrown to indicate that an input secret was invalid.
/// </summary>
public class InvalidSecretException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidSecretException"/> class.
    /// </summary>
    public InvalidSecretException()
        : this("Secret is not valid!", null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidSecretException"/> class.
    /// </summary>
    public InvalidSecretException(string message)
        : this(message, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidSecretException"/> class.
    /// </summary>
    public InvalidSecretException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
