using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Aegis.Core;

/// <summary>
/// Exception thrown when a requested entity isn't found in the <see cref="AegisArchive"/>.
/// </summary>
[Serializable]
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

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
    /// </summary>
    /// <param name="info">The SerializationInfo to populate with data.</param>
    /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
    /// <remarks>
    /// This overload is required to properly implement <see cref="ISerializable"/>.
    /// </remarks>
    protected EntityNotFoundException(SerializationInfo info, StreamingContext context)
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
        ArgCheck.NotNull(info);

        // Serialize any internal properties using info.AddValue
        base.GetObjectData(info, context);
    }
}
