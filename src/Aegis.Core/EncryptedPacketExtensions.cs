using System;
using System.Collections.Generic;

using Aegis.Models;

namespace Aegis.Core
{
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
                CipherText = new List<byte>(cipherText.ToArray()),
                IV = new List<byte>(iv.ToArray()),
                AuthTag = authTag.IsEmpty
                    ? new List<byte>()
                    : new List<byte>(authTag.ToArray()),
            };
        }

        /// <summary>
        /// Gets whether or the <see cref="EncryptedPacket"/> is empty.
        /// </summary>
        /// <param name="packet">The target <see cref="EncryptedPacket"/> instance.</param>
        public static bool IsEmpty(this EncryptedPacket packet) => packet?.CipherText?.Count == 0;
    }
}
