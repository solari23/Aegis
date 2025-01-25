namespace Aegis;

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
                args = [ReplHelpers.StartReplVerb];
            }

            var ioStreams = new IOStreamSet(
                Console.Out,
                Console.Error,
                Console.In,
                Console.ReadKey);
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
