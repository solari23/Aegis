namespace Aegis
{
    using System;

    using Aegis.Core;

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
            Console.WriteLine("Hello world");

            var archive = SecureArchive.CreateNew();
            Console.WriteLine($"Archive {(Guid)archive.Id} created on {archive.CreateTime}.");

            var serialized = BondHelpers.Serialize(archive);
            archive = BondHelpers.Deserialize<SecureArchive>(serialized);
            Console.WriteLine($"Deserialized archive {(Guid)archive.Id} created on {archive.CreateTime}.");
        }
    }
}
