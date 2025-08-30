namespace Aegis.Passkeys;

/// <summary>
/// A failure code meant to help classify <see cref="PasskeyOperationFailedException"/> exceptions.
/// </summary>
public enum PasskeyFailureCode
{
    /// <summary>
    /// The operation was cancelled by the user.
    /// </summary>
    OperationCancelled = 1,

    /// <summary>
    /// The interop with the underlying OS platform failed.
    /// </summary>
    PasskeyIneropFailed = 2,

    /// <summary>
    /// The passkey selected by the user does not support HMAC secret extension.
    /// </summary>
    PasskeyDoesNotSupportHmac = 3,
}
