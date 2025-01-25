using System.Security.Cryptography.X509Certificates;
using Aegis.Core.CredentialsInterface;
using Aegis.Core.Crypto;
using Aegis.Models;

namespace Aegis.Test;

/// <summary>
/// Secrets to use for testing purposes.
/// </summary>
public static class TestSecrets
{
    /// <summary>
    /// The default password for test archives.
    /// </summary>
    public const string DefaultPassword = "P@$$w3rd";

    /// <summary>
    /// The default certificate for an RSA key, exported with the private key in PFX format.
    /// </summary>
    private const string DefaultRsaCertificatePfx = "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";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="secretKind"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static RawUserSecret GetDefaultUserSecret(SecretKind secretKind) => secretKind switch
    {
        SecretKind.Password => RawUserSecret.FromPasswordString(DefaultPassword),
        SecretKind.RsaKeyFromCertificate => RawUserSecret.FromCertificate(
            new X509Certificate2(Convert.FromBase64String(DefaultRsaCertificatePfx), (string)null, X509KeyStorageFlags.Exportable)),
        _ => throw new Exception($"SecretKind {secretKind} not supported by this helper."),
    };


    public static UserKeyAuthorizationParameters GetDefaultUserKeyAuthorizationParameters(SecretKind secretKind) => secretKind switch
    {
        SecretKind.Password => new UserKeyAuthorizationParameters(GetDefaultUserSecret(secretKind))
        {
            FriendlyName = "TestPassword",
            SecretMetadata = new PasswordSecretMetadata(),
        },
        SecretKind.RsaKeyFromCertificate => new UserKeyAuthorizationParameters(GetDefaultUserSecret(secretKind))
        {
            FriendlyName = "TestRsaCertificate",
            SecretMetadata = new RsaKeyFromCertificateSecretMetadata
            {
                Thumbprint = "6cb6e4fddece5fd19c6194d41d63d097dd694a3e",
            },
        },
        _ => throw new Exception($"SecretKind {secretKind} not supported by this helper."),
    };
}