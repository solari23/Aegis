using System;
using System.Security.Cryptography;
using System.Text;

using Aegis.Models;

namespace Aegis.Core.Crypto
{
    /// <summary>
    /// A collection of static helper utilities to deal with crypto operations.
    /// </summary>
    public static class CryptoHelpers
    {
        /// <summary>
        /// Gets an array of a given size filled with cryptographically secure random bytes.
        /// </summary>
        /// <param name="numBytes">The number of bytes required.</param>
        /// <returns>The array of random bytes.</returns>
        public static byte[] GetRandomBytes(int numBytes)
        {
            ArgCheck.NotNegative(numBytes, nameof(numBytes));

            var bytes = new byte[numBytes];
            RandomNumberGenerator.Fill(bytes);
            return bytes;
        }

        /// <summary>
        /// Tests sequence equality of the two operands in a way that is resistant to timing attacks.
        /// </summary>
        /// <param name="left">The left hand operand.</param>
        /// <param name="right">The right hand operand.</param>
        /// <returns>True if the sequences in both operands are exactly the same, false otherwise.</returns>
        /// <remarks>
        /// This method returns 'false' if either operand is empty. For the purposes of this method,
        /// empty operands can never match any other value.
        /// </remarks>
        public static bool SecureEquals(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
        {
            bool isMatch = true;

            isMatch &= !left.IsEmpty;
            isMatch &= !right.IsEmpty;
            isMatch &= left.Length == right.Length;

            for (int i = 0; i < Math.Min(left.Length, right.Length); i++)
            {
                isMatch &= left[i] == right[i];
            }

            return isMatch;
        }

        /// <summary>
        /// Tests if two strings are equal in a way that is resistant to timing attacks.
        /// </summary>
        /// <param name="left">The left hand operand.</param>
        /// <param name="right">The right hand operand.</param>
        /// <returns>True if the strings are exactly the same, false otherwise.</returns>
        /// <remarks>
        /// This method returns 'false' if either string is null or empty. For the purposes of
        /// this method, empty operands can never match any other value.
        /// </remarks>
        public static bool SecureEquals(string left, string right)
        {
            // Use the single-pipe logical OR operator since it does not short-circuit
            // if the first operand resolves to 'true'.
            if (left == null | right == null)
            {
                return false;
            }

            var leftBytes = Encoding.UTF8.GetBytes(left);
            var rightBytes = Encoding.UTF8.GetBytes(right);

            return SecureEquals(leftBytes, rightBytes);
        }

        /// <summary>
        /// Gets the <see cref="ICryptoStrategy"/> associated with the given cryptographic algorithm.
        /// </summary>
        /// <param name="algo">The cryptographic algorithm.</param>
        /// <returns>The <see cref="ICryptoStrategy"/> associated with the algorithm.</returns>
        internal static ICryptoStrategy GetCryptoStrategy(EncryptionAlgo algo) => algo switch
        {
            EncryptionAlgo.Aes256Gcm => new Aes256GcmCryptoStrategy(),
            _ => throw new InvalidOperationException($"No CryptoStrategy defined for algorithm '{algo}'!"),
        };

        /// <summary>
        /// Gets the <see cref="IKeyDerivationStrategy"/> associated with the given key derivation function.
        /// </summary>
        /// <param name="function">The key derivation function.</param>
        /// <returns>The <see cref="IKeyDerivationStrategy"/> associated with the key derivation function.</returns>
        internal static IKeyDerivationStrategy GetKeyDerivationStrategy(KeyDerivationFunction function) => function switch
        {
            KeyDerivationFunction.PBKDF2 => new Pbkdf2KeyDerivationStrategy(),
            _ => throw new InvalidOperationException($"No key derivation strategy defined for function '{function}'!"),
        };
    }
}
