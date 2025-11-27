using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Aegis.Core;
using Aegis.Core.CredentialsInterface;
using Aegis.Core.Crypto;
using Aegis.Models;

namespace Aegis.CredentialsInterface;

/// <summary>
/// Implementation of <see cref="IUserSecretEntryInterface"/> for interacting with certificates from the command line.
/// </summary>
public class CertificatePickerInterface : IUserSecretEntryInterface
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CertificatePickerInterface"/> class.
    /// </summary>
    /// <param name="ioStreamSet">The IO streams.</param>
    public CertificatePickerInterface(IOStreamSet ioStreamSet)
    {
        ArgCheck.NotNull(ioStreamSet);

        this.IO = ioStreamSet;
    }

    /// <inheritdoc />
    public SecretKind ProvidedSecretKind => SecretKind.RsaKeyFromCertificate;

    /// <inheritdoc />
    public bool IsCurrentPlatformSupported() => true;

    /// <summary>
    /// Gets the IO streams.
    /// </summary>
    private IOStreamSet IO { get; }

    /// <inheritdoc />
    public bool CanProvideSecret(SecretMetadata secretMetadata) => true;

    /// <inheritdoc />
    public UserKeyAuthorizationParameters GetNewKeyAuthorizationParameters()
    {
        var useExistingCertPrompt = new ConfirmationPrompt(this.IO, "Would you like to use an existing certificate?");
        var useExistingCert = useExistingCertPrompt.GetConfirmation();

        using var certificate = useExistingCert
            ? PromptForCertificatePfx(this.IO)
            : CreateNewCertificateInStore(this.IO);

        var certSubjectName = certificate.GetNameInfo(X509NameType.SimpleName, false);

        return new UserKeyAuthorizationParameters(RawUserSecret.FromCertificate(certificate))
        {
            FriendlyName = certSubjectName,
            SecretMetadata = new RsaKeyFromCertificateSecretMetadata
            {
                Thumbprint = certificate.Thumbprint,
            },
        };
    }

    /// <inheritdoc />
    public RawUserSecret GetUserSecret(ImmutableArray<SecretMetadata> possibleSecretMetadata)
    {
        var possibleThumbprints = possibleSecretMetadata
            .Select(md => (md as RsaKeyFromCertificateSecretMetadata)?.Thumbprint)
            .Where(t => t is not null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (TryGetCertificateFromStore(possibleThumbprints, out var certificate))
        {
            this.IO.Out.WriteLine(
                $"Using certificate {certificate.Subject} with thumbprint {certificate.Thumbprint} from CurrentUser\\My store.");
        }
        else
        {
            certificate = PromptForCertificatePfx(this.IO);

            if (!possibleThumbprints.Contains(certificate.Thumbprint))
            {
                throw new AegisUserErrorException(
                    "The key for the certificate is not authorized.",
                    isRecoverable: true);
            }
        }

        return RawUserSecret.FromCertificate(certificate);
    }

    private static X509Certificate2 CreateNewCertificateInStore(IOStreamSet ioStreamSet)
    {
        var subjectNamePrompt = new InputPrompt(ioStreamSet, "Enter a subject name for the new certificate: ");
        var subjectName = subjectNamePrompt.GetValue();

        if (!subjectName.StartsWith("CN="))
        {
            subjectName = $"CN={subjectName}";
        }

        using var rsaKey = RSA.Create(4096);
        var csr = new CertificateRequest(subjectName, rsaKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        // Set extension values (Key Usage and SKI)
        csr.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment, critical: true));
        csr.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(csr.PublicKey, false));

        using var cert = csr.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1000));

        // Export and recreate the certificate to make it correctly exportable.
        var exportableCert = X509CertificateLoader.LoadPkcs12(
            cert.Export(X509ContentType.Pfx),
            password: null,  // No password
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.UserKeySet);

        using X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        certStore.Open(OpenFlags.ReadWrite);
        certStore.Add(exportableCert);
        certStore.Close();

        return exportableCert;
    }

    private static X509Certificate2 PromptForCertificatePfx(IOStreamSet ioStreamSet)
    {
        var pfxPathPrompt = new InputPrompt(
            ioStreamSet,
            "Enter path to certificate PFX file: ",
            validator: File.Exists);
        var pfxPath = pfxPathPrompt.GetValue();

        var pfxPasswordPrompt = new InputPrompt(
            ioStreamSet,
            "Enter password for PFX: ",
            isConfidentialInput: true,
            allowEmptyInput: true);
        var pfxPassword = pfxPasswordPrompt.GetValue();

        X509Certificate2 certificate = null;

        try
        {
            certificate = X509CertificateLoader.LoadPkcs12FromFile(pfxPath, pfxPassword, X509KeyStorageFlags.Exportable);
        }
        catch (CryptographicException e)
        {
            throw new AegisUserErrorException(
                "The password to the PFX file is not correct.",
                isRecoverable: true,
                innerException: e);
        }

        if (!certificate.HasPrivateKey)
        {
            throw new AegisUserErrorException(
                "The PFX file does not contain a private key.",
                isRecoverable: true);
        }

        return certificate;
    }

    private static bool TryGetCertificateFromStore(
        IReadOnlyCollection<string> possibleThumbprints,
        out X509Certificate2 certificate)
    {
        using X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        certStore.Open(OpenFlags.ReadOnly);

        certificate = certStore.Certificates.FirstOrDefault(cert => 
            cert.HasPrivateKey
            && possibleThumbprints.Contains(cert.Thumbprint, StringComparer.OrdinalIgnoreCase));

        certStore.Close();
        return certificate != null;
    }
}
