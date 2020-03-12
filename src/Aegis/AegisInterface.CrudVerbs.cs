using System;
using System.IO;
using System.Text;

using Aegis.Core;

using static Aegis.CommandLineVerbs;

namespace Aegis
{
    // Fragment of AegisInterface that implements the CRUD verbs (e.g. create, add, list, etc..).
    public partial class AegisInterface
    {
        /// <summary>
        /// Implements the Aegis 'create' verb.
        /// </summary>
        /// <param name="options">The verb options.</param>
        /// <returns>Whether or not the operation was handled.</returns>
        private bool CreateVerb(CreateVerbOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.AegisArchivePath))
            {
                throw new AegisUserErrorException("The archive path parameter is required for this operation.");
            }

            if (File.Exists(options.AegisArchivePath) && !options.Force)
            {
                throw new AegisUserErrorException(
                    "A file exists at the specified location. Use --force flag to overwrite.");
            }

            // TODO: Implement credential selection and input
            var userKeyFriendlyName = "Password";
            var rawUserSecret = Encoding.UTF8.GetBytes(TEMP_Password);

            var createParameters = new SecureArchiveCreationParameters(userKeyFriendlyName, rawUserSecret);
            var fileSettings = new SecureArchiveFileSettings(options.AegisArchivePath, this.TempDirectory);

            try
            {
                using var archive = AegisArchive.CreateNew(fileSettings, createParameters);

                var newArchiveFileInfo = new FileInfo(archive.FullFilePath);
                Console.WriteLine($"Created new secure archive file '{newArchiveFileInfo.FullName}'.");
            }
            catch (IOException e)
            {
                throw new AegisUserErrorException($"Unable to write to {fileSettings.Path}.", innerException: e);
            }

            return true;
        }
    }
}
