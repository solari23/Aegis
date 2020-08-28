using System;
using System.Text;

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
        /// <param name="isConfidentialInput">
        /// Whether or not the input is confidential. If so, the input is not displayed as it is typed on the console.
        /// </param>
        /// <param name="options">An optional value validator.</param>
        public InputPrompt(
            IOStreamSet ioStreamSet,
            string prompt,
            bool isConfidentialInput = false,
            Func<string, bool> validator = null)
        {
            ArgCheck.NotNull(ioStreamSet, nameof(IO));
            ArgCheck.NotEmpty(prompt, nameof(prompt));

            this.IO = ioStreamSet;
            this.Prompt = prompt;
            this.IsConfidentialInput = isConfidentialInput;
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
        /// Gets whether or not the input is confidential.
        /// </summary>
        private bool IsConfidentialInput { get; }

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

                var input = this.IsConfidentialInput
                    ? this.GetConfidentialInput()
                    : this.IO.In.ReadLine();

                if (!string.IsNullOrWhiteSpace(input)
                    && (this.Validator is null || this.Validator.Invoke(input)))
                {
                    value = input;
                }
            }
            while (value is null);

            return value;
        }

        /// <summary>
        /// Helper that gathers input from the console while hiding the keystrokes being typed.
        /// </summary>
        /// <returns>The entered value.</returns>
        private string GetConfidentialInput()
        {
            ConsoleKey key;
            var input = new StringBuilder();

            do
            {
                var keyInfo = this.IO.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && input.Length > 0)
                {
                    this.IO.Out.Write("\b \b");
                    input.Length--;
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    this.IO.Out.Write("*");
                    input.Append(keyInfo.KeyChar);
                }
            }
            while (key != ConsoleKey.Enter);

            // Output the linebreak for the enter key being hit.
            this.IO.Out.WriteLine();

            return input.ToString();
        }
    }
}
