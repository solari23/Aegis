namespace Aegis
{
    using System;
    using System.Linq;

    using CommandLine;

    using static AegisCommandLineVerbs;

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
                var parserResult = commandParser.ParseArguments(args, optionsClasses);

                // Map the parser result to an operation that implements it.
                // The operation should return 'true' if the request was handled and 'false' otherwise.
                var isHandled = parserResult.MapResult(
                    (CreateVerbOptions opts) => this.Execute(() => this.CreateVerb(opts)),
                    (OpenVerbOptions opts) => this.Execute(() => this.OpenVerb(opts)),

                    // These verbs just initiate/quit REPL mode. They don't need to be executed as Aegis operations.
                    (StartReplVerbOptions _) => this.StartRepl(),
                    (CloseReplVerbOptions _) => this.ExitRepl(),
                    (ExitReplVerbOptions _) => this.ExitRepl(),
                    (QuitReplVerbOptions _) => this.ExitRepl(),

                    // Default case for a parsed verb that doesn't have an implementation yet.
                    (object opts) => false,

                    // Case when the verb could not be parsed. Treat as "handled".
                    _ => true);

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

                if (!this.ExitAfterCommand)
                {
                    // In REPL mode -> Prompt for the next command.
                    args = CommandLineHelpers.Prompt();
                }
            }
            while (!this.ExitAfterCommand);
        }

        /// <summary>
        /// Internal wrapper to execute an Aegis operation.
        /// </summary>
        /// <param name="operation">The operation to execute.</param>
        /// <returns>
        /// Whether or not the operation was handled. It may be that some options for verbs are not yet implemented.
        /// </returns>
        private bool Execute(Func<bool> operation)
        {
            // TODO: Open/unlock an Aegis archive if none is yet opened.
            // TODO: Define an exception of type AegisUserErrorException with field IsRecoverable.
            // TODO: Catch AegisUserErrorException where IsRecoverable is true. Handle gracefully so execution continues.
            return operation();
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
    }
}
