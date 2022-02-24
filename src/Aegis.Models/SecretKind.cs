namespace Aegis.Models;

/// <summary>
/// Defines the types of secrets available for protecting and accessing Aegis archives.
/// </summary>
public enum SecretKind
{
    /// <summary>
    /// Invalid value to indicate an uninitialized field.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// A user-chosen password.
    /// </summary>
    Password = 1,
}
