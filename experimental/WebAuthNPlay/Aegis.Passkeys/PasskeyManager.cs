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

    /// <summary>
    /// Initializes a new instance of the <see cref="PasskeyManager"/> class.
    /// </summary>
    public PasskeyManager()
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(10))
        {
            this.PasskeyProvider = new WindowsPasskeyProvider();
        }
        else
        {
            this.PasskeyProvider = new UnsupportedOSPasskeyProvider();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PasskeyManager"/> class with a window handle.
    /// </summary>
    /// <param name="windowHandlePointer">The pointer to the window handle (HWND) to use for UI prompts.</param>
    /// <remarks>This constructor is only supported on Windows platforms.</remarks>
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

    /// <summary>
    /// Generates an HMAC secret from a passkey authenticator.
    /// </summary>
    /// <param name="rpInfo">Details about the Relying Party requesting the secret.</param>
    /// <param name="salt">The first salt to use in the HMAC secret generation.</param>
    /// <param name="secondSalt">An optional second salt to use in the HMAC secret generation.</param>
    /// <returns>The generated HMAC secrets.</returns>
    public GetHmacSecretResponse GetHmacSecret(RelyingPartyInfo rpInfo, HmacSecret salt, HmacSecret? secondSalt = null)
        => this.PasskeyProvider.GetHmacSecret(rpInfo, salt, secondSalt);
}