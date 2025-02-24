﻿namespace Aegis.Core.CredentialsInterface;

/// <summary>
/// Exception to be thrown by user interface implementations when the user cancels a prompt.
/// </summary>
public class SecretInterfaceOperationCancelledException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveCorruptedException"/> class.
    /// </summary>
    public SecretInterfaceOperationCancelledException()
        : this("The secret interface operation was cancelled!", null)
    {
        // Empty.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecretInterfaceOperationCancelledException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public SecretInterfaceOperationCancelledException(string message)
        : this(message, null)
    {
        // Empty.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecretInterfaceOperationCancelledException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The cause of the error.</param>
    public SecretInterfaceOperationCancelledException(string message, Exception innerException)
        : base(message, innerException)
    {
        // Empty.
    }
}
