﻿namespace Aegis.Core;

/// <summary>
/// Exception thrown when Aegis encounters an internal error. This indicates a bug in the Aegis library.
/// </summary>
public class AegisInternalErrorException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AegisInternalErrorException"/> class.
    /// </summary>
    public AegisInternalErrorException()
        : this("Aegis encountered an unexpected internal error!", null)
    {
        // Empty.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AegisInternalErrorException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public AegisInternalErrorException(string message)
        : this(message, null)
    {
        // Empty.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AegisInternalErrorException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The cause of the error.</param>
    public AegisInternalErrorException(string message, Exception innerException)
        : base(message, innerException)
    {
        // Empty.
    }
}
