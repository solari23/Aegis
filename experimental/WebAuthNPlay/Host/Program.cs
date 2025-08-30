using System.Text;

using Aegis.Passkeys;

namespace WebAuthNPlay
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var cred = MakeCredential("foo");
            //GetHmacSecret([cred]);
            //return;

            var cred1 = MakeCredential("foo");
            var cred2 = MakeCredential("bar");
            var cred3 = MakeCredential("baz");
            GetHmacSecret([cred1]);
            GetHmacSecret([cred1, cred2]);
            GetHmacSecret(null);
        }

        static Identifier MakeCredential(string userIdSalt)
        {
            var pkman = new PasskeyManager();
            var rpInfo = new RelyingPartyInfo
            {
                Id = "alkerTestRP.local",
                DisplayName = "Alker Test RP",
                Origin = "https://alkerTestRP.local",
            };
            var userInfo = new UserEntityInfo
            {
                Id = new Identifier(Encoding.UTF8.GetBytes("alker@alkerTestRP.local" + userIdSalt)),
                Name = "alker@alkerTestRP.local",
                DisplayName = "Alker User",
            };
            var makeCredResponse = pkman.MakeCredentialWithHmacSecret(rpInfo, userInfo);
            return makeCredResponse.NewCredentialId;
        }

        static void GetHmacSecret(Identifier[] credId)
        {
            var pkman = new PasskeyManager();
            using var hmacSalt = new HmacSecret(
                [
                    0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF,
                    0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF,
                ]);
            var rpInfo = new RelyingPartyInfo
            {
                Id = "alkerTestRP.local",
                Origin = "https://alkerTestRP.local",
            };
            using var hmacSecretResponse = pkman.GetHmacSecret(rpInfo, hmacSalt, allowedCredentialIds: credId);
        }
    }
}
