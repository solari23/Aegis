using System.Text.Json.Serialization;

namespace Aegis.Models
{
    /// <summary>
    /// Metadata that describes security settings (e.g. crypto algorithm choice) for the archive.
    /// </summary>
    public class SecuritySettings
    {
        /// <summary>
        /// The algorithm used to encrypt data in the archive.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EncryptionAlgo EncryptionAlgo { get; set; }

        /// <summary>
        /// The key derivation function (KDF) used to generate user keys.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public KeyDerivationFunction KeyDerivationFunction { get; set; }

        /// <summary>
        /// The work factor parameter (e.g. iteration count) for the KDF.
        /// </summary>
        public int KeyDerivationWorkFactor { get; set; }

        /// <summary>
        /// The size (in bytes) of KeyIds generated for user keys.
        /// </summary>
        public int KeyIdSizeInBytes { get; set; }
    }
}
