using System.Text.Json;

using Aegis.Core.Crypto;
using Aegis.Models;

namespace Aegis.Core;

/// <summary>
/// A collection of static helper utilities for dealing with JSON (de)serialization.
/// </summary>
internal static class JsonHelpers
{
    public static JsonSerializerOptions DefaultSerializerOptions => new JsonSerializerOptions
    {
        WriteIndented = true,
    };

    /// <summary>
    /// Decrypts and deserializes an encrypted JSON object.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="encryptedData">The encrypted object data.</param>
    /// <param name="key">The archive's decryption key to use.</param>
    /// <param name="securitySettings">The archive's security settings.</param>
    /// <returns>The requested deserialized object.</returns>
    public static T DecryptAndDeserialize<T>(
        EncryptedPacket encryptedData,
        ArchiveKey key,
        SecuritySettings securitySettings)
    {
        var cryptoStrategy = CryptoHelpers.GetCryptoStrategy(securitySettings.EncryptionAlgo);
        var rawData = key.Decrypt(cryptoStrategy, encryptedData);
        return JsonSerializer.Deserialize<T>(rawData);
    }

    /// <summary>
    /// Serializes a given object as JSON and encrypts it.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="objectToSerialize">The object to serialize.</param>
    /// <param name="key">The archive's ecryption key to use.</param>
    /// <param name="securitySettings">The archive's security settings.</param>
    /// <returns>The serialized and encrypted data.</returns>
    public static EncryptedPacket SerializeAndEncrypt<T>(
        T objectToSerialize,
        ArchiveKey key,
        SecuritySettings securitySettings)
    {
        var rawData = JsonSerializer.SerializeToUtf8Bytes(objectToSerialize);
        var cryptoStrategy = CryptoHelpers.GetCryptoStrategy(securitySettings.EncryptionAlgo);
        return key.Encrypt(cryptoStrategy, rawData);
    }
}
