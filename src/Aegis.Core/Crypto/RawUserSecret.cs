using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Aegis.Core.Crypto;

/// <summary>
/// Container for a user secret used to derive user keys.
/// </summary>
public sealed class RawUserSecret : Secret
{
    /// <summary>
    /// Helps to extract the canonical <see cref="RawUserSecret"/> from a password string.
    /// </summary>
    /// <param name="password">The password string.</param>
    /// <returns>The corresponding <see cref="RawUserSecret"/>.</returns>
    /// <remarks>The raw secret is the UTF8 encoding of the string.</remarks>
    public static RawUserSecret FromPasswordString(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new InvalidSecretException("Input password can not be empty.");
        }

        return new RawUserSecret(Encoding.UTF8.GetBytes(password));
    }

    /// <summary>
    /// Helps to extract the canonical <see cref="RawUserSecret"/> from a certificate.
    /// </summary>
    /// <param name="certificate">The certificate to extract the secret from.</param>
    /// <returns>The corresponding <see cref="RawUserSecret"/>.</returns>
    /// <remarks>
    /// For certificates holding RSA keys, the raw secret is the private exponent 'D'.
    /// Certificates for other algorithms are not supported.
    /// </remarks>
    public static RawUserSecret FromCertificate(X509Certificate2 certificate)
    {
        if (!certificate.HasPrivateKey)
        {
            throw new InvalidSecretException("Input certificate does not provide access to a private key.");
        }

        using var privateKey = certificate.GetRSAPrivateKey();
        if (privateKey is null)
        {
            throw new InvalidSecretException("Input certificate does not have an RSA private key.");
        }

        byte[] rsaKeyPrivateExponent = null;

        try
        {
            var rsaParams = privateKey.ExportParameters(includePrivateParameters: true);
            rsaKeyPrivateExponent = rsaParams.D;
        }
        catch (CryptographicException)
        {
            // We weren't able to export the key, though it may actually be exportable.
            // There is one more thing we can try.
        }

        if (rsaKeyPrivateExponent is null)
        {
            
            try
            {
                // In CNG, 'exportable' seems to only work if the key is/was encrypted.
                // Round-trip it using a temporary password.
                // See: https://github.com/dotnet/runtime/issues/26031#issuecomment-632912957
                var tmpPassword = Guid.NewGuid().ToByteArray();
                var passwordEncryptionParameters = new PbeParameters(
                    PbeEncryptionAlgorithm.Aes256Cbc,
                    HashAlgorithmName.SHA256,
                    100_000);

                using var tmpKey = RSA.Create();
                tmpKey.ImportEncryptedPkcs8PrivateKey(
                    tmpPassword,
                    privateKey.ExportEncryptedPkcs8PrivateKey(tmpPassword, passwordEncryptionParameters),
                    out _);

                var rsaParams = tmpKey.ExportParameters(includePrivateParameters: true);
                rsaKeyPrivateExponent = rsaParams.D;
            }
            catch (CryptographicException e)
            {
                throw new InvalidSecretException("Input certificate private parameters are not exportable.", e);
            }
        }

        return new RawUserSecret(rsaKeyPrivateExponent);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RawUserSecret"/> class.
    /// </summary>
    /// <param name="keyData">The raw archive encryption key.</param>
    public RawUserSecret(byte[] keyData)
        : base(keyData)
    {
        // Empty.
    }
}
