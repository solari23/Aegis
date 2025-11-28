namespace Aegis.Passkeys;

/// <summary>
/// Represents a secret used either as input or output for hmac-secret operations with passkeys.
/// </summary>
public class HmacSecret : IDisposable
{
    /// <summary>
    /// Creates a new instance of the <see cref="HmacSecret"/> class.
    /// </summary>
    /// <param name="secretData">The secret data. This container will store a copy of the data. It must be exactly 32 bytes.</param>
    public HmacSecret(ReadOnlySpan<byte> secretData)
    {
        if (secretData.Length != 32)
        {
            throw new ArgumentException("The secret data must be exactly 32 bytes long.", nameof(secretData));
        }

        this.InternalSecretData = new byte[secretData.Length];
        secretData.CopyTo(this.InternalSecretData);
    }

    /// <summary>
    /// Gets the underlying secret as a read-only data structure.
    /// </summary>
    public ReadOnlySpan<byte> Secret => this.InternalSecretData;

    /// <summary>
    /// Stores the underlying secret data.
    /// </summary>
    internal byte[] InternalSecretData { get; }

    #region IDisposable Implementation

    private bool isDisposed = false;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                // Clear out the key.
                ((Span<byte>)this.InternalSecretData).Fill(0);
            }

            isDisposed = true;
        }
    }

    #endregion
}
