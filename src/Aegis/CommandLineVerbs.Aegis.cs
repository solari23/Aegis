using CommandLine;

namespace Aegis
{
    /// <summary>
    /// Container for options types of Aegis verbs (i.e. verbs that implement Aegis functionality).
    /// </summary>
    public static partial class CommandLineVerbs
    {
        [Verb("create", HelpText = "Creates a new Aegis archive.")]
        public class CreateVerbOptions : AegisVerbOptions
        {
            // Hide the default '-a' parameter for the archive path; make it positional instead.
            // That way the user can just type "create foo.ags".
            [Value(0, Required = false, MetaName = "Archive", HelpText = "The path to the target Aegis archive.")]
            new public string AegisArchivePath { get; set; }

            [Option('f', "force", Required = false, HelpText = "Forces the file to be created, even if overwriting an existing file.")]
            public bool Force { get; set; }
        }

        [Verb("open", HelpText = "Unlocks an Aegis archive and enters a REPL mode to interact with it.")]
        public class OpenVerbOptions : AegisVerbOptions
        {
            // Hide the default '-a' parameter for the archive path; make it positional instead.
            // That way the user can just type "open foo.ags".
            [Value(0, Required = false, MetaName = "Archive", HelpText = "The path to the target Aegis archive.")]
            new public string AegisArchivePath { get; set; }

            [Option('f', "force", Required = false, HelpText = "Forces the archive to be opened, even if another archive is already open (REPL mode only).")]
            public bool Force { get; set; }
        }

        [Verb("close", HelpText = "Closes the opened archive when in REPL mode.")]
        public class CloseVerbOptions : AegisVerbOptions
        {
            // No specific options.
        }

        /// <summary>
        /// Base class containing common options for all Aegis verbs.
        /// </summary>
        public abstract class AegisVerbOptions
        {
            [Option('a', "archive", Required = false, HelpText = "The path to the target Aegis archive.")]
            public string AegisArchivePath { get; set; }

            /// <summary>
            /// Generates a string representation of the verb command.
            /// </summary>
            /// <returns>The string representation of the command.</returns>
            public override string ToString() => Parser.Default.FormatCommandLine(this);
        }
    }
}
