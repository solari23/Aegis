﻿namespace Aegis.Core
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// Exception thrown when an unauthorized user tries to access the <see cref="SecureArchive"/>.
    /// </summary>
    [Serializable]
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

        /// <summary>
        /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        /// <remarks>
        /// This overload is required to properly implement <see cref="ISerializable"/>.
        /// </remarks>
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "The method signature is required by the framework.")]
        protected UnauthorizedException(SerializationInfo info, StreamingContext context)
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