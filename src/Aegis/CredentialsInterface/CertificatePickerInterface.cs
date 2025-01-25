﻿using System.Collections.Immutable;
using System.Collections.ObjectModel;
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

    /// <summary>
    /// Gets the IO streams.
    /// </summary>
    private IOStreamSet IO { get; }

    /// <inheritdoc />
    public bool CanProvideSecret(SecretMetadata secretMetadata) => true;

    /// <inheritdoc />
    public UserKeyAuthorizationParameters GetNewKeyAuthorizationParameters()
    {
        var subjectNamePrompt = new InputPrompt(this.IO, "Enter a subject name for the new certificate: ");
        var newCertSubjectName = subjectNamePrompt.GetValue();

        using var newCertificate = CreateNewCertificateInStore(newCertSubjectName);

        return new UserKeyAuthorizationParameters(RawUserSecret.FromCertificate(newCertificate))
        {
            FriendlyName = newCertSubjectName,
            SecretMetadata = new RsaKeyFromCertificateSecretMetadata
            {
                Thumbprint = newCertificate.Thumbprint,
            },
        };
    }

    /// <inheritdoc />
    public RawUserSecret GetUserSecret(ImmutableArray<SecretMetadata> possibleSecretMetadata)
    {
        var possibleThumbprints = possibleSecretMetadata
            .Select(md => (md as RsaKeyFromCertificateSecretMetadata)?.Thumbprint)
            .Where(t => t is not null)
            .ToHashSet();

        if (!TryGetCertificateFromStore(possibleThumbprints, out var certificate))
        {
            // TODO: Let the user pick a PFX file + enter password.
            throw new AegisUserErrorException(
                "No authorized certificates available in the certificate store.",
                isRecoverable: true);
        }

        return RawUserSecret.FromCertificate(certificate);
    }

    private static X509Certificate2 CreateNewCertificateInStore(string subjectName)
    {
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
        var exportableCert = new X509Certificate2(
            cert.Export(X509ContentType.Pfx),
            (string)null,  // No password
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.UserKeySet);

        using X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        certStore.Open(OpenFlags.ReadWrite);
        certStore.Add(exportableCert);
        certStore.Close();

        return exportableCert;
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
