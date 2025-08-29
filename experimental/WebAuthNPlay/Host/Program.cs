using Aegis.Passkeys;

namespace WebAuthNPlay
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var pkman = new PasskeyManager();
            //pkman.Do();
            pkman.Do2();
            return;

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
