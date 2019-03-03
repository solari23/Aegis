namespace Aegis
{
    using CommandLine;

    /// <summary>
    /// Container for options types of Aegis verbs (i.e. verbs that implement Aegis functionality).
    /// </summary>
    public static partial class CommandLineVerbs
    {
        [Verb("create", HelpText = "Creates a new Aegis archive.")]
        public class CreateVerbOptions : AegisVerbOptions
        {
            [Option('f', "force", Required = false, HelpText = "Forces the file to be created, even if overwriting an existing file.")]
            public bool Force { get; set; }
        }

        [Verb("open", HelpText = "Unlocks an Aegis archive and enters a REPL mode to interact with it.")]
        public class OpenVerbOptions : AegisVerbOptions
        {
            // Empty -- verb only requires the default options for the archive path.
        }

        /// <summary>
        /// Base class containing common options for all Aegis verbs.
        /// </summary>
        public abstract class AegisVerbOptions
        {
            [Value(
                0,
                Required = false,
                MetaName = "Archive",
                HelpText = "The path to the target Aegis archive.")]
            public string AegisArchivePath { get; set; }

            /// <summary>
            /// Generates a string representation of the verb command.
            /// </summary>
            /// <returns>The string representation of the command.</returns>
            public override string ToString() => Parser.Default.FormatCommandLine(this);
        }
    }
}
