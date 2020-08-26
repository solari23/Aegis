using Aegis.Models;

namespace Aegis.Core.CredentialsInterface
{
    /// <summary>
    /// Interface for components that provide an interface for users to input secrets.
    /// </summary>
    public interface IUserSecretEntryInterface
    {
        /// <summary>
        /// Gets the <see cref="SecretKind"/> provided by this provider.
        /// </summary>
        SecretKind ProvidedSecretKind { get; }

        /// <summary>
        /// Queries the secret provider to check if it can provide a secret based
        /// on that secret's metadata.
        /// </summary>
        /// <param name="secretMetadata">The metadata of the secret to check for.</param>
        /// <returns>True if the provider can retrieve the secret, false otherwise.</returns>
        bool CanProvideSecret(SecretMetadata secretMetadata);

        /// <summary>
        /// Primary interface for retrieving secret values from user input.
        /// </summary>
        /// <param name="secretMetadata">The metadata of the secret to retrieve.</param>
        /// <returns>The user secret.</returns>
        /// <remarks>
        /// Implementations should throw <see cref="SecretInterfaceOperationCancelledException"/>
        /// to indicate that the user cancelled the operation without entering the secret.
        /// </remarks>
        RawUserSecret GetUserSecret(SecretMetadata secretMetadata);

        /// <summary>
        /// Interface to retrieve the necessary paramaters to register a new user secret.
        /// </summary>
        /// <returns>The <see cref="UserKeyAuthorizationParameters"/> for the new secret.</returns>
        /// <remarks>
        /// Implementations should throw <see cref="SecretInterfaceOperationCancelledException"/>
        /// to indicate that the user cancelled the operation without entering a secret.
        /// </remarks>
        UserKeyAuthorizationParameters GetNewKeyAuthorizationParameters();
    }
}
