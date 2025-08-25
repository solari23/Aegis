namespace Aegis.Passkeys;

/// <summary>
/// Implementation of <see cref="IPasskeyProvider"/> for unsupported operating systems.
/// </summary>
internal class UnsupportedOSPasskeyProvider : IPasskeyProvider
{
    public void Dbg1() => throw new NotImplementedException();
    public void Dbg2() => throw new NotImplementedException();

    /// <inheritdoc />
    public bool IsHmacSecretSupported() => false;
}
