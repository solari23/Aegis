﻿using System;
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

            var archiveFileSettings = new SecureArchiveFileSettings(options.AegisArchivePath, this.TempDirectory);

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
            // TODO: Implement the 'list' verb.
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