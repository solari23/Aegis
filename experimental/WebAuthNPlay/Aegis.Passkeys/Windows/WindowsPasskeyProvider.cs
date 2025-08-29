using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;

using Aegis.Passkeys.WebAuthNInterop;
using Aegis.Passkeys.Windows.Structures;
using Aegis.Passkeys.Windows.Extensions;

namespace Aegis.Passkeys.Windows;

/// <summary>
/// Provides access to passkey functionality on Windows platforms.
/// </summary>
[SupportedOSPlatform("windows")]
internal class WindowsPasskeyProvider : IPasskeyProvider
{
    // TODO [Fit & Finish]: Remove debug methods.
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
        WEBAUTHN_RP_ENTITY_INFORMATION rpEntityInfo = new()
        {
            pwszId = "alkerTestRP.local",
            pwszName = "Alker Test RP",
        };
        WEBAUTHN_USER_ENTITY_INFORMATION userEntityInfo = new()
        {
            pwszName = "alkerUser",//@alkerTestRP.local",
            pwszDisplayName = "Alker User",
            id = [0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEF, 0xDE, 0xAD, 0xBE, 0xEE],
        };
        WEBAUTHN_COSE_CREDENTIAL_PARAMETERS credentialParams = WEBAUTHN_COSE_CREDENTIAL_PARAMETERS.MakeDefault();

        var collectedClientData = CollectedClientData.ForMakeCredentialCall(
            challenge: RandomNumberGenerator.GetBytes(32),
            origin: "https://alkerTestRP.local");
        WEBAUTHN_CLIENT_DATA clientData = new()
        {
            clientDataJSON = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(collectedClientData)),
        };

        WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS options = new()
        {
            bRequireResidentKey = true,
            //dwAuthenticatorAttachment = AuthenticatorAttachment.CrossPlatform,
            dwUserVerificationRequirement = UserVerificationRequirement.Discouraged,

            dwFlags = WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS_FLAGS.WEBAUTHN_AUTHENTICATOR_HMAC_SECRET_VALUES_FLAG,

            Extensions = [new HmacSecretExtensions.Input().ToRawExtensionData()],
        };

        WEBAUTHN_CREDENTIAL_ATTESTATION.SafeHandle? attestationSafeHandle = null;
        try
        {
            HResult hr = Win32Interop.WebAuthNAuthenticatorMakeCredential(
                this.WindowHandle,
                ref rpEntityInfo,
                ref userEntityInfo,
                ref credentialParams,
                ref clientData,
                ref options,
                out attestationSafeHandle);

            var attestation = attestationSafeHandle.ToManaged()!.Value;

            var hmacSecretExtension = OutputExtension.ParseRawExtensions(attestation.Extensions)
                .Where(ext => ext is HmacSecretExtensions.Output)
                .FirstOrDefault() as HmacSecretExtensions.Output;
            if (hmacSecretExtension is null || !hmacSecretExtension.WasHmacCredentialMade)
            {
                throw new Exception();
            }
        }
        finally
        {
            attestationSafeHandle?.Dispose();
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

    /// <inheritdoc />
    public GetHmacSecretResponse GetHmacSecret(RelyingPartyInfo rpInfo, HmacSecret salt, HmacSecret? secondSalt = null)
    {
        var collectedClientData = CollectedClientData.ForGetAssertionCall(
            challenge: RandomNumberGenerator.GetBytes(32),  // We don't need the actual assertion, so just use a random challenge.
            origin: rpInfo.Origin);
        WEBAUTHN_CLIENT_DATA clientData = new()
        {
            clientDataJSON = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(collectedClientData)),
        };
        WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS options = new()
        {
            pHmacSecretSaltValues = new WEBAUTHN_HMAC_SECRET_SALT_VALUES
            {
                pGlobalHmacSalt = new WEBAUTHN_HMAC_SECRET_SALT
                {
                    first = salt.InternalSecretData,
                    second = secondSalt?.InternalSecretData,
                },
            }
        };

        WEBAUTHN_ASSERTION.SafeHandle? assertionSafeHandle = null;
        try
        {
            HResult hr = Win32Interop.WebAuthNAuthenticatorGetAssertion(
                this.WindowHandle,
                rpInfo.Id,
                ref clientData,
                ref options,
                out assertionSafeHandle);

            if (hr != HResult.S_OK)
            {
                string errorString = Win32Interop.WebAuthNGetErrorName(hr);
                // TODO [Fit & Finish]: Throw meaningful exception.
                throw new Exception();
            }

            var assertion = assertionSafeHandle.ToManaged()!.Value;

            if (assertion.pHmacSecret is null)
            {
                // TODO [Fit & Finish]: Throw meaningful exception.
                throw new Exception();
            }

            var hmacSecretFirst = new HmacSecret(assertion.pHmacSecret.Value.first);
            var hmacSecretSecond = assertion.pHmacSecret.Value.second is null
                ? null
                : new HmacSecret(assertion.pHmacSecret.Value.second);
            return new GetHmacSecretResponse
            {
                First = hmacSecretFirst,
                Second = hmacSecretSecond,
            };
        }
        finally
        {
            assertionSafeHandle?.Dispose();
        }
    }
}
