using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Aegis.Passkeys.WebAuthNInterop;
using Aegis.Passkeys.Windows.Extensions;
using Aegis.Passkeys.Windows.Structures;

namespace Aegis.Passkeys.Windows;

/// <summary>
/// Provides access to passkey functionality on Windows platforms.
/// </summary>
/// <remarks>
/// Win32 WebAuthN API documentation is somewhat sparse.
/// A good reference is chromium integration here:
/// https://chromium.googlesource.com/chromium/src/+/refs/heads/main/device/fido/win/
/// </remarks>
[SupportedOSPlatform("windows")]
internal class WindowsPasskeyProvider : IPasskeyProvider
{
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
    /// <remarks>
    /// Support was added in Win11 25H2. Prior to that, the MakeCredential API would bootstrap the PRF
    /// functionality but would not return an HMAC secret. A second call to GetAssertion would be needed.
    /// </remarks>
    public bool IsHmacGenerationDuringMakeCredentialSupported()
        => Win32ApiVersion.Value >= WEBAUTHN_API_VERSION.WEBAUTHN_API_VERSION_9;

    /// <inheritdoc />
    public GetHmacSecretResponse GetHmacSecret(
        RelyingPartyInfo rpInfo,
        HmacSecret salt,
        HmacSecret? secondSalt,
        IReadOnlyList<Identifier> allowedCredentialIds)
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

        if (allowedCredentialIds.Any())
        {
            options.pAllowCredentialList = new WEBAUTHN_CREDENTIAL_LIST
            {
                Credentials = allowedCredentialIds
                    .Select(id => new WEBAUTHN_CREDENTIAL_EX
                    {
                        id = id.internalValue,
                    })
                    .ToArray(),
            };
        }

        WEBAUTHN_ASSERTION.SafeHandle? assertionSafeHandle = null;
        try
        {
            HResult hr = Win32Interop.WebAuthNAuthenticatorGetAssertion(
                this.WindowHandle,
                rpInfo.Id,
                ref clientData,
                ref options,
                out assertionSafeHandle);
            HandleError(hr);

            var assertion = assertionSafeHandle.ToManaged()!.Value;

            if (assertion.pHmacSecret is null)
            {
                throw new PasskeyOperationFailedException(
                    PasskeyFailureCode.PasskeyDoesNotSupportHmac,
                    "The passkey selected by the user does not support HMAC secrets.");
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

    /// <inheritdoc />
    public MakeCredentialResponse MakeCredentialWithHmacSecret(
        RelyingPartyInfo rpInfo,
        UserEntityInfo userInfo,
        HmacSecret? salt,
        HmacSecret? secondSalt,
        UserVerificationRequirement userVerificationRequirement)
    {
        WEBAUTHN_RP_ENTITY_INFORMATION rpEntityInfo = new()
        {
            pwszId = rpInfo.Id,
            pwszName = rpInfo.DisplayName,
            pwszIcon = rpInfo.IconUrl,
        };

        WEBAUTHN_USER_ENTITY_INFORMATION userEntityInfo = new()
        {
            id = userInfo.Id.internalValue,
            pwszName = userInfo.Name,
            pwszDisplayName = userInfo.DisplayName,
            pwszIcon = userInfo.IconUrl,
        };

        WEBAUTHN_COSE_CREDENTIAL_PARAMETERS credentialParams = WEBAUTHN_COSE_CREDENTIAL_PARAMETERS.MakeDefault();

        var collectedClientData = CollectedClientData.ForMakeCredentialCall(
            challenge: RandomNumberGenerator.GetBytes(32), // We don't need the actual assertion, so just use a random challenge.
            origin: rpInfo.Origin);
        WEBAUTHN_CLIENT_DATA clientData = new()
        {
            clientDataJSON = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(collectedClientData)),
        };

        WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS options = new()
        {
            // A resident key is required to make this work.
            bRequireResidentKey = true,

            // HMAC Secret is not implemented by Windows platform. We're looking for a YubiKey.
            dwAuthenticatorAttachment = AuthenticatorAttachment.CrossPlatform,

            // Add the extension to create HMAC secret.
            dwFlags = WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS_FLAGS.WEBAUTHN_AUTHENTICATOR_HMAC_SECRET_VALUES_FLAG,
            Extensions = [new HmacSecretExtensions.Input().ToRawExtensionData()],

            // Other options.
            dwUserVerificationRequirement = userVerificationRequirement,
        };

        if (salt is not null)
        {
            options.pPRFGlobalEval = new WEBAUTHN_HMAC_SECRET_SALT
            {
                first = salt.InternalSecretData,
                second = secondSalt?.InternalSecretData,
            };
        }

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
            HandleError(hr);

            var attestation = attestationSafeHandle.ToManaged()!.Value;

            var hmacSecretExtension = OutputExtension.ParseRawExtensions(attestation.Extensions)
                .Where(ext => ext is HmacSecretExtensions.Output)
                .FirstOrDefault() as HmacSecretExtensions.Output;
            if (hmacSecretExtension is null || !hmacSecretExtension.WasHmacCredentialMade)
            {
                throw new PasskeyOperationFailedException(
                    PasskeyFailureCode.PasskeyDoesNotSupportHmac,
                    "The passkey registered by the user does not support HMAC secrets.");
            }

            HmacSecret? firstHmac = null;
            if (salt is not null)
            {
                if (attestation.pHmacSecret?.first is null)
                {
                    throw new PasskeyOperationFailedException(
                        PasskeyFailureCode.PasskeyIneropFailed,
                        "The desired HMAC for the first salt was not created. This is unexpected.");
                }

                firstHmac = new HmacSecret(attestation.pHmacSecret.Value.first);
            }

            HmacSecret? secondHmac = null;
            if (secondSalt is not null)
            {
                if (attestation.pHmacSecret?.second is null)
                {
                    throw new PasskeyOperationFailedException(
                        PasskeyFailureCode.PasskeyIneropFailed,
                        "The desired HMAC for the second salt was not created. This is unexpected.");
                }

                secondHmac = new HmacSecret(attestation.pHmacSecret.Value.second);
            }

            var response = new MakeCredentialResponse
            {
                NewCredentialId = new Identifier(attestation.credentialId),
                FirstHmac = firstHmac,
                SecondHmac = secondHmac,
            };
            return response;
        }
        finally
        {
            attestationSafeHandle?.Dispose();
        }
    }

    private static void HandleError(HResult hr)
    {
        if (hr == HResult.S_OK)
        {
            return;
        }

        switch (hr)
        {
            case HResult.E_CANCELLED:
            case HResult.NTE_USER_CANCELLED:
                throw new PasskeyOperationFailedException(
                    PasskeyFailureCode.OperationCancelled,
                    "The user cancelled the passkey operation.");
            default:
                string errorString = Win32Interop.WebAuthNGetErrorName(hr);
                throw new PasskeyOperationFailedException(
                    PasskeyFailureCode.PasskeyIneropFailed,
                    $"The passkey operation failed with error: {errorString} ({hr}).",
                    (int)hr,
                    errorString);
        }
    }
}
