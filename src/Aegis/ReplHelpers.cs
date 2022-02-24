using System.Text;

using CommandLine;

namespace Aegis;

/// <summary>
/// A collection of static helper utilities for implementing the command line REPL.
/// </summary>
public static class ReplHelpers
{
    /// <summary>
    /// A hidden command line verb that starts the REPL (Read-Execute-Print-Loop) mode.
    /// </summary>
    public const string StartReplVerb = "repl";

    /// <summary>
    /// Makes a prompt string to be displayed on the command line out of the given prompt.
    /// </summary>
    /// <param name="prompt">The prompt message.</param>
    /// <returns>The prompt string.</returns>
    public static string MakePromptString(string prompt) => $"{prompt}->";

    /// <summary>
    /// Gets all defined command line verbs' options types.
    /// </summary>
    /// <returns>An array containing the options types.</returns>
    public static Type[] GetCommandLineVerbOptionTypes()
    {
        var verbTypes = typeof(CommandLineVerbs)
            .GetNestedTypes()
            .Where(t => t.GetCustomAttributes(typeof(VerbAttribute), inherit: true).Any())
            .ToArray();

        return verbTypes ?? new Type[0];
    }

    /// <summary>
    /// Presents a command line prompt and gets the user's input.
    /// </summary>
    /// <param name="promptString">The prompt string to use.</param>
    /// <returns>The arguments entered by the user.</returns>
    public static string[] Prompt(string promptString = null)
    {
        promptString = promptString ?? MakePromptString(Program.Name);

        Console.Write(promptString);
        var rawInput = Console.ReadLine();

        return ParseCommandLine(rawInput).ToArray();
    }

    /// <summary>
    /// Implements a very basic state machine to parse a command line string into argument tokens.
    /// </summary>
    /// <param name="line">The line to parse.</param>
    /// <returns>The argument tokens.</returns>
    public static IEnumerable<string> ParseCommandLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            // Nothing in the string.
            yield break;
        }

        var builder = new StringBuilder();
        var inQuotes = false;
        var stop = false;

        for (int i = 0; i < line.Length && !stop; i++)
        {
            var curChar = line[i];
            var nextChar = (i + 1) < line.Length ? line[i + 1] : '\0';

            switch (curChar)
            {
                case '\\' when nextChar == '"':
                    // We found an escaped quote.
                    builder.Append('"');
                    i++;
                    break;

                case '"':
                    inQuotes = !inQuotes;
                    break;

                case ' ' when !inQuotes:
                    if (builder.Length > 0)
                    {
                        yield return builder.ToString();
                        builder.Clear();
                    }
                    break;

                case '|' when !inQuotes:
                case '>' when !inQuotes:
                case '<' when !inQuotes:
                    // We found an command pipe or stream redirect.
                    // The command string we were parsing is done.
                    stop = true;
                    break;

                default:
                    builder.Append(curChar);
                    break;
            }
        }

        // Flush anything left in the buffer.
        if (builder.Length > 0)
        {
            yield return builder.ToString();
        }
    }
}
