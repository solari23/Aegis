using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Aegis.Core.FileSystem;

/// <summary>
/// Represents a virtual directory path in an Aegis archive.
/// </summary>
public class AegisVirtualDirectoryPath : IEquatable<AegisVirtualDirectoryPath>, IComparable<AegisVirtualDirectoryPath>
{
    /// <summary>
    /// Gets a representation of the root directory.
    /// </summary>
    public static AegisVirtualDirectoryPath RootDirectory => new AegisVirtualDirectoryPath(Array.Empty<string>());

    /// <summary>
    /// Initializes a new instance of the <see cref="AegisVirtualDirectoryPath"/> class.
    /// </summary>
    /// <param name="components">The components of the path.</param>
    internal AegisVirtualDirectoryPath(IEnumerable<string> components)
    {
        this.Components = components.ToImmutableArray();
        this.FullPath = AegisVirtualFilePath.PathSeparator
            + string.Join(AegisVirtualFilePath.PathSeparator, components);
    }

    /// <summary>
    /// Gets the individual components of the path.
    /// </summary>
    public ImmutableArray<string> Components { get; }

    /// <summary>
    /// Gets the full directory path as a string.
    /// </summary>
    public string FullPath { get; }

    /// <summary>
    /// Gets the friendly display name of the virtual directory.
    /// </summary>
    public string DisplayName => this.Components.Length == 0
        ? "<root>"
        : this.Components.Last();

    /// <inheritdoc/>
    public override string ToString() => this.FullPath;

    /// <inheritdoc/>
    public override int GetHashCode() => this.FullPath.GetHashCode(StringComparison.OrdinalIgnoreCase);

    #region Implementation of IEquatable and IComparable

    /// <inheritdoc/>
    public int CompareTo([AllowNull] AegisVirtualDirectoryPath other)
    {
        const int BothEqual = 0;
        const int ThisPreceedsOther = -1;
        const int ThisFollowsOther = 1;

        if (other is null)
        {
            return ThisFollowsOther;
        }

        if (this.Equals(other))
        {
            return BothEqual;
        }

        var thisComponents = this.Components;
        var otherComponents = other.Components;

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

        // The two directories aren't the same, but all the path components we've compared are the same.
        // => One of the directories is the parent of the other. Parents preceed children.
        return thisComponents.Length < otherComponents.Length ? ThisPreceedsOther : ThisFollowsOther;
    }

    /// <inheritdoc/>
    public bool Equals([AllowNull] AegisVirtualDirectoryPath other)
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
    public static bool operator ==(AegisVirtualDirectoryPath left, AegisVirtualDirectoryPath right) => left is null ? right is null : left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(AegisVirtualDirectoryPath left, AegisVirtualDirectoryPath right) => !(left == right);

    /// <inheritdoc/>
    public static bool operator <(AegisVirtualDirectoryPath left, AegisVirtualDirectoryPath right) => left is null ? right is not null : left.CompareTo(right) < 0;

    /// <inheritdoc/>
    public static bool operator <=(AegisVirtualDirectoryPath left, AegisVirtualDirectoryPath right) => left is null || left.CompareTo(right) <= 0;

    /// <inheritdoc/>
    public static bool operator >(AegisVirtualDirectoryPath left, AegisVirtualDirectoryPath right) => left is not null && left.CompareTo(right) > 0;

    /// <inheritdoc/>
    public static bool operator >=(AegisVirtualDirectoryPath left, AegisVirtualDirectoryPath right) => left is null ? right is null : left.CompareTo(right) >= 0;

    #endregion
}
