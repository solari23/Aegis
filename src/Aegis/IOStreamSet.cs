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
        /// Initializes a new instance of the <see cref="IOStreamSet"/> class.
        /// </summary>
        /// <param name="infoOutput">The stream to write informational output or prompts to.</param>
        /// <param name="errorOutput">The stream to write error output to.</param>
        /// <param name="input">The input stream to read character data from.</param>
        public IOStreamSet(TextWriter infoOutput, TextWriter errorOutput, TextReader input)
        {
            ArgCheck.NotNull(infoOutput, nameof(infoOutput));
            ArgCheck.NotNull(errorOutput, nameof(errorOutput));
            ArgCheck.NotNull(input, nameof(input));

            this.Out = infoOutput;
            this.Error = errorOutput;
            this.In = input;
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
    }
}
