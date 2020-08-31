using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Aegis.Core;

namespace Aegis.CredentialsInterface
{
    /// <summary>
    /// Helper that prompts the user to confirm an operation.
    /// </summary>
    public class ConfirmationPrompt
    {
        /// <summary>
        /// Maps potential confirmation responses to whether or not they indicate confirmation.
        /// </summary>
        private static ReadOnlyDictionary<string, bool> ConfirmationResponseStrings { get; }
            = new ReadOnlyDictionary<string, bool>(new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
            {
                { "yes", true },
                { "y", true },
                { "no", false },
                { "n", false },
            });

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmationPrompt"/> class.
        /// </summary>
        /// <param name="ioStreamSet">The IO streams.</param>
        /// <param name="confirmationDetails">The details to confirm.</param>
        public ConfirmationPrompt(
            IOStreamSet ioStreamSet,
            string confirmationDetails)
        {
            ArgCheck.NotNull(ioStreamSet, nameof(ioStreamSet));
            ArgCheck.NotEmpty(confirmationDetails, nameof(confirmationDetails));

            this.IO = ioStreamSet;
            this.ConfirmationDetails = confirmationDetails;
        }

        /// <summary>
        /// Gets the IO streams.
        /// </summary>
        private IOStreamSet IO { get; }

        /// <summary>
        /// Gets the details to confirm.
        /// </summary>
        private string ConfirmationDetails { get; }

        /// <summary>
        /// Prompts the user and returns whether or not the user selected to confirm the operation.
        /// </summary>
        /// <returns>True if the user confirmed the operation, false otherwise.</returns>
        public bool GetConfirmation()
        {
            this.IO.Out.WriteLine(this.ConfirmationDetails);

            var inputPrompt = new InputPrompt(
                this.IO,
                "Confirm? [Y]es or [No]: ",
                validator: s => ConfirmationResponseStrings.ContainsKey(s));

            return ConfirmationResponseStrings[inputPrompt.GetValue()];
        }
    }
}
