using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Aegis.Core
{
    /// <summary>
    /// Exception thrown when Aegis encounters an internal error. This indicates a bug in the Aegis library.
    /// </summary>
    [Serializable]
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

        /// <summary>
        /// Initializes a new instance of the <see cref="AegisInternalErrorException"/> class.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        /// <remarks>
        /// This overload is required to properly implement <see cref="ISerializable"/>.
        /// </remarks>
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "The method signature is required by the framework.")]
        protected AegisInternalErrorException(SerializationInfo info, StreamingContext context)
        {
            // Deserialize any internal properties using info.GetValue
        }

        /// <summary>
        /// Serializes internal state to the <see cref="SerializationInfo"/> object.
        /// </summary>
        /// <param name="info">Details of the serialization.</param>
        /// <param name="context">Stream context.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ArgCheck.NotNull(info, nameof(info));

            // Serialize any internal properties using info.AddValue
            base.GetObjectData(info, context);
        }
    }
}