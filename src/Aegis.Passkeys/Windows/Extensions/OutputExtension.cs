using Aegis.Passkeys.Windows.Structures;

namespace Aegis.Passkeys.Windows.Extensions;

internal abstract class OutputExtension
{
    public static List<OutputExtension> ParseRawExtensions(IEnumerable<WEBAUTHN_EXTENSION> extensions)
    {
        var parsedExtensions = new List<OutputExtension>();

        foreach (var ext in extensions)
        {
            var parsed = ext.pwszExtensionIdentifier switch
            {
                HmacSecretExtensions.Identifier => new HmacSecretExtensions.Output(ext),
                _ => null,
            };

            if (parsed is not null)
            {
                parsedExtensions.Add(parsed);
            }
        }

        return parsedExtensions;
    }

    public OutputExtension(string expectedIdentifier, WEBAUTHN_EXTENSION rawExtensionData)
    {
        if (rawExtensionData.pwszExtensionIdentifier != expectedIdentifier)
        {
            throw new ArgumentException($"Expected extension identifier '{expectedIdentifier}', got '{rawExtensionData.pwszExtensionIdentifier}'");
        }

        this.Parse(rawExtensionData);
    }

    protected abstract void Parse(WEBAUTHN_EXTENSION rawExtensionData);
}
