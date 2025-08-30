using System.Runtime.InteropServices.Marshalling;

using Aegis.Passkeys.Marshalling;

namespace Aegis.Passkeys.Windows.Structures;

[NativeMarshalling(typeof(Marshaller))]
internal struct WEBAUTHN_COSE_CREDENTIAL_PARAMETERS
{
    public static WEBAUTHN_COSE_CREDENTIAL_PARAMETERS MakeDefault() => new()
    {
        credentialParameters =
        [
            new WEBAUTHN_COSE_CREDENTIAL_PARAMETER
            {
                lAlg = WEBAUTHN_COSE_ALGORITHM.ECDSA_P256_WITH_SHA256,
            },
        ],
    };

    // DWORD cCredentialParameters
    // PWEBAUTHN_COSE_CREDENTIAL_PARAMETER pCredentialParameters
    public WEBAUTHN_COSE_CREDENTIAL_PARAMETER[] credentialParameters;

    [CustomMarshaller(typeof(WEBAUTHN_COSE_CREDENTIAL_PARAMETERS), MarshalMode.ManagedToUnmanagedRef, typeof(Marshaller))]
    internal static unsafe class Marshaller
    {
        internal struct Unmanaged
        {
            public uint cCredentialParameters;
            public IntPtr pCredentialParameters;
        }

        public static Unmanaged ConvertToUnmanaged(WEBAUTHN_COSE_CREDENTIAL_PARAMETERS managed)
        {
            var marshalledCredentialParameters = SizePrefixedContiguousStuctArray.From(
                managed.credentialParameters,
                WEBAUTHN_COSE_CREDENTIAL_PARAMETER.Marshaller.ConvertToUnmanaged);

            var ret = new Unmanaged
            {
                cCredentialParameters = marshalledCredentialParameters.NumElements,
                pCredentialParameters = marshalledCredentialParameters.Pointer,
            };
            return ret;
        }

        public static WEBAUTHN_COSE_CREDENTIAL_PARAMETERS ConvertToManaged(Unmanaged unmanaged)
        {
            var ret = new WEBAUTHN_COSE_CREDENTIAL_PARAMETERS
            {
                credentialParameters = new SizePrefixedContiguousStuctArray(unmanaged.cCredentialParameters, unmanaged.pCredentialParameters)
                    .ToManagedArray<WEBAUTHN_COSE_CREDENTIAL_PARAMETER, WEBAUTHN_COSE_CREDENTIAL_PARAMETER.Marshaller.Unmanaged>(
                        WEBAUTHN_COSE_CREDENTIAL_PARAMETER.Marshaller.ConvertToManaged)
                    ?? [],
            };
            return ret;
        }

        public static void Free(Unmanaged unmanaged)
        {
            new SizePrefixedContiguousStuctArray(unmanaged.cCredentialParameters, unmanaged.pCredentialParameters)
                .Free<WEBAUTHN_COSE_CREDENTIAL_PARAMETER.Marshaller.Unmanaged>(WEBAUTHN_COSE_CREDENTIAL_PARAMETER.Marshaller.Free);
        }
    }
}
