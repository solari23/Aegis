using System.Runtime.Versioning;

using Aegis.Passkeys.Windows;
using Aegis.Passkeys.Windows.Structures;

namespace Aegis.Passkeys;

/// <summary>
/// Provides abstracted access to passkey functionality provided by the OS.
/// </summary>
public class PasskeyManager
{
    public void Do() => this.PasskeyProvider.Dbg1();
    public void Do2() => this.PasskeyProvider.Dbg2();

    public PasskeyManager()
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(10))
        {
            this.PasskeyProvider = new WindowsPasskeyProvider();
        }

        this.PasskeyProvider = new UnsupportedOSPasskeyProvider();
    }

    [SupportedOSPlatform("windows")]
    public PasskeyManager(IntPtr windowHandlePointer)
    {
        this.PasskeyProvider = new WindowsPasskeyProvider(
            new HWnd(windowHandlePointer));
    }

    private IPasskeyProvider PasskeyProvider { get; init; }

    /// <summary>
    /// Checks if the current platform supports the HMAC secrets extension from CTAP.
    /// </summary>
    /// <returns>True if HMAC secrets are supported, false otherwise.</returns>
    public bool IsHmacSecretSupported() => this.PasskeyProvider.IsHmacSecretSupported();
}