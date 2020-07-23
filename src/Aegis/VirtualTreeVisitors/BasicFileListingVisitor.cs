using System;
using System.IO;

using Aegis.Core.FileSystem;

namespace Aegis.VirtualTreeVisitors
{
    /// <summary>
    /// Implementation of <see cref="IVirtualFileTreeVisitor"/> that prints the names of files
    /// to the supplied output stream.
    /// </summary>
    public class BasicFileListingVisitor : IVirtualFileTreeVisitor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BasicFileListingVisitor"/> class.
        /// </summary>
        /// <param name="output">The output stream to write the listing to.</param>
        public BasicFileListingVisitor(TextWriter output)
        {
            this.Output = output;
        }

        /// <summary>
        /// Gets the output stream to write the listing to.
        /// </summary>
        private TextWriter Output { get; }

        /// <summary>
        /// Gets or sets a counter that tracks the number of files in the archive.
        /// </summary>
        private int FileCount { get; set; }

        /// <inheritdoc/>
        public void OnStart()
        {
            this.Output.WriteLine();
        }

        /// <inheritdoc/>
        public void OnPreOrderVisit(AegisVirtualDirectoryPath directory, ReadOnlySpan<AegisFileInfo> files)
        {
            this.Output.WriteLine($" Directory of {directory}");
            this.Output.WriteLine();

            foreach (var file in files)
            {
                this.Output.WriteLine($"{file.LastModifiedTime.ToLocalTime():yyyy-MM-dd hh:mm tt}   {file.Path.FileName}");
            }

            this.Output.WriteLine();
            this.Output.WriteLine();

            this.FileCount += files.Length;
        }

        /// <inheritdoc/>
        public void OnPostOrderVisit(AegisVirtualDirectoryPath directory, ReadOnlySpan<AegisFileInfo> files)
        {
            // Empty.
        }

        /// <inheritdoc/>
        public void OnDone()
        {
            this.Output.WriteLine($"-- The archive contains {this.FileCount} files --");
            this.Output.WriteLine();
        }
    }
}
