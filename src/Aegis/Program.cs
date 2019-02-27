namespace Aegis
{
    using System;
    using System.Text;

    using Aegis.Core;
    using Aegis.Core.Crypto;

    /// <summary>
    /// Contains the main entry point for the Aegis CLI.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The entry point for the Aegis CLI.
        /// </summary>
        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Hello world");

                var archiveCreationParams = new SecureArchiveCreationParameters(
                    "Password",
                    Encoding.UTF8.GetBytes("P@$sW3rd!"));

                var archive = SecureArchive.CreateNew(archiveCreationParams);
                Console.WriteLine($"Archive {(Guid)archive.Id} created on {archive.CreateTime}.");

                var serialized = BondHelpers.Serialize(archive);
                archive = BondHelpers.Deserialize<SecureArchive>(serialized);
                Console.WriteLine($"Deserialized archive {(Guid)archive.Id} created on {archive.CreateTime}.");

                var userKey = UserKey.DeriveFrom(
                    Encoding.UTF8.GetBytes("P@$sW3rd!"),
                    archive.KeyDerivationSalt.ToArray(),
                    archive.SecuritySettings);
                Console.WriteLine($"UserKey keyId: {userKey.KeyId}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[FATAL ERROR] {e}");
            }
        }
    }
}
