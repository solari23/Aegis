using System.Collections.Immutable;
using System.IO;

using Aegis.Core;
using Aegis.Core.CredentialsInterface;
using Aegis.Models;

namespace Aegis.CredentialsInterface
{
    /// <summary>
    /// Implementation of <see cref="IUserSecretSelector"/> for selecting a secret from the command line.
    /// </summary>
    public class SecretSelector : IUserSecretSelector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecretSelector"/> class.
        /// </summary>
        /// <param name="input">The input stream to read characters from.</param>
        /// <param name="output">The output stream to write prompts to.</param>
        public SecretSelector(TextReader input, TextWriter output)
        {
            ArgCheck.NotNull(input, nameof(input));
            ArgCheck.NotNull(output, nameof(output));

            this.Input = input;
            this.Output = output;
        }

        /// <summary>
        /// Gets the input stream to read characters from.
        /// </summary>
        public TextReader Input { get; }

        /// <summary>
        /// Gets the output stream to write prompts to.
        /// </summary>
        public TextWriter Output { get; }

        /// <inheritdoc />
        public SecretKind PromptSelectSecretKind(ImmutableArray<SecretKind> availableSecretKinds)
        {
            // Only passwords are currently supported. No need to prompt.
            return SecretKind.Password;
        }
    }
}
