using System.Collections.Immutable;
using System.Text;

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
        var namePrompt = new InputPrompt(this.IO, "Enter a name to identify the passkey: ");
        var friendlyName = namePrompt.GetValue();

        var rpId = PasskeyHelpers.GetRelyingPartyIdForIdentifier(archiveId);
        var hmacSalt = PasskeyHelpers.IdentifierToHmacSalt(archiveId);

        var makeCredentialResult = this.PasskeyManager.MakeCredentialWithHmacSecret(
            new RelyingPartyInfo
            {
                Id = rpId,
                Origin = rpId,
                DisplayName = $"Aegis (Archive {archiveId:N})",
            },
            new UserEntityInfo
            {
                Id = new Identifier(Encoding.UTF8.GetBytes(friendlyName)),
                Name = friendlyName,
                DisplayName = friendlyName,
            },
            salt: new HmacSecret(hmacSalt));

        var userSecret = RawUserSecret.CopyFromBytes(makeCredentialResult.FirstHmac.Secret);
        return new UserKeyAuthorizationParameters(userSecret)
        {
            FriendlyName = friendlyName,
            SecretMetadata = new PasskeyHmacSecretMetadata
            {
                CredentialId = makeCredentialResult.NewCredentialId.Value.ToArray(),
            },
        };
    }

    /// <inheritdoc />
    public RawUserSecret GetUserSecret(Guid archiveId, ImmutableArray<SecretMetadata> possibleSecretMetadata)
    {
        var possibleCredentialIds = possibleSecretMetadata
            .Select(md => new Identifier((md as PasskeyHmacSecretMetadata)?.CredentialId))
            .ToArray();

        var rpId = PasskeyHelpers.GetRelyingPartyIdForIdentifier(archiveId);
        var hmacSalt = PasskeyHelpers.IdentifierToHmacSalt(archiveId);

        var getHmacResult = this.PasskeyManager.GetHmacSecret(
            new RelyingPartyInfo
            {
                Id = rpId,
                Origin = rpId,
                DisplayName = $"Aegis (Archive {archiveId:N})",
            },
            new HmacSecret(hmacSalt),
            allowedCredentialIds: possibleCredentialIds);

        return RawUserSecret.CopyFromBytes(getHmacResult.First.Secret);
    }
}
