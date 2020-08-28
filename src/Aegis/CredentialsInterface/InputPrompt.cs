using System;

using Aegis.Core;

namespace Aegis.CredentialsInterface
{
    /// <summary>
    /// Helper that prompts the user until a valid value is entered.
    /// </summary>
    public class InputPrompt
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Menu"/> class.
        /// </summary>
        /// <param name="ioStreamSet">The IO streams.</param>
        /// <param name="prompt">The prompt to display.</param>
        /// <param name="options">An optional value validator.</param>
        public InputPrompt(
            IOStreamSet ioStreamSet,
            string prompt,
            Func<string, bool> validator = null)
        {
            ArgCheck.NotNull(ioStreamSet, nameof(IO));
            ArgCheck.NotEmpty(prompt, nameof(prompt));

            this.IO = ioStreamSet;
            this.Prompt = prompt;
            this.Validator = validator;
        }

        /// <summary>
        /// Gets the IO streams.
        /// </summary>
        private IOStreamSet IO { get; }

        /// <summary>
        /// Gets the prompt to display.
        /// </summary>
        private string Prompt { get; }

        /// <summary>
        /// Gets the optional value validator.
        /// </summary>
        private Func<string, bool> Validator { get; }

        /// <summary>
        /// Displays the prompt and returns the value entered by the user.
        /// </summary>
        /// <returns>The value entered by the user.</returns>
        public string GetValue()
        {
            string value = null;

            do
            {
                this.IO.Out.Write(this.Prompt);

                var input = this.IO.In.ReadLine();

                if (!string.IsNullOrWhiteSpace(input)
                    && (this.Validator is null || this.Validator.Invoke(input)))
                {
                    value = input;
                }
            }
            while (value is null);

            return value;
        }
    }
}
