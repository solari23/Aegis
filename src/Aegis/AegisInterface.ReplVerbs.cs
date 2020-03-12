using System;

using static Aegis.CommandLineVerbs;

namespace Aegis
{
    // Fragment of AegisInterface that implements the REPL verbs (e.g. open/close and meta verbs).
    public partial class AegisInterface
    {
        /// <summary>
        /// Implements the Aegis 'open' verb.
        /// </summary>
        /// <param name="options">The verb options.</param>
        /// <returns>Whether or not the operation was handled.</returns>
        private bool OpenVerb(OpenVerbOptions options)
        {
            if (this.Archive != null
                && !options.Force)
            {
                throw new AegisUserErrorException(
                    "Another archive is already opened. Either close the opened archive or use the --force flag.");
            }

            var archive = OpenArchive(options.AegisArchivePath, this.TempDirectory);

            // The new archive is fully loaded. Close out the previous one, if there was one.
            this.CloseVerb(new CloseVerbOptions());

            this.Archive = archive;
            return this.StartRepl();
        }

        /// <summary>
        /// Implements the Aegis 'close' verb.
        /// </summary>
        /// <param name="options">The verb options.</param>
        /// <returns>Whether or not the operation was handled.</returns>
        private bool CloseVerb(CloseVerbOptions _)
        {
            if (this.Archive is null)
            {
                // There's no archive opened. Do nothing.
                return true;
            }

            this.Archive?.Dispose();
            this.Archive = null;
            this.SetWindowTitle();

            return true;
        }

        /// <summary>
        /// Puts the interface into REPL (Read-Execute-Print-Loop) mode.
        /// </summary>
        /// <returns>'True' to indicate the operation was handled.</returns>
        private bool StartRepl()
        {
            this.SetWindowTitle();
            this.InReplMode = true;
            return true;
        }

        /// <summary>
        /// Signals the exit flag to leave REPL mode.
        /// </summary>
        /// <returns>'True' to indicate the operation was handled.</returns>
        private bool ExitRepl()
        {
            this.InReplMode = false;
            return true;
        }

        /// <summary>
        /// Clears the console screen.
        /// </summary>
        /// <returns>'True' to indicate the operation was handled.</returns>
        private bool ClearScreen()
        {
            Console.Clear();
            return true;
        }

        /// <summary>
        /// Sets the title in the command line window.
        /// </summary>
        private void SetWindowTitle()
        {
            var archiveName = this.Archive is null
                ? "No Archive Selected"
                : this.Archive.FileName;

            Console.Title = $"{Program.Name} <{archiveName}>";
        }
    }
}
