using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Aegis.Models;

namespace Aegis.Core.CredentialsInterface
{
    /// <summary>
    /// Helper to assist in unlocking an <see cref="AegisArchive"/>.
    /// </summary>
    public sealed class ArchiveUnlocker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveUnlocker"/> class.
        /// </summary>
        /// <param name="secretSelector">The secret selector user interface to use.</param>
        /// <param name="secretProviders">The secret entry user interfaces to register.</param>
        public ArchiveUnlocker(
            IUserSecretSelector secretSelector,
            params IUserSecretEntryInterface[] secretProviders)
        {
            ArgCheck.NotNull(secretSelector, nameof(secretSelector));
            ArgCheck.NotEmpty(secretProviders, nameof(secretProviders));

            this.SecretSelector = secretSelector;

            foreach (var provider in secretProviders)
            {
                this.RegisterSecretEntryInterface(provider);
            }
        }

        /// <summary>
        /// Gets the secret selector user interface.
        /// </summary>
        private IUserSecretSelector SecretSelector { get; }

        /// <summary>
        /// Gets the collection of registered secret entry user interfaces,
        /// keyed by their respective <see cref="SecretKind"/>.
        /// </summary>
        private Dictionary<SecretKind, IUserSecretEntryInterface> SecretEntryInterfaces { get; }
            = new Dictionary<SecretKind, IUserSecretEntryInterface>();

        /// <summary>
        /// Registers a <see cref="IUserSecretEntryInterface"/> that can be used to retrieve user secrets from the user.
        /// </summary>
        /// <param name="secretEntryInterface">The interface to register.</param>
        /// <exception cref="ArgumentException">
        /// A <see cref="IUserSecretEntryInterface"/> is already registered for the same <see cref="SecretKind"/>.
        /// </exception>
        public void RegisterSecretEntryInterface(IUserSecretEntryInterface secretEntryInterface)
        {
            ArgCheck.NotNull(secretEntryInterface, nameof(secretEntryInterface));

            if (secretEntryInterface.ProvidedSecretKind == SecretKind.Unknown)
            {
                throw new ArgumentException(
                    $"Can't register secret interface for 'unknown' secret kind.",
                    nameof(secretEntryInterface));
            }

            if (this.SecretEntryInterfaces.ContainsKey(secretEntryInterface.ProvidedSecretKind))
            {
                var existingProviderType = this.SecretEntryInterfaces[secretEntryInterface.ProvidedSecretKind].GetType();
                throw new ArgumentException(
                    $"Can't register interface for secret kind '{secretEntryInterface.ProvidedSecretKind}' since one of type {existingProviderType.FullName} is already registered.");
            }

            this.SecretEntryInterfaces.Add(secretEntryInterface.ProvidedSecretKind, secretEntryInterface);
        }

        /// <summary>
        /// Prompts the user for their user key and uses it to unlock the given archive.
        /// </summary>
        /// <param name="archive">The archive to unlock.</param>
        public void Unlock(AegisArchive archive)
        {
            ArgCheck.NotNull(archive, nameof(archive));

            if (!archive.IsLocked)
            {
                throw new InvalidOperationException("The given archive is already unlocked.");
            }

            // First, filter down the user key authorizations to only keep those for which we have a registered interface.
            var availableUserKeys = archive.GetUserKeyAuthorizations()
                .Where(key => 
                    this.SecretEntryInterfaces.ContainsKey(key.SecretMetadata.SecretKind)
                    && this.SecretEntryInterfaces[key.SecretMetadata.SecretKind].CanProvideSecret(key.SecretMetadata));

            if (!availableUserKeys.Any())
            {
                throw new NoKeyAvailableException(
                    "None of the authorized user keys are currently available because this interface does not support them.");
            }

            // Of those, ask the user to choose which type of secret they'd like to use to unlock the archive.
            var availableSecretKinds = availableUserKeys.Select(k => k.SecretMetadata.SecretKind).Distinct().ToImmutableArray();
            var selectedSecretKind = availableSecretKinds.Length > 1
                ? this.SecretSelector.PromptSelectSecretKind(availableSecretKinds)
                : availableSecretKinds[0];

            // Split out the user keys that are available, and of the selected type.
            var selectedUserKeys = availableUserKeys
                .Where(k => k.SecretMetadata.SecretKind == selectedSecretKind)
                .Select(k => k.SecretMetadata)
                .ToImmutableArray();

            // Finally, prompt to get the user secret and use it to unlock the archive.
            using var userSecret = this.SecretEntryInterfaces[selectedSecretKind].GetUserSecret(selectedUserKeys);
            archive.Unlock(userSecret);
        }

        /// <summary>
        /// Prompts the user to provide the parameters for a new user key authorization.
        /// </summary>
        /// <returns>The new <see cref="UserKeyAuthorizationParameters"/>.</returns>
        public UserKeyAuthorizationParameters GetNewUserKeyAuthorization()
        {
            var newSecretKind = this.SecretEntryInterfaces.Count > 1
                ? this.SecretSelector.PromptSelectSecretKind(this.SecretEntryInterfaces.Keys.ToImmutableArray())
                : this.SecretEntryInterfaces.Keys.First();

            return this.SecretEntryInterfaces[newSecretKind].GetNewKeyAuthorizationParameters();
        }

        /// <summary>
        /// Prompts the user for a new user key and authorizes it to unlock the given archive.
        /// The archive must already be unlocked.
        /// </summary>
        /// <param name="archive">The archive that the new secret will be authorized to unlock.</param>
        public void AuthorizeNewUserSecret(AegisArchive archive)
        {
            ArgCheck.NotNull(archive, nameof(archive));

            archive.ThrowIfLocked();

            using var newAuthorization = this.GetNewUserKeyAuthorization();
            
            // TODO: Implement subroutine for authorizing new user secrets.
            this.SecretSelector.ToString();
            throw new NotImplementedException();
        }
    }
}
