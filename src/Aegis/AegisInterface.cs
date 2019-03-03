namespace Aegis
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Aegis.Core;
    using CommandLine;

    using static CommandLineVerbs;

    /// <summary>
    /// Implements the Aegis command line interface.
    /// </summary>
    public class AegisInterface
    {
        /// <summary>
        /// Gets the temporary directory where secured files can be checked out.
        /// </summary>
        private string TempDirectory { get; } = Path.GetTempPath();

        /// <summary>
        /// The reference to the <see cref="SecureArchive"/> that is opened.
        /// </summary>
        private SecureArchive SecureArchive { get; set; }

        /// <summary>
        /// Flag to stop the program after the current command completes.
        /// Used to keep the program in a loop during REPL mode.
        /// </summary>
        private bool ExitAfterCommand { get; set; } = true;

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

            do
            {
                if (!this.ExitAfterCommand)
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
                //       "Meta verbs" are hidden verbs that implement basic command line functionality (like clear screen).

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
                    (CreateVerbOptions opts) => this.ExecuteAegisVerb(() => this.CreateVerb(opts)),
                    (OpenVerbOptions opts) => this.ExecuteAegisVerb(() => this.OpenVerb(opts)),
                    (CloseVerbOptions opts) => this.ExecuteAegisVerb(() => this.CloseVerb(opts)),

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
            while (!this.ExitAfterCommand);
        }

        /// <summary>
        /// Internal wrapper to execute an Aegis verbs.
        /// </summary>
        /// <param name="operation">The operation to execute.</param>
        /// <returns>
        /// Whether or not the operation was fully handled.
        /// It may be that some options for some verbs are not yet implemented.
        /// </returns>
        private bool ExecuteAegisVerb(Func<bool> operation)
        {
            bool isHandled = true;

            try
            {
                // TODO: Open/unlock an Aegis archive if none is yet opened.
                isHandled = operation();
            }
            catch (AegisUserErrorException e) when (e.IsRecoverable)
            {
                // Only catch recoverable errors. Let unrecoverable errors bubble up
                // to the top-level error handler.
                Console.Error.WriteLine($"[Error] {e.Message}");
            }

            return isHandled;
        }

        /// <summary>
        /// Implements the Aegis 'create' verb.
        /// </summary>
        /// <param name="options">The verb options.</param>
        /// <returns>Whether or not the operation was handled.</returns>
        private bool CreateVerb(CreateVerbOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.AegisArchivePath))
            {
                throw new AegisUserErrorException("The archive path parameter is required for this operation.");
            }

            if (File.Exists(options.AegisArchivePath) && !options.Force)
            {
                throw new AegisUserErrorException(
                    "A file exists at the specified location. Use --force flag to overwrite.");
            }

            // TODO: Implement credential selection and input
            var userKeyFriendlyName = "Password";
            var rawUserSecret = Encoding.UTF8.GetBytes("P@$sW3rD!!1!");

            var createParameters = new SecureArchiveCreationParameters(userKeyFriendlyName, rawUserSecret);
            var fileSettings = new SecureArchiveFileSettings(options.AegisArchivePath, this.TempDirectory);

            using var archive = SecureArchive.CreateNew(fileSettings, createParameters);

            try
            {
                // Create the output directory if it doesn't already exist.
                var directoryPath = Path.GetDirectoryName(options.AegisArchivePath);
                if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                archive.Save();

                var newArchiveFileInfo = new FileInfo(archive.FullFilePath);
                Console.WriteLine($"Created new secure archive file '{newArchiveFileInfo.FullName}'.");
            }
            catch (IOException e)
            {
                throw new AegisUserErrorException($"Unable to write to {fileSettings.Path}.", innerException: e);
            }

            return true;
        }

        /// <summary>
        /// Implements the Aegis 'open' verb.
        /// </summary>
        /// <param name="options">The verb options.</param>
        /// <returns>Whether or not the operation was handled.</returns>
        private bool OpenVerb(OpenVerbOptions options)
        {
            if (this.SecureArchive != null
                && this.SecureArchive.IsDirty
                && !options.Force)
            {
                throw new AegisUserErrorException(
                    "An unsaved archive is already opened. Either save the opened archive or use the --force flag.");
            }

            // If we already have an archive opened, then any errors we encounter
            // during this operation are recoverable (keep working on existing one).
            // Otherwise, load errors should kill the app.
            var areLoadErrorsRecoverable = this.SecureArchive != null;
            SecureArchive archive = null;

            try
            {
                var archiveFileSettings = new SecureArchiveFileSettings(options.AegisArchivePath, this.TempDirectory);
                archive = SecureArchive.Load(archiveFileSettings);

                // TODO: Implement credential selection and input
                var rawUserSecret = Encoding.UTF8.GetBytes("P@$sW3rD!!1!");

                archive.Unlock(rawUserSecret);
            }
            catch (IOException e)
            {
                throw new AegisUserErrorException(
                    $"Unable to read file at {options.AegisArchivePath}.",
                    isRecoverable: areLoadErrorsRecoverable,
                    innerException: e);
            }
            catch (ArchiveCorruptedException e)
            {
                throw new AegisUserErrorException(
                    $"The archive file at {options.AegisArchivePath} is corrupted.",
                    isRecoverable: areLoadErrorsRecoverable,
                    innerException: e);
            }
            catch (UnauthorizedException e)
            {
                throw new AegisUserErrorException(
                    $"The key was not able to unlock the archive.",
                    isRecoverable: areLoadErrorsRecoverable,
                    innerException: e);
            }

            // The new archive is fully loaded. Close out the previous one.
            this.CloseVerb(new CloseVerbOptions());

            this.SecureArchive = archive;
            return this.StartRepl();
        }

        /// <summary>
        /// Implements the Aegis 'close' verb.
        /// </summary>
        /// <param name="options">The verb options.</param>
        /// <returns>Whether or not the operation was handled.</returns>
        private bool CloseVerb(CloseVerbOptions options)
        {
            if (this.SecureArchive is null)
            {
                // There's no archive opened. Do nothing.
                return true;
            }

            if (this.SecureArchive.IsDirty && !options.Force)
            {
                throw new AegisUserErrorException(
                    "An archive contains unsaved content. Either save the opened archive or use the --force flag.");
            }

            this.SecureArchive?.Dispose();
            this.SecureArchive = null;
            this.SetWindowTitle();

            return true;
        }

        /// <summary>
        /// Puts the interface into REPL (Read-Execute-Print-Loop) mode.
        /// </summary>
        /// <returns>'True' to indicate the operation was handled.</returns>
        private bool StartRepl()
        {
            this.SetWindowTitle();
            this.ExitAfterCommand = false;
            return true;
        }

        /// <summary>
        /// Signals the exit flag to leave REPL mode.
        /// </summary>
        /// <returns>'True' to indicate the operation was handled.</returns>
        private bool ExitRepl()
        {
            // Safety check.
            if (this.SecureArchive != null && this.SecureArchive.IsDirty)
            {
                Console.Error.WriteLine("[Error] There is an unsaved archive currently opened.");
                Console.Error.WriteLine("Either save it or close it with the --force flag before exiting the REPL.");
                return true;
            }

            this.ExitAfterCommand = true;
            return true;
        }

        /// <summary>
        /// Clears the console screen.
        /// </summary>
        /// <returns>'True' to indicate the operation was handled.</returns>
        private bool ClearScreen()
        {
            Console.Clear();
            return true;
        }

        /// <summary>
        /// Sets the title in the command line window.
        /// </summary>
        private void SetWindowTitle()
        {
            var archiveName = this.SecureArchive is null
                ? "No Archive Selected"
                : this.SecureArchive.FileName;

            Console.Title = $"{Program.Name} <{archiveName}>";
        }
    }
}
