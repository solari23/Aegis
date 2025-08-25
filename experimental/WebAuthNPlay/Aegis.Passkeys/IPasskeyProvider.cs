namespace Aegis.Passkeys;

internal interface IPasskeyProvider
{
    // TODO: Remove Dbg1 and Dbg2 methods.
    void Dbg1();
    void Dbg2();

    /// <summary>
    /// Checks if the current platform supports HMAC secret generation for passkeys.
    /// </summary>
    bool IsHmacSecretSupported();
}
