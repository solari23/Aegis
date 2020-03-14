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
        /// The types of 'list' verb targets that the user can specify via the <see cref="ListVerbOptions.ListType"/> option.
        /// </summary>
        public enum ListVerbTargetType
        {
            /// <summary>
            /// The command is to list files.
            /// </summary>
            Files,

            /// <summary>
            /// The command is to list authorized keys.
            /// </summary>
            Keys,

            /// <summary>
            /// Hidden option to list archive metadata information.
            /// </summary>
            Metadata,
        }

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

            // Add the canonical "ags" extension to the new archive, if it's not already part of the path.
            var newArchivePath = options.AegisArchivePath.EndsWith(
                $".{AegisConstants.CanonicalSecureArchiveFileExtension}",
                StringComparison.OrdinalIgnoreCase)
                    ? options.AegisArchivePath
                    : $"{options.AegisArchivePath}.{AegisConstants.CanonicalSecureArchiveFileExtension}";

            var archiveFileSettings = new SecureArchiveFileSettings(newArchivePath, this.TempDirectory);

            try
            {
                if (File.Exists(archiveFileSettings.Path))
                {
                    if (options.Force)
                    {
                        // Get rid of the pre-existing archive file first.
                        File.Delete(archiveFileSettings.Path);
                    }
                    else
                    {
                        throw new AegisUserErrorException(
                            "A file exists at the specified location. Use --force flag to overwrite.");
                    }
                }

                // TODO: Implement credential selection and input
                var userKeyFriendlyName = "Password";
                var rawUserSecret = Encoding.UTF8.GetBytes(TEMP_Password);

                var createParameters = new SecureArchiveCreationParameters(userKeyFriendlyName, rawUserSecret);
                

                using var archive = AegisArchive.CreateNew(archiveFileSettings, createParameters);

                var newArchiveFileInfo = new FileInfo(archive.FullFilePath);
                Console.WriteLine($"Created new secure archive file '{newArchiveFileInfo.FullName}'.");
            }
            catch (IOException e)
            {
                throw new AegisUserErrorException($"Unable to write to {archiveFileSettings.Path}.", innerException: e);
            }

            return true;
        }

        /// <summary>
        /// Implements the Aegis 'add' verb.
        /// </summary>
        /// <param name="options">The verb options.</param>
        /// <returns>Whether or not the operation was handled.</returns>
        private bool AddVerb(AddVerbOptions options)
        {
            // TODO: Implement the 'add' verb.
            Console.WriteLine($"[DEBUG] Command will attempt to add {options.FilePath} to the archive");
            return false;
        }

        /// <summary>
        /// Implements the Aegis 'remove' verb.
        /// </summary>
        /// <param name="options">The verb options.</param>
        /// <returns>Whether or not the operation was handled.</returns>
        private bool RemoveVerb(RemoveVerbOptions options)
        {
            // TODO: Implement the 'remove' verb.
            return false;
        }

        /// <summary>
        /// Implements the Aegis 'update' verb.
        /// </summary>
        /// <param name="options">The verb options.</param>
        /// <returns>Whether or not the operation was handled.</returns>
        private bool UpdateVerb(UpdateVerbOptions options)
        {
            // TODO: Implement the 'update' verb.
            return false;
        }

        /// <summary>
        /// Implements the Aegis 'list' verb.
        /// </summary>
        /// <param name="options">The verb options.</param>
        /// <returns>Whether or not the operation was handled.</returns>
        private bool ListVerb(ListVerbOptions options)
        {
            if (!Enum.TryParse<ListVerbTargetType>(options.ListType, ignoreCase: true, out var listTargetType))
            {
                throw new AegisUserErrorException(
                    $"Can't list '{options.ListType}'. This command can list 'files' or 'keys'.");
            }

            // TODO: Implement the 'list' verb.
            Console.WriteLine(
                $"[DEBUG] Command will list {listTargetType}" + (listTargetType == ListVerbTargetType.Files && options.TreeView ? " in tree view" : string.Empty));
            return false;
        }

        /// <summary>
        /// Implements the Aegis 'authorize' verb.
        /// </summary>
        /// <param name="options">The verb options.</param>
        /// <returns>Whether or not the operation was handled.</returns>
        private bool AuthorizeVerb(AuthorizeVerbOptions options)
        {
            // TODO: Implement the 'authorize' verb.
            // This is pending fleshing out the user keys subsystem.
            return false;
        }

        /// <summary>
        /// Implements the Aegis 'revoke' verb.
        /// </summary>
        /// <param name="options">The verb options.</param>
        /// <returns>Whether or not the operation was handled.</returns>
        private bool RevokeVerb(RevokeVerbOptions options)
        {
            // TODO: Implement the 'revoke' verb.
            // This is pending fleshing out the user keys subsystem.
            return false;
        }
    }
}
