namespace Aegis
{
    using System;

    using static AegisCommandLineVerbs;

    /// <summary>
    /// A collection of static helper utilities for dealing with the command line.
    /// </summary>
    public static class CommandLineHelpers
    {
        /// <summary>
        /// The default command line prompt string to use in REPL mode.
        /// </summary>
        public const string DefaultReplPrompt = Program.Name + "~>";

        /// <summary>
        /// A hidden command line verb that starts the REPL (Read-Execute-Print-Loop) mode.
        /// </summary>
        public const string StartReplVerb = "repl";

        /// <summary>
        /// Gets all defined command line verbs' options types.
        /// </summary>
        /// <returns>An array containing the options types.</returns>
        public static Type[] GetCommandLineVerbOptionTypes()
        {
            // TODO: Retrieve command line verb options types using reflection.
            return new Type[]
            {
                typeof(CreateVerbOptions),
                typeof(OpenVerbOptions),
                typeof(StartReplVerbOptions),
                typeof(CloseReplVerbOptions),
                typeof(ExitReplVerbOptions),
                typeof(QuitReplVerbOptions),
            };
        }

        /// <summary>
        /// Presents a command line prompt and gets the user's input.
        /// </summary>
        /// <param name="promptString">The prompt string to use.</param>
        /// <returns>The arguments entered by the user.</returns>
        public static string[] Prompt(string promptString = DefaultReplPrompt)
        {
            Console.Write(promptString);
            var rawInput = Console.ReadLine();

            // TODO: Handle quoted strings input on command line as single arguments.
            return rawInput.Split();
        }
    }
}
