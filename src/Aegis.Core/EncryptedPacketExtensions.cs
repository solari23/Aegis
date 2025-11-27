using System.Diagnostics;

using Aegis.Core.Crypto;
using Aegis.Models;

namespace Aegis.Core;

/// <summary>
/// A collection of extension methods for the <see cref="EncryptedPacket"/> class.
/// </summary>
internal static class EncryptedPacketExtensions
{
    /// <summary>
    /// Creates a new instance of the <see cref="EncryptedPacket"/> class.
    /// </summary>
    /// <param name="cipherText">The encrypted data.</param>
    /// <param name="iv">The initialization vector for the encryption.</param>
    /// <param name="authTag">The authentication tag, for when authenticated encryption algorithms are used.</param>
    /// <returns></returns>
    public static EncryptedPacket CreateNewEncryptedPacket(
        Span<byte> cipherText,
        Span<byte> iv,
        Span<byte> authTag = default)
    {
        ArgCheck.NotEmpty(cipherText, nameof(cipherText));
        ArgCheck.NotEmpty(iv, nameof(iv));

        return new EncryptedPacket
        {
            CipherText = cipherText.ToArray(),
            IV = iv.ToArray(),
            AuthTag = authTag.IsEmpty
                ? []
                : authTag.ToArray(),
        };
    }

    /// <summary>
    /// Gets whether or the <see cref="EncryptedPacket"/> is empty.
    /// </summary>
    /// <param name="packet">The target <see cref="EncryptedPacket"/> instance.</param>
    public static bool IsEmpty(this EncryptedPacket packet) => packet?.CipherText?.Length == 0;

    /// <summary>
    /// Writes the <see cref="EncryptedPacket"/> in pure binary form to the given stream.
    /// </summary>
    /// <param name="packet">The packet to write.</param>
    /// <param name="stream">The stream to write to.</param>
    public static void WriteToBinaryStream(this EncryptedPacket packet, Stream stream)
    {
        stream.Write(packet.IV.ToArray());
        stream.Write(packet.AuthTag.ToArray());
        stream.Write(packet.CipherText.ToArray());
    }

    /// <summary>
    /// Reads a pure binary form <see cref="EncryptedPacket"/> from the given stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="encryptionAlgo">The algo used to create the encrypted packet.</param>
    /// <returns>The <see cref="EncryptedPacket"/> read from the stream.</returns>
    public static EncryptedPacket ReadFromBinaryStream(Stream stream, EncryptionAlgo encryptionAlgo)
    {
        byte[] iv = null;
        byte[] authTag = null;
        byte[] cipherText = null;

        var encryptionAlgoProperties = EncryptionAlgoProperties.For(encryptionAlgo);

        if (encryptionAlgoProperties.IvSizeInBytes > 0)
        {
            iv = new byte[encryptionAlgoProperties.IvSizeInBytes];

            var ivBytesRead = stream.Read(iv, 0, encryptionAlgoProperties.IvSizeInBytes);
            Debug.Assert(ivBytesRead == encryptionAlgoProperties.IvSizeInBytes);
        }

        if (encryptionAlgoProperties.IsAuthenticated && encryptionAlgoProperties.AuthTagSizeInBytes > 0)
        {
            authTag = new byte[encryptionAlgoProperties.AuthTagSizeInBytes];

            var authTagBytesRead = stream.Read(authTag);
            Debug.Assert(authTagBytesRead == encryptionAlgoProperties.AuthTagSizeInBytes);
        }

        var cipherTextSizeInBytes = stream.Length - stream.Position;

        if (cipherTextSizeInBytes > 0)
        {
            cipherText = new byte[cipherTextSizeInBytes];

            var cipherTextBytesRead = stream.Read(cipherText);
            Debug.Assert(cipherTextBytesRead == cipherTextSizeInBytes);
        }

        return CreateNewEncryptedPacket(cipherText, iv, authTag);
    }
}
