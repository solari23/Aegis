using CommandLine;

namespace Aegis
{
    /// <summary>
    /// Container for options types of Aegis verbs (i.e. verbs that implement Aegis functionality).
    /// </summary>
    public static partial class CommandLineVerbs
    {
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

        [Verb("add", HelpText = "Adds a file to the archive.")]
        public class AddVerbOptions : AegisVerbOptions
        {
            // TODO: Define options for the 'add' verb.

            [Option('f', "force", Required = false, HelpText = "Forces the file to be added, even if overwriting an existing file.")]
            public bool Force { get; set; }
        }

        [Verb("remove", HelpText = "Removes a file from the archive.")]
        public class RemoveVerbOptions : AegisVerbOptions
        {
            // TODO: Define options for the 'remove' verb.
        }

        [Verb("update", HelpText = "Updates a file from the archive. Equivalent to using the 'add' verb with the 'force' option set.")]
        public class UpdateVerbOptions : AegisVerbOptions
        {
            // TODO: Define options for the 'update' verb.
        }

        [Verb("list", HelpText = "Lists archive files or authorized keys.")]
        public class ListVerbOptions : AegisVerbOptions
        {
            // TODO: Define options for the 'list' verb.
        }

        [Verb("authorize", HelpText = "Authorizes a new user key to access the archive.")]
        public class AuthorizeVerbOptions : AegisVerbOptions
        {
            // TODO: Define options for the 'authorize' verb.
            // This is pending fleshing out the user keys subsystem.
        }

        [Verb("revoke", HelpText = "Revokes a user key authorization.")]
        public class RevokeVerbOptions : AegisVerbOptions
        {
            // TODO: Define options for the 'revoke' verb.
            // This is pending fleshing out the user keys subsystem.
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
