using System;
using System.IO;

using Aegis.Core;

namespace Aegis
{
    /// <summary>
    /// A collection of streams for IO.
    /// </summary>
    public class IOStreamSet
    {
        /// <summary>
        /// Delegate for reading a keystroke from the console.
        /// </summary>
        /// <param name="intercept">Whether or not to intercept the keystrokes. If true, the keystroke is not displayed.</param>
        /// <returns>The keystroke that was read.</returns>
        public delegate ConsoleKeyInfo ReadConsoleKeyDelegate(bool intercept);

        /// <summary>
        /// Initializes a new instance of the <see cref="IOStreamSet"/> class.
        /// </summary>
        /// <param name="infoOutput">The stream to write informational output or prompts to.</param>
        /// <param name="errorOutput">The stream to write error output to.</param>
        /// <param name="input">The input stream to read character data from.</param>
        /// <param name="readKey">Function that reads individual keystrokes from the console.</param>
        public IOStreamSet(
            TextWriter infoOutput,
            TextWriter errorOutput,
            TextReader input,
            ReadConsoleKeyDelegate readKey)
        {
            ArgCheck.NotNull(infoOutput, nameof(infoOutput));
            ArgCheck.NotNull(errorOutput, nameof(errorOutput));
            ArgCheck.NotNull(input, nameof(input));
            ArgCheck.NotNull(readKey, nameof(readKey));

            this.Out = infoOutput;
            this.Error = errorOutput;
            this.In = input;
            this.ReadKey = readKey;
        }

        /// <summary>
        /// Gets the stream to write informational output or prompts to.
        /// </summary>
        public TextWriter Out { get; }

        /// <summary>
        /// Gets the stream to write error output to.
        /// </summary>
        public TextWriter Error { get; }

        /// <summary>
        /// Gets the input stream to read character data from.
        /// </summary>
        public TextReader In { get; }

        /// <summary>
        /// Gets the function that reads individual keystrokes from the console.
        /// </summary>
        public ReadConsoleKeyDelegate ReadKey { get; }
    }
}
