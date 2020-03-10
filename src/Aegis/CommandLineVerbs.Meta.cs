using CommandLine;

namespace Aegis
{
    /// <summary>
    /// Container for options types of Meta verbs (i.e. verbs that implement basic command line functionality).
    /// </summary>
    public static partial class CommandLineVerbs
    {
        [Verb(CommandLineHelpers.StartReplVerb, Hidden = true, HelpText = "Hidden verb that allows the user the start REPL mode.")]
        public class StartReplVerbOptions
        {
            // Empty.
        }

        [Verb("quit", Hidden = true, HelpText = "Hidden verb that allows the user the exit REPL mode.")]
        public class QuitReplVerbOptions
        {
            // Empty.
        }

        [Verb("exit", Hidden = true, HelpText = "Hidden verb that allows the user to exit REPL mode.")]
        public class ExitReplVerbOptions
        {
            // Empty.
        }

        [Verb("clear", Hidden = true, HelpText = "Hidden verb that allows the user to clear the screen.")]
        public class ClearVerbOptions
        {

        }

        [Verb("cls", Hidden = true, HelpText = "Hidden verb that allows the user to clear the screen.")]
        public class ClsVerbOptions
        {

        }
    }
}
