﻿using System;

using Aegis.Models;

namespace Aegis.Core.CredentialsInterface
{
    /// <summary>
    /// Encapsulates the parameters required to authorize a new user key.
    /// </summary>
    public sealed class UserKeyAuthorizationParameters : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserKeyAuthorizationParameters"/> class.
        /// </summary>
        /// <param name="keyData">The raw archive encryption key.</param>
        public UserKeyAuthorizationParameters(RawUserSecret userSecret)
        {
            ArgCheck.NotNull(userSecret, nameof(userSecret));

            this.UserSecret = userSecret;
        }

        /// <summary>
        /// Gets or sets the friendly name for the user key.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the user key.
        /// </summary>
        public RawUserSecret UserSecret { get; }

        /// <summary>
        /// Gets or sets metadata to store about the user secret.
        /// </summary>
        public SecretMetadata SecretMetadata { get; set; }

        #region IDisposable Support

        /// <summary>
        /// Flag to detect redundant calls to dispose the object.
        /// </summary>
        private bool isDisposed = false;

        /// <summary>
        /// Disposes the current object when it is no longer required.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the current object when it is no longer required.
        /// </summary>
        /// <param name="disposing">Whether or not the operation is coming from Dispose() (as opposed to a finalizer).</param>
        protected void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    this.UserSecret.Dispose();
                }

                this.isDisposed = true;
            }
        }

        #endregion
    }
}