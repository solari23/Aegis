using System.Collections.Immutable;
using System.Text;

using Aegis.Core;
using Aegis.Core.CredentialsInterface;
using Aegis.Models;

namespace Aegis.CredentialsInterface;

/// <summary>
/// Implementation of <see cref="IUserSecretEntryInterface"/> for entering passwords on the command line.
/// </summary>
public class PasswordEntryInterface : IUserSecretEntryInterface
{
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
        var namePrompt = new InputPrompt(this.IO, "Enter a name to identify the new password: ");
        var friendlyName = namePrompt.GetValue();

        var secret = this.GetUserSecret(ImmutableArray<SecretMetadata>.Empty);

        return new UserKeyAuthorizationParameters(secret)
        {
            FriendlyName = friendlyName,
            SecretMetadata = new PasswordSecretMetadata(),
        };
    }

    /// <inheritdoc />
    public RawUserSecret GetUserSecret(ImmutableArray<SecretMetadata> possibleSecretMetadata)
    {
        var passwordPrompt = new InputPrompt(this.IO, "Enter the password: ", isConfidentialInput: true);
        var password = passwordPrompt.GetValue();

        return new RawUserSecret(Encoding.UTF8.GetBytes(password));
    }
}
