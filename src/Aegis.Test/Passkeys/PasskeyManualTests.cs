using Aegis.Passkeys;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aegis.Test.Passkeys;

[TestClass]
public class PasskeyManualTests
{
    [TestMethod]
    //[Ignore("Manual test - run only during active development.")]
    public void TestMakeCredential()
    {
        var passkeyManager = new PasskeyManager();

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
            salt: DefaultSalt);
    }

    private static HmacSecret DefaultSalt => new HmacSecret(
        [
            0xDE, 0xAD, 0xBE, 0xEF, 0xCA, 0xFE, 0xF0, 0x0D,
            0xDE, 0xAD, 0xBE, 0xEF, 0xCA, 0xFE, 0xF0, 0x0D,
            0xDE, 0xAD, 0xBE, 0xEF, 0xCA, 0xFE, 0xF0, 0x0D,
            //0xDE, 0xAD, 0xBE, 0xEF, 0xCA, 0xFE, 0xF0, 0x0D
        ]);
}
