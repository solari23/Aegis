using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Aegis.Core.FileSystem;

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
    /// Splits a path string by path separators into it's individual components.
    /// </summary>
    /// <param name="path">The path to split.</param>
    /// <returns>The split directory components.</returns>
    /// <example>
    /// SplitPathComponents("/foo/bar/baz.txt") -> ["foo", "bar", "baz.txt"]
    /// </example>
    public static ImmutableArray<string> SplitPathComponents(string path)
    {
        ArgCheck.NotEmpty(path);

        return path
            .Split(parsablePathSeparators, StringSplitOptions.RemoveEmptyEntries)
            .ToImmutableArray();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AegisVirtualFilePath"/> class.
    /// </summary>
    /// <param name="path">The string representation of the virtual path.</param>
    public AegisVirtualFilePath(string path)
    {
        ArgCheck.NotEmpty(path);

        var pathComponents = SplitPathComponents(path);

        if (pathComponents.Length == 0)
        {
            throw new ArgumentException("The input path is not valid.", nameof(path));
        }

        this.FullPath = PathSeparator + string.Join(PathSeparator, pathComponents);
        this.FileName = pathComponents.Last();
        this.DirectoryPath = new AegisVirtualDirectoryPath(pathComponents.SkipLast(1));
    }

    /// <summary>
    /// Gets the virtual directory path of the file.
    /// </summary>
    public AegisVirtualDirectoryPath DirectoryPath { get; }

    /// <summary>
    /// Gets the full string representation of the path.
    /// </summary>
    public string FullPath { get; }

    /// <summary>
    /// Gets the name of the file at the end of the path.
    /// </summary>
    public string FileName { get; }

    /// <inheritdoc/>
    public override string ToString() => this.FullPath;

    /// <inheritdoc/>
    public override int GetHashCode() => this.FullPath.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Allows implicit conversion of strings to <see cref="AegisVirtualFilePath"/> for cleane code.
    /// </summary>
    /// <param name="path">The string representation of the virtual path.</param>
    public static implicit operator AegisVirtualFilePath(string path) => new AegisVirtualFilePath(path);

    #region Implementation of IEquatable and IComparable

    /// <inheritdoc/>
    public int CompareTo([AllowNull] AegisVirtualFilePath other)
    {
        const int BothEqual = 0;
        const int ThisFollowsOther = 1;

        if (other is null)
        {
            return ThisFollowsOther;
        }

        if (this.Equals(other))
        {
            return BothEqual;
        }

        // If the files are in the same directory, order is based on file name.
        // Otherwise order is based on the directories that they are in.
        return this.DirectoryPath == other.DirectoryPath
            ? string.Compare(this.FileName, other.FileName, StringComparison.OrdinalIgnoreCase)
            : this.DirectoryPath.CompareTo(other.DirectoryPath);
    }

    /// <inheritdoc/>
    public bool Equals([AllowNull] AegisVirtualFilePath other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return other is not null
            && string.Equals(this.FullPath, other.FullPath, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) => this.Equals(obj as AegisVirtualFilePath);

    /// <inheritdoc/>
    public static bool operator ==(AegisVirtualFilePath left, AegisVirtualFilePath right) => left is null ? right is null : left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(AegisVirtualFilePath left, AegisVirtualFilePath right) => !(left == right);

    /// <inheritdoc/>
    public static bool operator <(AegisVirtualFilePath left, AegisVirtualFilePath right) => left is null ? right is not null : left.CompareTo(right) < 0;

    /// <inheritdoc/>
    public static bool operator <=(AegisVirtualFilePath left, AegisVirtualFilePath right) => left is null || left.CompareTo(right) <= 0;

    /// <inheritdoc/>
    public static bool operator >(AegisVirtualFilePath left, AegisVirtualFilePath right) => left is not null && left.CompareTo(right) > 0;

    /// <inheritdoc/>
    public static bool operator >=(AegisVirtualFilePath left, AegisVirtualFilePath right) => left is null ? right is null : left.CompareTo(right) >= 0;

    #endregion
}
