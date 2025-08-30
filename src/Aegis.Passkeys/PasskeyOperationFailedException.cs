namespace Aegis.Passkeys;

/// <summary>
/// Exception thrown when a passkey operation fails.
/// </summary>
public class PasskeyOperationFailedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PasskeyOperationFailedException"/> class.
    /// </summary>
    /// <param name="failureCode">The failure code classifying the error.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">An inner exception previously caught (optional).</param>
    internal PasskeyOperationFailedException(
        PasskeyFailureCode failureCode,
        string message,
        Exception? innerException = null)
        : this(failureCode, message, 0, string.Empty, innerException)
    {
        // Empty.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PasskeyOperationFailedException"/> class.
    /// </summary>
    /// <param name="failureCode">The failure code classifying the error.</param>
    /// <param name="message">The error message.</param>
    /// <param name="platformErrorCode">The error code returned by the underlying OS platform.</param>
    /// <param name="webAuthnError">The WebAuthn error string corresponding to the error (if applicable).</param>
    /// <param name="exception">An inner exception previously caught (optional).</param>
    internal PasskeyOperationFailedException(
        PasskeyFailureCode failureCode,
        string message,
        int platformErrorCode,
        string webAuthnError,
        Exception? exception = null)
        : base(message, exception)
    {
        this.PlatformErrorCode = platformErrorCode;
        this.WebAuthnErrorName = webAuthnError;
    }

    /// <summary>
    /// The failure code classifying the error.
    /// </summary>
    public PasskeyFailureCode FailureCode { get; private set; }

    /// <summary>
    /// The error code returned by the underlying OS platform (if applicable).
    /// </summary>
    public int PlatformErrorCode { get; private set; }

    /// <summary>
    /// The WebAuthn error string corresponding to the error (if applicable).
    /// </summary>
    public string WebAuthnErrorName { get; private set; }
}
