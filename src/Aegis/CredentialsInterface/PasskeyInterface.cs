using System.Collections.Immutable;

using Aegis.Core;
using Aegis.Core.CredentialsInterface;
using Aegis.Core.Crypto;
using Aegis.Models;
using Aegis.Passkeys;

namespace Aegis.CredentialsInterface;

/// <summary>
/// Implementation of <see cref="IUserSecretEntryInterface"/> for interacting with certificates from the command line.
/// </summary>
public class PasskeyInterface : IUserSecretEntryInterface
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PasskeyInterface"/> class.
    /// </summary>
    /// <param name="ioStreamSet">The IO streams.</param>
    public PasskeyInterface(IOStreamSet ioStreamSet)
    {
        ArgCheck.NotNull(ioStreamSet);

        this.IO = ioStreamSet;
        this.PasskeyManager = new PasskeyManager();
    }

    /// <inheritdoc />
    public SecretKind ProvidedSecretKind => SecretKind.PasskeyHmacSecret;

    /// <inheritdoc />
    public bool IsCurrentPlatformSupported() => this.PasskeyManager.IsHmacSecretSupported();

    /// <summary>
    /// Gets the IO streams.
    /// </summary>
    private IOStreamSet IO { get; }

    /// <summary>
    /// The manager class to use for interacting with passkeys.
    /// </summary>
    private PasskeyManager PasskeyManager { get; }

    /// <inheritdoc />
    public bool CanProvideSecret(Guid archiveId, SecretMetadata secretMetadata) => this.PasskeyManager.IsHmacSecretSupported();

    /// <inheritdoc />
    public UserKeyAuthorizationParameters GetNewKeyAuthorizationParameters(Guid archiveId)
    {
        // TODO: Implement PasskeyPickerInterface.GetNewKeyAuthorizationParameters
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public RawUserSecret GetUserSecret(Guid archiveId, ImmutableArray<SecretMetadata> possibleSecretMetadata)
    {
        var possibleCredentialIds = possibleSecretMetadata
            .Select(md => (md as PasskeyHmacSecretMetadata)?.CredentialId);
        


        throw new NotImplementedException();
    }
}
