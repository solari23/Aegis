using System;

namespace Aegis
{
    /// <summary>
    /// Exception thrown to indicate a user error.
    /// </summary>
    public class AegisUserErrorException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AegisUserErrorException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="isRecoverable">Whether or not the program can recover from the error.</param>
        /// <param name="innerException">The cause of the error.</param>
        public AegisUserErrorException(string message, bool isRecoverable = true, Exception innerException = null)
            : base(message, innerException)
        {
            this.IsRecoverable = isRecoverable;
        }

        /// <summary>
        /// Gets whether or not the error is recoverable. If not, program execution should end immediately.
        /// </summary>
        public bool IsRecoverable { get; }
    }
}