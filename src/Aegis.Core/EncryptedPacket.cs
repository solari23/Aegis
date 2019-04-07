namespace Aegis.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Container that packages encrypted data with an envelope describing the encryption used.
    /// </summary>
    /// <remarks>
    /// Data members for this class are defined in a Bond schema (see Aegis.bond).
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1047:DoNotDeclareProtectedMembersInSealedTypes", Justification = "The protected ctor is code generated.")]
    public sealed partial class EncryptedPacket
    {
        /// <summary>
        /// Creates a new instance of the <see cref="EncryptedPacket"/> class.
        /// </summary>
        /// <param name="cipherText">The encrypted data.</param>
        /// <param name="iv">The initialization vector for the encryption.</param>
        /// <param name="authTag">The authentication tag, for when authenticated encryption algorithms are used.</param>
        /// <returns></returns>
        internal EncryptedPacket(
            Span<byte> cipherText,
            Span<byte> iv,
            Span<byte> authTag = default)
        {
            ArgCheck.NotEmpty(cipherText, nameof(cipherText));
            ArgCheck.NotEmpty(iv, nameof(iv));

            this.CipherText = new List<byte>(cipherText.ToArray());
            this.IV = new List<byte>(iv.ToArray());

            this.AuthTag = authTag.IsEmpty
                ? new List<byte>()
                : new List<byte>(authTag.ToArray());
        }

        /// <summary>
        /// Gets whether or the <see cref="EncryptedPacket"/> is empty.
        /// </summary>
        public bool IsEmpty => this.CipherText?.Count == 0;
    }
}
