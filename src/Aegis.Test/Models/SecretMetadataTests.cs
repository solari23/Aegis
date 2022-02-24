using Aegis.Models;
using Aegis.Models.JsonConverters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aegis.Test.Models;

[TestClass]
public class SecretMetadataTests
{
    [TestMethod]
    [Description("Ensures that all SecretKind values have an associated polymorphic serialization converter.")]
    public void EnsureAllSecretKindsHavePolymorphicConverter()
    {
        foreach (var secretKindEnumName in Enum.GetNames(typeof(SecretKind)))
        {
            var enumValue = Enum.Parse<SecretKind>(secretKindEnumName);

            if (enumValue == SecretKind.Unknown)
            {
                // Invalid value -> won't have a converter.
                continue;
            }

            Assert.IsTrue(SecretMetadataPolymorphicConverter.TypeDiscriminatorMappings.ContainsKey(enumValue));
        }
    }
}
