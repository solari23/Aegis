using System.Runtime.Versioning;

using Aegis.Passkeys.WebAuthNInterop;
using Aegis.Passkeys.Windows;
using Aegis.Passkeys.Windows.Structures;

namespace Aegis.Passkeys;

/// <summary>
/// Provides abstracted access to passkey functionality provided by the OS.
/// </summary>
public class PasskeyManager
{
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
    /// <param name="allowedCredentialIds">
    ///     An optional list of allowed credential <see cref="Identifier"/>s from which to get the assertion.
    /// </param>
    /// <returns>The generated HMAC secrets.</returns>
    public GetHmacSecretResponse GetHmacSecret(
        RelyingPartyInfo rpInfo,
        HmacSecret salt,
        HmacSecret? secondSalt = null,
        IReadOnlyList<Identifier>? allowedCredentialIds = null)
    {
        ArgumentNullException.ThrowIfNull(rpInfo);

        if (salt.InternalSecretData.Length != 32)
        {
            throw new ArgumentException("The salt must be exactly 32 bytes long.", nameof(salt));
        }

        if (secondSalt is not null && secondSalt.InternalSecretData.Length != 32)
        {
            throw new ArgumentException("The second salt must be exactly 32 bytes long.", nameof(secondSalt));
        }

        return this.PasskeyProvider.GetHmacSecret(rpInfo, salt, secondSalt, allowedCredentialIds ?? []);
    }

    /// <summary>
    /// Makes a new passkey credential with an HMAC secret.
    /// 
    /// The user may save the created passkey to a device that does not support HMAC secrets.
    /// In that case, this method will throw.
    /// </summary>
    /// <param name="rpInfo">Details about the Relying Party that the credential will be registered for.</param>
    /// <param name="userInfo">Details about the user that the credential will be registered for.</param>
    /// <param name="salt">An salt to use to optionally generate an HMAC secret as part of credential creation.</param>
    /// <param name="secondSalt">An optional second salt to use in the HMAC secret generation. If provided, <paramref name="salt"/> is required too.</param>
    /// <param name="userVerificationRequirement">A requested <see cref="UserVerificationRequirement"/>, which may not necessarily be honoured.</param>
    /// <returns>A <see cref="MakeCredentialResponse"/> object instance with information about the newly created credential.</returns>
    public MakeCredentialResponse MakeCredentialWithHmacSecret(
        RelyingPartyInfo rpInfo,
        UserEntityInfo userInfo,
        HmacSecret? salt = null,
        HmacSecret? secondSalt = null,
        UserVerificationRequirement userVerificationRequirement = UserVerificationRequirement.Discouraged)
    {
        ArgumentNullException.ThrowIfNull(rpInfo);
        ArgumentNullException.ThrowIfNull(userInfo);

        if (salt is not null && salt.InternalSecretData.Length != 32)
        {
            throw new ArgumentException("The salt must be exactly 32 bytes long.", nameof(salt));
        }

        if (secondSalt is not null)
        {
            if (salt is null)
            {
                throw new ArgumentException("The first salt must be provided if a second salt is provided.", nameof(salt));
            }

            if (secondSalt.InternalSecretData.Length != 32)
            {
                throw new ArgumentException("The second salt must be exactly 32 bytes long.", nameof(secondSalt));
            }
        }

        return this.PasskeyProvider.MakeCredentialWithHmacSecret(rpInfo, userInfo, salt, secondSalt, userVerificationRequirement);
    }
}