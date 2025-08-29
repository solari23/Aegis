namespace Aegis.Passkeys;

public class UserEntityInfo
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public required Identifier Id { get; init; }

    /// <summary>
    /// An identifying name for the user (e.g. 'john.doe@example.com').
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// A friendly name to diplay for the user (e.g. 'John Doe' - optional).
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// A URL to an icon image for the user (optional).
    /// </summary>
    public string? IconUrl { get; init; }
}
