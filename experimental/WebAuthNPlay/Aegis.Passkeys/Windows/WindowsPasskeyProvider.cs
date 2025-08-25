using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;

using Aegis.Passkeys.WebAuthNInterop;
using Aegis.Passkeys.Windows.Structures;

namespace Aegis.Passkeys.Windows;

/// <summary>
/// Provides access to passkey functionality on Windows platforms.
/// </summary>
[SupportedOSPlatform("windows")]
internal class WindowsPasskeyProvider : IPasskeyProvider
{
    #region Debug To Delete
    public void Dbg1()
    {
        Console.WriteLine($"VNum: {Win32Interop.WebAuthNGetApiVersionNumber()}");

        WEBAUTHN_GET_CREDENTIALS_OPTIONS getCredOptions = new()
        {
            pwszRpId = "login.microsoft.com",
            bBrowserInPrivateMode = false,
        };

        WEBAUTHN_CREDENTIAL_DETAILS_LIST.SafeHandle? credentialListHandle = null;
        try
        {
            var foo = BitConverter.IsLittleEndian;
            HResult hr = Win32Interop.WebAuthNGetPlatformCredentialList(ref getCredOptions, out credentialListHandle);
            var credsList = credentialListHandle.ToManaged()?.credentials!;

            foreach (var cred in credsList)
            {
                Console.WriteLine("CRED: ");
                Console.WriteLine($"    RP_ID: {cred.pRpInformation.pwszId}");
                Console.WriteLine($"    RP_Name: {cred.pRpInformation.pwszName}");
                Console.WriteLine($"    USER_Name: {cred.pUserInformation.pwszName}");
                Console.WriteLine($"    USER_DName: {cred.pUserInformation.pwszDisplayName}");
            }
        }
        finally
        {
            credentialListHandle?.Dispose();
        }
    }

    public void Dbg2()
    {
        var collectedClientData = CollectedClientData.ForGetAssertionCall(
            challenge: RandomNumberGenerator.GetBytes(32),
            origin: "https://github.com");

        WEBAUTHN_CLIENT_DATA clientData = new()
        {
            clientDataJSON = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(collectedClientData)),
        };
        WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS options = new();
        options.pHmacSecretSaltValues = new WEBAUTHN_HMAC_SECRET_SALT_VALUES
        {
            pGlobalHmacSalt = new WEBAUTHN_HMAC_SECRET_SALT
            {
                first = [0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF,
                         0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF],
                second = [0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF,
                          0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xED],
            }
        };

        WEBAUTHN_ASSERTION.SafeHandle? assertionSafeHandle = null;
        try
        {
            HResult hr = Win32Interop.WebAuthNAuthenticatorGetAssertion(
                this.WindowHandle,
                "alkerTestRP.local",
                ref clientData,
                ref options,
                out assertionSafeHandle);

            var assertion = assertionSafeHandle.ToManaged();
        }
        finally
        {
            assertionSafeHandle?.Dispose();
        }
    }
    #endregion

    public WindowsPasskeyProvider() : this(Win32Interop.GetForegroundWindow())
    {
        // Empty.
    }

    public WindowsPasskeyProvider(HWnd windowHandle)
    {
        this.WindowHandle = windowHandle;
    }

    private HWnd WindowHandle { get; init; }

    private Lazy<WEBAUTHN_API_VERSION> Win32ApiVersion => new(Win32Interop.WebAuthNGetApiVersionNumber);

    /// <inheritdoc />
    public bool IsHmacSecretSupported() => Win32ApiVersion.Value >= WEBAUTHN_API_VERSION.WEBAUTHN_API_VERSION_6;
}
