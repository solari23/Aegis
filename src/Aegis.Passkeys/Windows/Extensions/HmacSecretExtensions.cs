using Aegis.Passkeys.Windows.Structures;

namespace Aegis.Passkeys.Windows.Extensions;

internal class HmacSecretExtensions
{
    public const string Identifier = "hmac-secret";

    // MakeCredential Input Type:   BOOL.
    //   - pvExtension must point to a BOOL with the value TRUE.
    //   - cbExtension must contain the sizeof(BOOL).
    public class Input : InputExtension
    {
        public bool MakeHmacCredential { get; init; } = true;

        public override WEBAUTHN_EXTENSION ToRawExtensionData()
        {
            Int32 value = this.MakeHmacCredential ? 1 : 0;
            var ret = new WEBAUTHN_EXTENSION
            {
                pwszExtensionIdentifier = Identifier,
                extension = BitConverter.GetBytes(value),
            };
            return ret;
        }
    }

    // MakeCredential Output Type:  BOOL.
    //   - pvExtension will point to a BOOL with the value TRUE if credential
    //     was successfully created with HMAC_SECRET.
    //   - cbExtension will contain the sizeof(BOOL).
    public class Output : OutputExtension
    {
        public Output(WEBAUTHN_EXTENSION rawExtensionData) : base(Identifier, rawExtensionData)
        {
            // Empty
        }

        public bool WasHmacCredentialMade { get; private set; }

        protected override void Parse(WEBAUTHN_EXTENSION rawExtensionData)
        {
            if (rawExtensionData.extension?.Length != 4)
            {
                throw new ArgumentException("Unexpected extension data for hmac-secret output extension");
            }

            this.WasHmacCredentialMade = BitConverter.ToBoolean(rawExtensionData.extension, 0);
        }
    }
}
