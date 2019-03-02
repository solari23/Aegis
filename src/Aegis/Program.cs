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
        /// The name of the program.
        /// </summary>
        public const string Name = "Aegis";

        /// <summary>
        /// The entry point for the Aegis application.
        /// </summary>
        public static void Main(string[] args)
        {
            try
            {
                if (args is null || args.Length == 0)
                {
                    // No arguments -> signal to start REPL mode.
                    args = new string[] { CommandLineHelpers.StartReplVerb };
                }

                // If we ever implement a UI, we'll expect a single input argument with the
                // path to the archive to open. For now we just have the CLI implementation :)
                AegisInterface aegis = new AegisInterface();
                aegis.Run(args);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"[INTERNAL ERROR] {Name} ran into internal issues.{Environment.NewLine}Error details: {e}");
            }
        }
    }
}
