using System.Collections.Immutable;
using System.Text;

using Aegis.Core;
using Aegis.Core.CredentialsInterface;
using Aegis.Models;

namespace Aegis.CredentialsInterface
{
    /// <summary>
    /// Implementation of <see cref="IUserSecretEntryInterface"/> for entering passwords on the command line.
    /// </summary>
    public class PasswordEntryInterface : IUserSecretEntryInterface
    {
        /// <summary>
        /// The password used for all archives.
        /// Important: This is a temporary placeholder for development.
        /// </summary>
        private const string TEMP_Password = "P@$sW3rD!!1!";

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordEntryInterface"/> class.
        /// </summary>
        /// <param name="ioStreamSet">The IO streams.</param>
        public PasswordEntryInterface(IOStreamSet ioStreamSet)
        {
            ArgCheck.NotNull(ioStreamSet, nameof(ioStreamSet));

            this.IO = ioStreamSet;
        }

        /// <inheritdoc />
        public SecretKind ProvidedSecretKind => SecretKind.Password;

        /// <summary>
        /// Gets the IO streams.
        /// </summary>
        private IOStreamSet IO { get; }

        /// <inheritdoc />
        public bool CanProvideSecret(SecretMetadata secretMetadata) => true;

        /// <inheritdoc />
        public UserKeyAuthorizationParameters GetNewKeyAuthorizationParameters()
        {
            // TODO: Implement prompting for the friendly name and password
            var password = this.GetUserSecret(ImmutableArray<SecretMetadata>.Empty);
            return new UserKeyAuthorizationParameters(password)
            {
                FriendlyName = "Password",
                SecretMetadata = new PasswordSecretMetadata(),
            };
        }

        /// <inheritdoc />
        public RawUserSecret GetUserSecret(ImmutableArray<SecretMetadata> possibleSecretMetadata)
        {
            // TODO: Implement password entry on the command-line.
            return new RawUserSecret(Encoding.UTF8.GetBytes(TEMP_Password));
        }
    }
}
