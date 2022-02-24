using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Aegis.Models.JsonConverters;

namespace Aegis.Models;

/// <summary>
/// Container that packages encrypted data with an envelope describing the encryption used.
/// </summary>
public class EncryptedPacket
{
    /// <summary>
    /// The initialization vector for the encryption.
    /// </summary>
    [JsonConverter(typeof(JsonByteListBase64Converter))]
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Required for serialization framework.")]
    public List<byte> IV { get; set; }

    /// <summary>
    /// The authentication tag, for when authenticated encryption algorithms are used.
    /// </summary>
    [JsonConverter(typeof(JsonByteListBase64Converter))]
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Required for serialization framework.")]
    public List<byte> AuthTag { get; set; }

    /// <summary>
    /// The actual encrypted data.
    /// </summary>
    [JsonConverter(typeof(JsonByteListBase64Converter))]
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Required for serialization framework.")]
    public List<byte> CipherText { get; set; }
}
