using System;

using CommandLine;

using ListVerbTargetType = Aegis.AegisInterface.ListVerbTargetType;

namespace Aegis
{
    /// <summary>
    /// Container for options types of Aegis verbs (i.e. verbs that implement Aegis functionality).
    /// </summary>
    public static class CommandLineVerbs
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
            [Value(0, Required = true, MetaName = "FilePath", HelpText = "The path to the file to add to the archive.")]
            public string FilePath { get; set; }

            [Option('v', "vpath", Required = false, HelpText = "The virtual path in the archive to add the file at. If not specified, the file will be added at the archive root with the same file name.")]
            public string ArchiveVirtualPath { get; set; }

            [Option('f', "force", Required = false, HelpText = "Forces the file to be added, even if overwriting an existing file.")]
            public bool Force { get; set; }
        }

        [Verb("remove", HelpText = "Removes a file from the archive.")]
        public class RemoveVerbOptions : AegisVerbOptions
        {
            // The user must choose exactly one of ArchiveVirtualPath or ArchiveFileId.
            // This is enforced by setting them in the same "Group" (at least one of a group must be provided)
            // but with different "SetName" values (can't specify options from more than one set).

            [Option('v', "vpath", SetName = "UpdateTarget_VPATH", Group = "UpdateTarget", HelpText = "The virtual path in the archive of the file to update.")]
            public string ArchiveVirtualPath { get; set; }

            [Option('i', "id", SetName = "UpdateTarget_ID", Group = "UpdateTarget", HelpText = "[GUID] The ID of the archive file to update.")]
            public Guid ArchiveFileId { get; set; }
        }

        [Verb("extract", HelpText = "Decrypts and extracts a file from the archive.")]
        public class ExtractVerbOptions : AegisVerbOptions
        {
            // The user must choose exactly one of ArchiveVirtualPath or ArchiveFileId.
            // This is enforced by setting them in the same "Group" (at least one of a group must be provided)
            // but with different "SetName" values (can't specify options from more than one set).

            [Option('v', "vpath", SetName = "ExtractTarget_VPATH", Group = "ExtractTarget", HelpText = "The virtual path in the archive of the file to extract.")]
            public string ArchiveVirtualPath { get; set; }

            [Option('i', "id", SetName = "ExtractTarget_ID", Group = "ExtractTarget", HelpText = "[GUID] The ID of the archive file to extract.")]
            public Guid ArchiveFileId { get; set; }

            [Option('o', "out", Required = false, HelpText = "The directory to which the file will be extracted. Defaults to current directory.")]
            public string OutputDirectoryPath { get; set; }
        }

        [Verb("update", HelpText = "Updates a file in the archive. Equivalent to using the 'add' verb with the 'force' option set.")]
        public class UpdateVerbOptions : AegisVerbOptions
        {
            [Value(0, Required = true, MetaName = "FilePath", HelpText = "The path to the file to add to the archive.")]
            public string FilePath { get; set; }

            // The user must choose exactly one of ArchiveVirtualPath or ArchiveFileId.
            // This is enforced by setting them in the same "Group" (at least one of a group must be provided)
            // but with different "SetName" values (can't specify options from more than one set).

            [Option('v', "vpath", SetName = "UpdateTarget_VPATH", Group = "UpdateTarget", HelpText = "The virtual path in the archive of the file to update.")]
            public string ArchiveVirtualPath { get; set; }

            [Option('i', "id", SetName = "UpdateTarget_ID", Group = "UpdateTarget", HelpText = "[GUID] The ID of the archive file to update.")]
            public Guid ArchiveFileId { get; set; }
        }

        [Verb("list", HelpText = "Lists archive files or authorized keys.")]
        public class ListVerbOptions : AegisVerbOptions
        {
            [Value(0, Required = true, MetaName = "<Files | Keys>", HelpText = "Indicates whether to list files or authorized keys.")]
            public string ListType { get; set; }

            public bool IsListTarget(ListVerbTargetType target) =>
                Enum.TryParse<ListVerbTargetType>(this.ListType, ignoreCase: true, out var listTargetType)
                && listTargetType == target;
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

        #region REPL control verbs

        [Verb(ReplHelpers.StartReplVerb, Hidden = true, HelpText = "Hidden verb that allows the user the start REPL mode.")]
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
            // Empty.
        }

        [Verb("cls", Hidden = true, HelpText = "Hidden verb that allows the user to clear the screen.")]
        public class ClsVerbOptions
        {
            // Empty.
        }

        #endregion
    }
}
