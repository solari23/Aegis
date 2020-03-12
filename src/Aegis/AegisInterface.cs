﻿using System;
using System.IO;
using System.Linq;
using System.Text;

using Aegis.Core;
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
        /// The password used for all archives.
        /// Important: This is a temporary placeholder for development.
        /// </summary>
        private const string TEMP_Password = "P@$sW3rD!!1!";

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
        /// Runs the Aegis command line interface.
        /// </summary>
        public void Run(string[] initialArgs)
        {
            var args = initialArgs;
            var optionsClasses = CommandLineHelpers.GetCommandLineVerbOptionTypes();
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
                    args = CommandLineHelpers.Prompt();
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
                    (OpenVerbOptions opts) => this.ExecuteAegisVerb(opts, this.OpenVerb, openArchiveIfNotOpened: false),
                    (CloseVerbOptions opts) => this.ExecuteAegisVerb(opts, this.CloseVerb, openArchiveIfNotOpened: false),

                    // CRUD verbs available in either CLI or REPL mode.
                    (CreateVerbOptions opts) => this.ExecuteAegisVerb(opts, this.CreateVerb, openArchiveIfNotOpened: false),

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

                    Console.WriteLine($"The requested '{verbName}' operation is not yet implemented.");
                }
            }
            while (this.InReplMode);
        }

        /// <summary>
        /// Internal wrapper to execute an Aegis verbs.
        /// </summary>
        /// <param name="options">The options to pass to the verb.</param>
        /// <param name="operation">The verb operation to execute.</param>
        /// <param name="openArchiveIfNotOpened">Whether or not to open/unlock the archive if one is not already opened.</param>
        /// <returns>
        /// Whether or not the operation was fully handled.
        /// It may be that some options for some verbs are not yet implemented.
        /// </returns>
        private bool ExecuteAegisVerb<TOptions>(
            TOptions options,
            Func<TOptions, bool> operation,
            bool openArchiveIfNotOpened = true)
            where TOptions : AegisVerbOptions
        {
            bool isHandled = true;

            try
            {
                if (this.Archive is null && openArchiveIfNotOpened)
                {
                    this.Archive = OpenArchive(options.AegisArchivePath, this.TempDirectory);
                }

                isHandled = operation(options);
            }
            catch (AegisUserErrorException e) when (e.IsRecoverable && this.InReplMode)
            {
                // Only catch recoverable errors while in REPL mode.
                // Let unrecoverable errors bubble up to the top-level error handler.
                Console.Error.WriteLine($"[Error] {e.Message}");
            }

            return isHandled;
        }

        /// <summary>
        /// Helper that opens (loads and unlocks) an <see cref="AegisArchive"/> from disk.
        /// </summary>
        /// <param name="archivePath">The path to the <see cref="AegisArchive"/> on disk.</param>
        /// <param name="tempDirectory">The temp directory that the archive can use for operations.</param>
        /// <returns>The opened <see cref="AegisArchive"/>.</returns>
        private static AegisArchive OpenArchive(string archivePath, string tempDirectory)
        {
            if (string.IsNullOrWhiteSpace(archivePath))
            {
                throw new AegisUserErrorException(
                    "Specify the path to the Aegis archive to open. Check the 'open' command help for details.");
            }

            AegisArchive archive;

            try
            {
                var archiveFileSettings = new SecureArchiveFileSettings(archivePath, tempDirectory);
                archive = AegisArchive.Load(archiveFileSettings);

                // TODO: Implement credential selection and input
                var rawUserSecret = Encoding.UTF8.GetBytes(TEMP_Password);

                archive.Unlock(rawUserSecret);
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
            catch (UnauthorizedException e)
            {
                throw new AegisUserErrorException(
                    $"The key was not able to unlock the archive.",
                    innerException: e);
            }

            return archive;
        }
    }
}
