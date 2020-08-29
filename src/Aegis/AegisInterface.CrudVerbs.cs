using System;
using System.IO;
using System.Text;

using Aegis.Core;
using Aegis.Core.CredentialsInterface;
using Aegis.Core.FileSystem;
using Aegis.Models;
using Aegis.VirtualTreeVisitors;

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

                using var firstUserKeyAuthorization = this.ArchiveUnlocker.GetNewUserKeyAuthorization();
                using var createParameters = new SecureArchiveCreationParameters(firstUserKeyAuthorization);

                using var archive = AegisArchive.CreateNew(archiveFileSettings, createParameters);

                var newArchiveFileInfo = new FileInfo(archive.FullFilePath);
                this.IO.Out.WriteLine($"Created new secure archive file '{newArchiveFileInfo.FullName}'.");
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
            // If the virtual path isn't specified, the default is to add the file to
            // the root of the archive with the same file name.
            var addPath = string.IsNullOrWhiteSpace(options.ArchiveVirtualPath)
                ? new AegisVirtualFilePath(Path.GetFileName(options.FilePath))
                : new AegisVirtualFilePath(options.ArchiveVirtualPath);

            var existingFileInfo = this.Archive.GetFileInfo(addPath);

            if (existingFileInfo != null && !options.Force)
            {
                throw new AegisUserErrorException(
                    $"Can't overwrite existing file at '{addPath}'. If you want to overwrite, use the --force flag or the 'update' verb instead.");
            }

            if (!File.Exists(options.FilePath))
            {
                throw new AegisUserErrorException($"No file found on local disk at location '{options.FilePath}'.");
            }

            try
            {
                using var inputFileStream = File.OpenRead(options.FilePath);
                var newFileInfo = this.Archive.PutFile(addPath, inputFileStream, options.Force);

                if (existingFileInfo?.FileId == newFileInfo.FileId)
                {
                    this.IO.Out.WriteLine($"Archive file ID '{newFileInfo.FileId}' at virtual path '{newFileInfo.Path}' was updated.");
                }
                else
                {
                    this.IO.Out.WriteLine($"New file ID '{newFileInfo.FileId}' created at virtual path '{newFileInfo.Path}'.");
                }
            }
            catch
            {
                // TODO: do a better job of differentiating issues reading the file from internal issues.
                throw;
            }

            return true;
        }

        /// <summary>
        /// Implements the Aegis 'remove' verb.
        /// </summary>
        /// <param name="options">The verb options.</param>
        /// <returns>Whether or not the operation was handled.</returns>
        private bool RemoveVerb(RemoveVerbOptions options)
        {
            if (options.ArchiveFileId == Guid.Empty
                && string.IsNullOrWhiteSpace(options.ArchiveVirtualPath))
            {
                throw new AegisUserErrorException($"Must specify either a file ID or a virtual path to remove.");
            }

            var existingFileInfo = options.ArchiveFileId != Guid.Empty
                ? this.Archive.GetFileInfo(options.ArchiveFileId)
                : this.Archive.GetFileInfo(new AegisVirtualFilePath(options.ArchiveVirtualPath));

            if (existingFileInfo is null)
            {
                var errorMessage = "Can't remove file. ";
                errorMessage += options.ArchiveFileId != Guid.Empty
                    ? $"File with ID '{options.ArchiveFileId}' does not exist."
                    : $"No file exists at archive virtual path '{options.ArchiveVirtualPath}'.";
                throw new AegisUserErrorException(errorMessage);
            }

            this.Archive.RemoveFile(existingFileInfo.FileId);
            this.IO.Out.WriteLine($"Removed file ID '{existingFileInfo.FileId}' from virtual path '{existingFileInfo.Path}'.");

            return true;
        }

        /// <summary>
        /// Implements the Aegis 'extract' verb.
        /// </summary>
        /// <param name="options">The verb options.</param>
        /// <returns>Whether or not the operation was handled.</returns>
        private bool ExtractVerb(ExtractVerbOptions options)
        {
            if (options.ArchiveFileId == Guid.Empty
                && string.IsNullOrWhiteSpace(options.ArchiveVirtualPath))
            {
                throw new AegisUserErrorException($"Must specify either a file ID or a virtual path to extract.");
            }

            var existingFileInfo = options.ArchiveFileId != Guid.Empty
                ? this.Archive.GetFileInfo(options.ArchiveFileId)
                : this.Archive.GetFileInfo(new AegisVirtualFilePath(options.ArchiveVirtualPath));

            if (existingFileInfo is null)
            {
                var errorMessage = "Can't extract file. ";
                errorMessage += options.ArchiveFileId != Guid.Empty
                    ? $"File with ID '{options.ArchiveFileId}' does not exist."
                    : $"No file exists at archive virtual path '{options.ArchiveVirtualPath}'.";
                throw new AegisUserErrorException(errorMessage);
            }

            if (string.IsNullOrWhiteSpace(options.OutputDirectoryPath))
            {
                options.OutputDirectoryPath = ".";
            }
            else
            {
                if (!Directory.Exists(options.OutputDirectoryPath))
                {
                    throw new AegisUserErrorException(
                        $"Output directory '{options.OutputDirectoryPath}' does not exist. Please create it first before extracting to it.");
                }
            }

            try
            {
                var outputFilePath = Path.Join(options.OutputDirectoryPath, existingFileInfo.Path.FileName);

                this.IO.Out.WriteLine(
                    $"Extracting file {existingFileInfo.FileId} @ virtual path '{existingFileInfo.Path}' to local path '{outputFilePath}'");
                using var fileStream = File.OpenWrite(outputFilePath);
                this.Archive.ExtractFile(existingFileInfo, fileStream);
                this.IO.Out.WriteLine("Extraction complete!");
            }
            catch
            {
                // TODO: Improved error handling during extraction.
                throw;
            }

            return true;
        }

        /// <summary>
        /// Implements the Aegis 'update' verb.
        /// </summary>
        /// <param name="options">The verb options.</param>
        /// <returns>Whether or not the operation was handled.</returns>
        private bool UpdateVerb(UpdateVerbOptions options)
        {
            if (options.ArchiveFileId == Guid.Empty
                && string.IsNullOrWhiteSpace(options.ArchiveVirtualPath))
            {
                throw new AegisUserErrorException($"Must specify either a file ID or a virtual path to update.");
            }

            var existingFileInfo = options.ArchiveFileId != Guid.Empty
                ? this.Archive.GetFileInfo(options.ArchiveFileId)
                : this.Archive.GetFileInfo(new AegisVirtualFilePath(options.ArchiveVirtualPath));

            if (existingFileInfo is null)
            {
                var errorMessage = "Can't update file. ";
                errorMessage += options.ArchiveFileId != Guid.Empty
                    ? $"File with ID '{options.ArchiveFileId}' does not exist."
                    : $"No file exists at path '{options.ArchiveVirtualPath}'.";
                throw new AegisUserErrorException(errorMessage);
            }

            this.AddVerb(new AddVerbOptions
            {
                ArchiveVirtualPath = existingFileInfo.Path.ToString(),
                FilePath = options.FilePath,
                Force = true,
            });

            return true;
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

            switch (listTargetType)
            {
                case ListVerbTargetType.Files:
                    this.Archive.TraverseFileTree(new BasicFileListingVisitor(this.IO.Out));
                    break;

                case ListVerbTargetType.Keys:
                    this.IO.Out.WriteLine("Authorized Keys:");
                    this.IO.Out.WriteLine("================");

                    foreach (var key in this.Archive.GetUserKeyAuthorizations())
                    {
                        var kind = key.SecretMetadata.SecretKind;
                        var friendlyName = key.FriendlyName;
                        var timeAdded = key.TimeAdded.ToLocalTime();

                        this.IO.Out.WriteLine($"[{kind}] {friendlyName} (Added on: {timeAdded:yyyy-MM-dd hh:mm tt})");
                    }

                    break;

                case ListVerbTargetType.Metadata:
                    this.IO.Out.WriteLine(this.Archive.GetArchiveMetadataJson());
                    break;

                default:
                    throw new InvalidOperationException($"Unhandled listing target of type '{listTargetType}'.");
            }

            return true;
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
