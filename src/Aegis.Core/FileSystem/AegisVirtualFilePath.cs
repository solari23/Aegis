using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Aegis.Core.FileSystem
{
    /// <summary>
    /// Represents a virtual file path in an Aegis archive.
    /// </summary>
    public class AegisVirtualFilePath : IEquatable<AegisVirtualFilePath>, IComparable<AegisVirtualFilePath>
    {
        /// <summary>
        /// The canonical separator used in Aegis virtual paths.
        /// </summary>
        public const char PathSeparator = '/';

        /// <summary>
        /// Characters that can be parsed as path separators when constructing a virtual path.
        /// </summary>
        public static readonly ImmutableArray<char> ParsablePathSeparators = ImmutableArray.Create(parsablePathSeparators);

        /// <summary>
        /// Private copy of the parsable path separators for usage with functions like <see cref="string.Split(char[])"/>.
        /// </summary>
        private static readonly char[] parsablePathSeparators = new[] { '/', '\\' };

        /// <summary>
        /// Initializes a new instance of the <see cref="AegisVirtualFilePath"/> class.
        /// </summary>
        /// <param name="path">The string representation of the virtual path.</param>
        public AegisVirtualFilePath(string path)
        {
            ArgCheck.NotEmpty(path, nameof(path));

            this.FullPathComponents = path
                .Split(parsablePathSeparators, StringSplitOptions.RemoveEmptyEntries)
                .ToImmutableArray();
            this.DirectoryPathComponents = this.FullPathComponents.SkipLast(1).ToImmutableArray();

            if (this.FullPathComponents.Length == 0)
            {
                throw new ArgumentException("The input path is not valid.", nameof(path));
            }

            this.FullPath = PathSeparator + string.Join(PathSeparator, this.FullPathComponents);
        }

        /// <summary>
        /// Gets the individual components making up the full virtual path (including the filename).
        /// </summary>
        public ImmutableArray<string> FullPathComponents { get; }

        /// <summary>
        /// Gets the individual components making the virtual directory path (without the filename).
        /// </summary>
        public ImmutableArray<string> DirectoryPathComponents { get; }

        /// <summary>
        /// Gets the full string representation of the path.
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// Gets the name of the file at the end of the path.
        /// </summary>
        public string FileName => this.FullPathComponents.Last();

        /// <inheritdoc/>
        public override string ToString() => this.FullPath;

        /// <inheritdoc/>
        public override int GetHashCode() => this.FullPath.GetHashCode(StringComparison.OrdinalIgnoreCase);

        #region Implementation of IEquatable and IComparable

        /// <inheritdoc/>
        public int CompareTo([AllowNull]AegisVirtualFilePath other)
        {
            const int BothEqual = 0;
            const int ThisPreceedsOther = -1;
            const int ThisFollowsOther = 1;

            if (other is null)
            {
                return ThisFollowsOther;
            }

            var thisComponents = this.DirectoryPathComponents;
            var otherComponents = other.DirectoryPathComponents;

            // Compare as many components of directory paths as we can.
            var numComponentsToCompare = Math.Min(thisComponents.Length, otherComponents.Length);

            for (int i = 0; i < numComponentsToCompare; i++)
            {
                var componentComparisonResult = string.Compare(thisComponents[i], otherComponents[i], StringComparison.OrdinalIgnoreCase);

                if (componentComparisonResult == BothEqual)
                {
                    continue;
                }

                return componentComparisonResult < 0 ? ThisPreceedsOther : ThisFollowsOther;
            }

            // At this point, all the directory path components that we've compared are the same.
            // Either both directories are the same (i.e. same number of components) -> order based on file names
            // or one directory is a parent of the other (i.e. fewer components) -> parent preceeds other.
            return thisComponents.Length == otherComponents.Length
                ? string.Compare(this.FileName, other.FileName, StringComparison.OrdinalIgnoreCase)
                : thisComponents.Length - otherComponents.Length;
        }

        /// <inheritdoc/>
        public bool Equals([AllowNull]AegisVirtualFilePath other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other is null
                ? false
                : string.Equals(this.FullPath, other.FullPath, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => this.Equals(obj as AegisVirtualFilePath);

        /// <inheritdoc/>
        public static bool operator ==(AegisVirtualFilePath left, AegisVirtualFilePath right) => left is null ? right is null : left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(AegisVirtualFilePath left, AegisVirtualFilePath right) => !(left == right);

        /// <inheritdoc/>
        public static bool operator <(AegisVirtualFilePath left, AegisVirtualFilePath right) => left is null ? right is object : left.CompareTo(right) < 0;

        /// <inheritdoc/>
        public static bool operator <=(AegisVirtualFilePath left, AegisVirtualFilePath right) => left is null || left.CompareTo(right) <= 0;

        /// <inheritdoc/>
        public static bool operator >(AegisVirtualFilePath left, AegisVirtualFilePath right) => left is object && left.CompareTo(right) > 0;

        /// <inheritdoc/>
        public static bool operator >=(AegisVirtualFilePath left, AegisVirtualFilePath right) => left is null ? right is null : left.CompareTo(right) >= 0;

        #endregion
    }
}
