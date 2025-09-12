using System.Security.Cryptography;

using Aegis.Passkeys;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aegis.Test.Passkeys;

[TestClass]
public class PasskeyManualTests
{
    [TestMethod]
    [Ignore("Manual test - run only during active development.")]
    public void TestMakeCredential()
    {
        var passkeyManager = new PasskeyManager();

        var isPasskeySupported = passkeyManager.IsHmacSecretSupported();
        var makeCredResponse = passkeyManager.MakeCredentialWithHmacSecret(
            rpInfo: new RelyingPartyInfo
            {
                Id = "AlkerTestRP.local",
                Origin = "https://AlkerTestRP.local",
                DisplayName = "Alker Test RP"
            },
            userInfo: new UserEntityInfo
            {
                Id = new Identifier([0xFE, 0xED, 0xBE, 0xEF, 0xF0, 0x0D]),
                Name = "testuser",
                DisplayName = "Test User"
            },
            salt: DefaultSalt,
            secondSalt: DefaultSalt);
    }

    [TestMethod]
    [Ignore("Manual test - run only during active development.")]
    public void TestEndToEnd()
    {
        var passkeyManager = new PasskeyManager();

        var isPasskeySupported = passkeyManager.IsHmacSecretSupported();
        Assert.IsTrue(isPasskeySupported);

        var userId = RandomNumberGenerator.GetBytes(8);
        var userName = $"user-{Convert.ToHexString(userId)}@AlkerTestRP.local";

        var makeCredResponse = passkeyManager.MakeCredentialWithHmacSecret(
            rpInfo: new RelyingPartyInfo
            {
                Id = "AlkerTestRP.local",
                Origin = "https://AlkerTestRP.local",
                DisplayName = "Alker Test RP"
            },
            userInfo: new UserEntityInfo
            {
                Id = new Identifier(userId),
                Name = userName,
                DisplayName = "Test User"
            },
            salt: DefaultSalt,
            secondSalt: ZeroSalt);
        Assert.IsNotNull(makeCredResponse);
        Assert.IsNotNull(makeCredResponse.NewCredentialId);
        Assert.IsNotNull(makeCredResponse.FirstHmac);
        Assert.IsNotNull(makeCredResponse.SecondHmac);
        Assert.IsFalse(makeCredResponse.FirstHmac.Secret.SequenceEqual(makeCredResponse.SecondHmac.Secret));

        var getHmacResponse = passkeyManager.GetHmacSecret(
            rpInfo: new RelyingPartyInfo
            {
                Id = "AlkerTestRP.local",
                Origin = "https://AlkerTestRP.local",
                DisplayName = "Alker Test RP"
            },
            salt: DefaultSalt,
            secondSalt: ZeroSalt,
            allowedCredentialIds: [makeCredResponse.NewCredentialId]);
        Assert.IsNotNull(getHmacResponse);
        Assert.IsNotNull(getHmacResponse.First);
        Assert.IsNotNull(getHmacResponse.Second);
        Assert.IsTrue(makeCredResponse.FirstHmac.Secret.SequenceEqual(getHmacResponse.First.Secret));
        Assert.IsTrue(makeCredResponse.SecondHmac.Secret.SequenceEqual(getHmacResponse.Second.Secret));
    }

    private static HmacSecret DefaultSalt => new HmacSecret(
        [
            0xDE, 0xAD, 0xBE, 0xEF, 0xCA, 0xFE, 0xF0, 0x0D,
            0xDE, 0xAD, 0xBE, 0xEF, 0xCA, 0xFE, 0xF0, 0x0D,
            0xDE, 0xAD, 0xBE, 0xEF, 0xCA, 0xFE, 0xF0, 0x0D,
            0xDE, 0xAD, 0xBE, 0xEF, 0xCA, 0xFE, 0xF0, 0x0D
        ]);

    private static HmacSecret ZeroSalt => new HmacSecret(new byte[32]);
}
