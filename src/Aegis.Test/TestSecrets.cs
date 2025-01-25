using System.Text;

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
    /// 
    /// </summary>
    /// <param name="secretKind"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static RawUserSecret GetDefaultUserSecret(SecretKind secretKind) => secretKind switch
    {
        SecretKind.Password => new RawUserSecret(Encoding.UTF8.GetBytes(DefaultPassword)),
        _ => throw new Exception($"SecretKind {secretKind} not supported by this helper."),
    };


    public static UserKeyAuthorizationParameters GetDefaultUserKeyAuthorizationParameters(SecretKind secretKind) => secretKind switch
    {
        SecretKind.Password => new UserKeyAuthorizationParameters(GetDefaultUserSecret(secretKind))
        {
            FriendlyName = "TestPassword",
            SecretMetadata = new PasswordSecretMetadata(),
        },
        _ => throw new Exception($"SecretKind {secretKind} not supported by this helper."),
    };
}