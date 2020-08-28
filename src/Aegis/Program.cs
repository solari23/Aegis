using System;

namespace Aegis
{
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
                    args = new string[] { ReplHelpers.StartReplVerb };
                }

                // If we ever implement a UI, we'll expect a single input argument with the
                // path to the archive to open. For now we just have the CLI implementation :)
                var ioStreams = new IOStreamSet(Console.Out, Console.Error, Console.In);
                var aegis = new AegisInterface(ioStreams);
                aegis.Run(args);
            }
            catch (AegisUserErrorException e)
            {
                Console.Error.WriteLine($"[Error] {e.Message}");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"[INTERNAL ERROR] {Name} ran into internal issues.{Environment.NewLine}Error details: {e}");
            }
        }
    }
}
