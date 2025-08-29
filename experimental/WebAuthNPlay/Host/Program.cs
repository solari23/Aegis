using System.Text;

using Aegis.Passkeys;

namespace WebAuthNPlay
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MakeCredential();
            GetHmacSecret();
        }

        static Identifier MakeCredential()
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
                Id = new Identifier(Encoding.UTF8.GetBytes("alker@alkerTestRP.local")),
                Name = "alker@alkerTestRP.local",
                DisplayName = "Alker User",
            };
            var makeCredResponse = pkman.MakeCredentialWithHmacSecret(rpInfo, userInfo);
            return makeCredResponse.NewCredentialId;
        }

        static void GetHmacSecret()
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
            using var hmacSecretResponse = pkman.GetHmacSecret(rpInfo, hmacSalt);
        }
    }
}
