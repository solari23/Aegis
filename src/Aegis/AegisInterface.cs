namespace Aegis
{
    using System;
    using System.Linq;

    using CommandLine;

    using static CommandLineVerbs;

    /// <summary>
    /// Implements the Aegis command line interface.
    /// </summary>
    public class AegisInterface
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AegisInterface"/> class.
        /// </summary>
        public AegisInterface()
        {
            // Empty?
        }

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
                    (CloseReplVerbOptions _) => this.ExitRepl(),
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
            // TODO [Verb]: Implement the 'create' verb.
            return false;
        }

        /// <summary>
        /// Implements the Aegis 'open' verb.
        /// </summary>
        /// <param name="options">The verb options.</param>
        /// <returns>Whether or not the operation was handled.</returns>
        private bool OpenVerb(OpenVerbOptions options)
        {
            // TODO [Verb]: Implement the 'open' verb.
            this.StartRepl();
            return false;
        }

        /// <summary>
        /// Puts the interface into REPL (Read-Execute-Print-Loop) mode.
        /// </summary>
        /// <returns>'True' to indicate the operation was handled.</returns>
        private bool StartRepl()
        {
            // TODO: In REPL mode, set the console title to contain the archive name if one is opened.
            Console.Title = $"{Program.Name} <No Archive Selected>";
            this.ExitAfterCommand = false;
            return true;
        }

        /// <summary>
        /// Signals the exit flag to leave REPL mode.
        /// </summary>
        /// <returns>'True' to indicate the operation was handled.</returns>
        private bool ExitRepl()
        {
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
    }
}
