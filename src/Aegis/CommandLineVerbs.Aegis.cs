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
            // Empty -- verb only requires the default options for the archive path.
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
            [Option(
                'a',
                "archive",
                Required = false,
                HelpText = "The path to the target Aegis archive. This parameter is ignored in REPL mode, but required for other CLI modes.")]
            public string AegisArchivePath { get; set; }
        }
    }
}
