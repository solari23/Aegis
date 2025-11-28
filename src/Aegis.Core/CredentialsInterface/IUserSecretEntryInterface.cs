using System.Collections.Immutable;

using Aegis.Core.Crypto;
using Aegis.Models;

namespace Aegis.Core.CredentialsInterface;

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
    /// Gets whether or not the current platform is supported by the <see cref="IUserSecretEntryInterface"/>.
    /// </summary>
    bool IsCurrentPlatformSupported();

    /// <summary>
    /// Queries the secret provider to check if it can provide a secret based
    /// on that secret's metadata.
    /// </summary>
    /// <param name="archiveId">The unique identifier of the secure archive.</param>
    /// <param name="secretMetadata">
    /// The metadata of the secret to check for. The secret is guaranteed to match <see cref="ProvidedSecretKind"/>.
    /// </param>
    /// <returns>True if the provider can retrieve the secret, false otherwise.</returns>
    bool CanProvideSecret(Guid archiveId, SecretMetadata secretMetadata);

    /// <summary>
    /// Primary interface for retrieving secret values from user input.
    /// </summary>
    /// <param name="archiveId">The unique identifier of the secure archive.</param>
    /// <param name="possibleSecretMetadata">
    /// The metadata for secrets that the user may choose from.
    /// These secrets are guaranteed to match <see cref="ProvidedSecretKind"/>.
    /// </param>
    /// <returns>The user secret.</returns>
    /// <remarks>
    /// Implementations should throw <see cref="SecretInterfaceOperationCancelledException"/>
    /// to indicate that the user cancelled the operation without entering the secret.
    /// </remarks>
    RawUserSecret GetUserSecret(Guid archiveId, ImmutableArray<SecretMetadata> possibleSecretMetadata);

    /// <summary>
    /// Interface to retrieve the necessary paramaters to register a new user secret.
    /// </summary>
    /// <param name="archiveId">The unique identifier of the secure archive.</param>
    /// <returns>The <see cref="UserKeyAuthorizationParameters"/> for the new secret.</returns>
    /// <remarks>
    /// Implementations should throw <see cref="SecretInterfaceOperationCancelledException"/>
    /// to indicate that the user cancelled the operation without entering a secret.
    /// </remarks>
    UserKeyAuthorizationParameters GetNewKeyAuthorizationParameters(Guid archiveId);
}
