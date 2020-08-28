using System;
using System.IO;
using System.Linq;

using Aegis.Core;
using Aegis.Core.CredentialsInterface;
using Aegis.CredentialsInterface;

using CommandLine;

using static Aegis.CommandLineVerbs;

namespace Aegis
{
    /// <summary>
    /// Implements the Aegis command line interface.
    /// </summary>
    public partial class AegisInterface
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AegisInterface"/> class.
        /// </summary>
        /// <param name="ioStreamSet">The IO streams.</param>
        public AegisInterface(IOStreamSet ioStreamSet)
        {
            ArgCheck.NotNull(ioStreamSet, nameof(ioStreamSet));

            this.IO = ioStreamSet;

            this.ArchiveUnlocker = new ArchiveUnlocker(
                new SecretSelector(ioStreamSet),
                new PasswordEntryInterface(ioStreamSet));
        }

        /// <summary>
        /// Gets the temporary directory where secured files can be checked out.
        /// </summary>
        private string TempDirectory { get; } = Path.GetTempPath();

        /// <summary>
        /// The reference to the <see cref="AegisArchive"/> that is opened.
        /// </summary>
        private AegisArchive Archive { get; set; }

        /// <summary>
        /// Flag that indicates we're running in REPL mode.
        /// </summary>
        private bool InReplMode { get; set; }

        /// <summary>
        /// Gets the IO streams.
        /// </summary>
        private IOStreamSet IO { get; }

        /// <summary>
        /// Gets the archive unlocking helper.
        /// </summary>
        private ArchiveUnlocker ArchiveUnlocker { get; }

        /// <summary>
        /// Runs the Aegis command line interface.
        /// </summary>
        public void Run(string[] initialArgs)
        {
            var args = initialArgs;
            var optionsClasses = ReplHelpers.GetCommandLineVerbOptionTypes();
            var commandParser = new Parser(settings =>
            {
                settings.AutoHelp = true;
                settings.AutoVersion = true;
                settings.CaseSensitive = false;
                settings.CaseInsensitiveEnumValues = true;
                settings.IgnoreUnknownArguments = false;
                settings.HelpWriter = Parser.Default.Settings.HelpWriter;
            });

            // No arguments given -> enter REPL mode.
            this.InReplMode = args is null || args.Length == 0;

            do
            {
                if (this.InReplMode)
                {
                    // In REPL mode -> Prompt for the next command.
                    var prompt = this.Archive is null 
                        ? null
                        : ReplHelpers.MakePromptString(this.Archive.FileName);
                    args = ReplHelpers.Prompt(prompt);
                }

                var parserResult = commandParser.ParseArguments(args, optionsClasses);

                if (parserResult.Tag == ParserResultType.NotParsed)
                {
                    // Parser was unable to parse the user's intent.
                    // The parsing operation will automatically display help text.
                    continue;
                }

                // Map the parser result to an operation that implements it.
                // The operation should return 'true' if the request was handled and 'false' otherwise.
                // Note: The CommandLine library has a limit to how many handlers can be passed to the MapResult
                //       method so we split out "Aegis" and "Meta" verbs into two MapResult calls.
                //       "Aegis verbs" are verbs that implement functionality around Aegis archives.
                //       "Meta verbs" are hidden verbs that implement basic command line functionality for REPL mode (like clear screen).

                var isHandled = false;

                // First check handlers for Meta verbs.
                isHandled = parserResult.MapResult(
                    (StartReplVerbOptions _) => this.StartRepl(),
                    (ExitReplVerbOptions _) => this.ExitRepl(),
                    (QuitReplVerbOptions _) => this.ExitRepl(),
                    (ClearVerbOptions _) => this.ClearScreen(),
                    (ClsVerbOptions _) => this.ClearScreen(),

                    // Default catch-all case. The command could be an Aegis verb or possibly not implemented.
                    (object opts) => false,

                    // Case when the verb could not be parsed.
                    // We shouldn't hit this because of checks above, but just in case -> treat as "handled".
                    _ => true);

                if (isHandled)
                {
                    // We're good for this iteration.
                    continue;
                }

                // Check handlers for Aegis verbs.
                isHandled = parserResult.MapResult(
                    // Verbs that specifically handle dealing with archives in REPL mode.
                    (OpenVerbOptions opts) => this.ExecuteAegisVerb(opts, this.OpenVerb, requireOpenArchive: false),
                    (CloseVerbOptions opts) => this.ExecuteAegisVerb(opts, this.CloseVerb, requireOpenArchive: false),

                    // CRUD verbs available in either CLI or REPL mode.
                    (CreateVerbOptions opts) => this.ExecuteAegisVerb(opts, this.CreateVerb, requireOpenArchive: false),
                    (AddVerbOptions opts) => this.ExecuteAegisVerb(opts, this.AddVerb),
                    (RemoveVerbOptions opts) => this.ExecuteAegisVerb(opts, this.RemoveVerb),
                    (ExtractVerbOptions opts) => this.ExecuteAegisVerb(opts, this.ExtractVerb),
                    (UpdateVerbOptions opts) => this.ExecuteAegisVerb(opts, this.UpdateVerb),
                    (ListVerbOptions opts) => this.ExecuteAegisVerb(opts, this.ListVerb, requireUnlockedArchive: opts.IsListTarget(ListVerbTargetType.Files)),
                    (AuthorizeVerbOptions opts) => this.ExecuteAegisVerb(opts, this.AuthorizeVerb),
                    (RevokeVerbOptions opts) => this.ExecuteAegisVerb(opts, this.RevokeVerb),

                    // Default catch-all case. The command is just not implemented yet.
                    (object opts) => false,

                    // Case when the verb could not be parsed.
                    // We shouldn't hit this because of checks above, but just in case -> treat as "handled".
                    _ => true);

                // Emit an error message if the request handler isn't implemented yet.
                if (!isHandled)
                {
                    // Reflect out the name of the requested verb.
                    var verbName = parserResult
                        .TypeInfo
                        .Current
                        .GetCustomAttributes(typeof(VerbAttribute), inherit: true)
                        .Select(attr => (attr as VerbAttribute)?.Name)
                        .FirstOrDefault();

                    this.IO.Out.WriteLine($"The requested '{verbName}' operation is not yet implemented.");
                }
            }
            while (this.InReplMode);
        }

        /// <summary>
        /// Internal wrapper to execute an Aegis verbs.
        /// </summary>
        /// <param name="options">The options to pass to the verb.</param>
        /// <param name="operation">The verb operation to execute.</param>
        /// <param name="requireOpenArchive">Whether or not to open the archive if not already open.</param>
        /// <param name="requireUnlockedArchive">Whether or not to unlock the archive if not already unlocked.</param>
        /// <returns>
        /// Whether or not the operation was fully handled.
        /// It may be that some options for some verbs are not yet implemented.
        /// </returns>
        private bool ExecuteAegisVerb<TOptions>(
            TOptions options,
            Func<TOptions, bool> operation,
            bool requireOpenArchive = true,
            bool requireUnlockedArchive = true)
            where TOptions : AegisVerbOptions
        {
            bool isHandled = true;

            try
            {
                if (this.Archive is null && requireOpenArchive)
                {
                    this.Archive = OpenArchive(
                        options.AegisArchivePath,
                        this.TempDirectory,
                        requireUnlockedArchive ? this.ArchiveUnlocker : null);
                    this.SetWindowTitle();
                }

                if (this.Archive != null && this.Archive.IsLocked && requireUnlockedArchive)
                {
                    UnlockArchive(this.Archive, this.ArchiveUnlocker);
                }

                isHandled = operation(options);
            }
            catch (AegisUserErrorException e) when (e.IsRecoverable && this.InReplMode)
            {
                // Only catch recoverable errors while in REPL mode.
                // Let unrecoverable errors bubble up to the top-level error handler.
                this.IO.Error.WriteLine($"[Error] {e.Message}");
            }

            return isHandled;
        }

        /// <summary>
        /// Helper that opens an <see cref="AegisArchive"/> from disk.
        /// </summary>
        /// <param name="archivePath">The path to the <see cref="AegisArchive"/> on disk.</param>
        /// <param name="tempDirectory">The temp directory that the archive can use for operations.</param>
        /// <param name="archiveUnlocker">The archive unlock helper. If null, archive will be opened but left unlocked.</param>
        /// <returns>The opened <see cref="AegisArchive"/>.</returns>
        private static AegisArchive OpenArchive(string archivePath, string tempDirectory, ArchiveUnlocker archiveUnlocker)
        {
            if (string.IsNullOrWhiteSpace(archivePath))
            {
                throw new AegisUserErrorException(
                    "Specify the path to the Aegis archive to open. Use the '-a' option or check the 'open' command help for details.");
            }

            AegisArchive archive = null;
            var openSuccess = false;

            try
            {
                var archiveFileSettings = new SecureArchiveFileSettings(archivePath, tempDirectory);
                archive = AegisArchive.Load(archiveFileSettings);

                if (archiveUnlocker != null)
                {
                    UnlockArchive(archive, archiveUnlocker);
                }

                openSuccess = true;
            }
            catch (IOException e)
            {
                throw new AegisUserErrorException(
                    $"Unable to read file at {archivePath}.",
                    innerException: e);
            }
            catch (ArchiveCorruptedException e)
            {
                throw new AegisUserErrorException(
                    $"The archive file at {archivePath} is corrupted: {e.Message}",
                    innerException: e);
            }
            finally
            {
                if (!openSuccess)
                {
                    // Make sure to release any holds if we couldn't successfully open the archive.
                    archive?.Dispose();
                }
            }

            return archive;
        }

        /// <summary>
        /// Helper that unlocks the <see cref="AegisArchive"/> using the given <see cref="ArchiveUnlocker"/>.
        /// </summary>
        /// <param name="archive">The archive to unlock.</param>
        /// <param name="unlocker">The unlocking helper.</param>
        private static void UnlockArchive(AegisArchive archive, ArchiveUnlocker unlocker)
        {
            try
            {
                unlocker.Unlock(archive);
            }
            catch (ArchiveCorruptedException e)
            {
                throw new AegisUserErrorException(
                    $"The archive file at {archive.FullFilePath} is corrupted: {e.Message}",
                    innerException: e);
            }
            catch (UnauthorizedException e)
            {
                throw new AegisUserErrorException(
                    $"The key was not able to unlock the archive.",
                    innerException: e);
            }
        }
    }
}
