﻿namespace Aegis.Core.Crypto
{
    using System;
    using System.Security.Cryptography;

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
        /// Gets the <see cref="ICryptoStrategy"/> associated with the given cryptographic algorithm.
        /// </summary>
        /// <param name="algo">The cryptographic algorithm.</param>
        /// <returns>The <see cref="ICryptoStrategy"/> associated with the algorithm.</returns>
        internal static ICryptoStrategy GetCryptoStrategy(EncryptionAlgo algo)
        {
            ArgCheck.IsNot(EncryptionAlgo.Unknown, algo, nameof(algo));

            switch (algo)
            {
                case EncryptionAlgo.Aes256Gcm:
                    return new Aes256GcmCryptoStrategy();

                default:
                    throw new InvalidOperationException($"No CryptoStrategy defined for algorithm '{algo}'!");
            }
        }

        /// <summary>
        /// Gets the <see cref="IKeyDerivationStrategy"/> associated with the given key derivation function.
        /// </summary>
        /// <param name="keyDerivationFunction">The key derivation function.</param>
        /// <returns>The <see cref="IKeyDerivationStrategy"/> associated with the key derivation function.</returns>
        internal static IKeyDerivationStrategy GetKeyDerivationStrategy(KeyDerivationFunction keyDerivationFunction)
        {
            ArgCheck.IsNot(KeyDerivationFunction.Unknown, keyDerivationFunction, nameof(keyDerivationFunction));

            switch (keyDerivationFunction)
            {
                case KeyDerivationFunction.PBKDF2:
                    return new Pbkdf2KeyDerivationStrategy();

                default:
                    throw new InvalidOperationException($"No key derivation strategy defined for function '{keyDerivationFunction}'!");
            }
        }
    }
}
