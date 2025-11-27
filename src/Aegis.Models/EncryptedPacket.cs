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
    [JsonConverter(typeof(ByteArrayBase64JsonConverter))]
    public byte[] IV { get; set; }

    /// <summary>
    /// The authentication tag, for when authenticated encryption algorithms are used.
    /// </summary>
    [JsonConverter(typeof(ByteArrayBase64JsonConverter))]
    public byte[] AuthTag { get; set; }

    /// <summary>
    /// The actual encrypted data.
    /// </summary>
    [JsonConverter(typeof(ByteArrayBase64JsonConverter))]
    public byte[] CipherText { get; set; }
}
