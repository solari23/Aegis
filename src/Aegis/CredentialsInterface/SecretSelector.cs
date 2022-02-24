using System.Collections.Immutable;

using Aegis.Core;
using Aegis.Core.CredentialsInterface;
using Aegis.Models;

namespace Aegis.CredentialsInterface;

/// <summary>
/// Implementation of <see cref="IUserSecretSelector"/> for selecting a secret from the command line.
/// </summary>
public class SecretSelector : IUserSecretSelector
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecretSelector"/> class.
    /// </summary>
    /// <param name="ioStreamSet">The IO streams.</param>
    public SecretSelector(IOStreamSet ioStreamSet)
    {
        ArgCheck.NotNull(ioStreamSet, nameof(ioStreamSet));

        this.IO = ioStreamSet;
    }

    /// <summary>
    /// Gets the IO streams.
    /// </summary>
    private IOStreamSet IO { get; }

    /// <inheritdoc />
    public SecretKind PromptSelectSecretKind(ImmutableArray<SecretKind> availableSecretKinds)
    {
        var menu = new Menu<SecretKind>(
            this.IO,
            "Select secret kind",
            availableSecretKinds.Select(k => new Menu<SecretKind>.Option(k, k.ToString())).ToArray());

        return menu.GetSelection();
    }
}
