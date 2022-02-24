using System.Collections.Immutable;

using Aegis.Models;

namespace Aegis.Core.CredentialsInterface;

/// <summary>
/// Interface for components that provide an interface for users to input secrets.
/// </summary>
public interface IUserSecretSelector
{
    /// <summary>
    /// User interface for choosing a <see cref="SecretKind"/> from the list of available values.
    /// </summary>
    /// <param name="availableSecretKinds">The list of choices.</param>
    /// <returns>The user's choice.</returns>
    /// <remarks>
    /// Implementations should throw <see cref="SecretInterfaceOperationCancelledException"/>
    /// to indicate that the user cancelled the operation without selecting a secret kind.
    /// </remarks>
    SecretKind PromptSelectSecretKind(ImmutableArray<SecretKind> availableSecretKinds);
}
