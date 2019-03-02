namespace Aegis
{
    using CommandLine;

    /// <summary>
    /// Static container for CLI verb option containers.
    /// </summary>
    public static class AegisCommandLineVerbs
    {
        [Verb("create", HelpText = "Creates a new Aegis archive.")]
        public class CreateVerbOptions : AegisVerbOptions
        {
            // Empty -- just needs default options for the archive path.
        }

        [Verb("open", HelpText = "Unlocks an Aegis archive and enters a REPL mode to interact with it.")]
        public class OpenVerbOptions : AegisVerbOptions
        {
            // Empty -- just needs default options for the archive path.
        }

        [Verb(CommandLineHelpers.StartReplVerb, Hidden = true, HelpText = "Hidden verb that allows the user the start REPL mode.")]
        public class StartReplVerbOptions
        {
            // Empty. Options are not required; this is just a signal to start REPL mode.
        }

        [Verb("close", Hidden = true, HelpText = "Hidden verb that allows the user the exit REPL mode.")]
        public class CloseReplVerbOptions
        {
            // Empty. Options are not required; this is just a signal to exit REPL mode.
        }

        [Verb("quit", Hidden = true, HelpText = "Hidden verb that allows the user the exit REPL mode.")]
        public class QuitReplVerbOptions
        {
            // Empty. Options are not required; this is just a signal to exit REPL mode.
        }

        [Verb("exit", Hidden = true, HelpText = "Hidden verb that allows the user to exit REPL mode.")]
        public class ExitReplVerbOptions
        {
            // Empty. Options are not required; this is just a signal to exit REPL mode.
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
