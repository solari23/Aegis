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
    /// <remarks>
    /// The raw key used to protect the archive is the Utf8 encoding of the password string.
    /// </remarks>
    Password = 1,

    /// <summary>
    /// An RSA key from a certificate.
    /// </summary>
    /// <remarks>
    /// The raw key used to protect the archive is the private exponent 'D' of the RSA key.
    /// </remarks>
    RsaKeyFromCertificate = 2,

    /// <summary>
    /// A key derived from a FIDO2/WebAuthn passkey using the HMAC-Secret extension.
    /// </summary>
    PasskeyHmacSecret = 3,
}
