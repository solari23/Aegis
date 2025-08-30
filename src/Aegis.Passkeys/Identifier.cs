namespace Aegis.Passkeys;

/// <summary>
/// Represents an identifier, that is underlyingly a non-empty array of bytes.
/// </summary>
public class Identifier
{
    /// <summary>
    /// Creates a new instance of the <see cref="Identifier"/>.
    /// </summary>
    public Identifier(ReadOnlySpan<byte> value)
    {
        if (value.Length == 0)
        {
            throw new ArgumentException("Identifier value cannot be empty.", nameof(value));
        }

        this.internalValue = new byte[value.Length];
        value.CopyTo(this.internalValue);
    }

    internal readonly byte[] internalValue;

    /// <summary>
    /// Gets the value of the identifier.
    /// </summary>
    public ReadOnlySpan<byte> Value => this.internalValue;
}
