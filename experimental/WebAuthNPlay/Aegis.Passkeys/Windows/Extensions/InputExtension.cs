using Aegis.Passkeys.Windows.Structures;

namespace Aegis.Passkeys.Windows.Extensions;

internal abstract class InputExtension
{
    public abstract WEBAUTHN_EXTENSION ToRawExtensionData();
}
