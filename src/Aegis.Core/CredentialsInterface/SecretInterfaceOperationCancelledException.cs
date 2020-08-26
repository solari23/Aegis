using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Aegis.Core.CredentialsInterface
{
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

        /// <summary>
        /// Initializes a new instance of the <see cref="SecretInterfaceOperationCancelledException"/> class.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        /// <remarks>
        /// This overload is required to properly implement <see cref="ISerializable"/>.
        /// </remarks>
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "The method signature is required by the framework.")]
        protected SecretInterfaceOperationCancelledException(SerializationInfo info, StreamingContext context)
        {
            // Deserialize any internal properties using info.GetValue
        }

        /// <summary>
        /// Serializes internal state to the <see cref="SerializationInfo"/> object.
        /// </summary>
        /// <param name="info">Details of the serialization.</param>
        /// <param name="context">Stream context.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ArgCheck.NotNull(info, nameof(info));

            // Serialize any internal properties using info.AddValue
            base.GetObjectData(info, context);
        }
    }
}
