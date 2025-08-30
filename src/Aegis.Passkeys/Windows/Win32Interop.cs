using System.Runtime.InteropServices;
using System.Runtime.Versioning;

using Aegis.Passkeys.Windows.Structures;

namespace Aegis.Passkeys.Windows;

[SupportedOSPlatform("windows")]
internal static partial class Win32Interop
{
    [LibraryImport("user32.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static partial HWnd GetForegroundWindow();

    [LibraryImport("kernel32.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static partial HWnd GetConsoleWindow();

    /// <summary>
    /// https://learn.microsoft.com/en-us/windows/win32/api/webauthn/nf-webauthn-webauthnauthenticatorgetassertion
    /// </summary>
    /// <returns></returns>
    [LibraryImport("webauthn.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static partial HResult WebAuthNAuthenticatorGetAssertion(
        HWnd hWnd,
        [MarshalAs(UnmanagedType.LPWStr)] string pwszRpId,
        ref WEBAUTHN_CLIENT_DATA pWebAuthNClientData,
        ref WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS pWebAuthNGetAssertionOptions,
        out WEBAUTHN_ASSERTION.SafeHandle ppWebAuthNAssertion);

    /// <summary>
    /// https://learn.microsoft.com/en-us/windows/win32/api/webauthn/nf-webauthn-webauthngetapiversionnumber
    /// </summary>
    [LibraryImport("webauthn.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static partial WEBAUTHN_API_VERSION WebAuthNGetApiVersionNumber();

    /// <summary>
    /// https://learn.microsoft.com/en-us/windows/win32/api/webauthn/nf-webauthn-webauthnfreeassertion
    /// </summary>
    [LibraryImport("webauthn.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static partial void WebAuthNFreeAssertion(IntPtr pWebAuthNAssertion);

    /// <summary>
    /// https://learn.microsoft.com/en-us/windows/win32/api/webauthn/nf-webauthn-webauthnfreecredentialattestation
    /// </summary>
    /// <param name="pWebAuthNCredentialAttestation"></param>
    [LibraryImport("webauthn.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static partial void WebAuthNFreeCredentialAttestation(IntPtr pWebAuthNCredentialAttestation);

    /// <summary>
    /// https://learn.microsoft.com/en-us/windows/win32/api/webauthn/nf-webauthn-webauthnfreeplatformcredentiallist
    /// </summary>
    [LibraryImport("webauthn.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static partial void WebAuthNFreePlatformCredentialList(IntPtr pCredentialDetailsList);

    /// <summary>
    /// https://learn.microsoft.com/en-us/windows/win32/api/webauthn/nf-webauthn-webauthngetplatformcredentiallist
    /// </summary>
    [LibraryImport("webauthn.dll", StringMarshalling = StringMarshalling.Utf16), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static partial HResult WebAuthNGetPlatformCredentialList(
        ref WEBAUTHN_GET_CREDENTIALS_OPTIONS pGetCredentialsOptions,
        out WEBAUTHN_CREDENTIAL_DETAILS_LIST.SafeHandle ppCredentialDetailsList);

    /// <summary>
    /// https://learn.microsoft.com/en-us/windows/win32/api/webauthn/nf-webauthn-webauthngeterrorname
    /// </summary>
    [LibraryImport("webauthn.dll", StringMarshalling = StringMarshalling.Utf16), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static partial ConstString WebAuthNGetErrorName(HResult hr);

    /// <summary>
    /// https://learn.microsoft.com/en-us/windows/win32/api/webauthn/nf-webauthn-webauthnauthenticatormakecredential
    /// </summary>
    [LibraryImport("webauthn.dll", StringMarshalling = StringMarshalling.Utf16), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static partial HResult WebAuthNAuthenticatorMakeCredential(
        HWnd hWind,
        ref WEBAUTHN_RP_ENTITY_INFORMATION pRpInformation,
        ref WEBAUTHN_USER_ENTITY_INFORMATION pUserInformation,
        ref WEBAUTHN_COSE_CREDENTIAL_PARAMETERS pPubKeyCredParams,
        ref WEBAUTHN_CLIENT_DATA pWebAuthNClientData,
        ref WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS pWebAuthNMakeCredentialOptions,
        out WEBAUTHN_CREDENTIAL_ATTESTATION.SafeHandle ppWebAuthNCredentialAttestation);
}
