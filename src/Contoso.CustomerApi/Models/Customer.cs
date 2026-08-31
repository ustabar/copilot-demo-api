namespace Contoso.CustomerApi.Models;

/// <summary>
/// A customer record. Kept deliberately small so a demo audience can hold it in their head.
/// </summary>
public sealed record Customer
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string Country { get; init; }
    public required CustomerTier Tier { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Internal credit note. Must never be returned on a public endpoint.
    /// </summary>
    public string? InternalNotes { get; init; }
}

public enum CustomerTier
{
    Standard = 0,
    Premium = 1,
    Enterprise = 2
}
