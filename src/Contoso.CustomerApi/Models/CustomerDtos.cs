namespace Contoso.CustomerApi.Models;

/// <summary>
/// Inbound payload for creating or updating a customer.
/// Deliberately separate from <see cref="Customer"/> so the API surface and the
/// domain model can drift independently - and so InternalNotes cannot be set by a caller.
/// </summary>
public sealed record CustomerRequest
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public CustomerTier Tier { get; init; } = CustomerTier.Standard;
}

/// <summary>
/// Outbound representation. Note that InternalNotes is absent by design.
/// </summary>
public sealed record CustomerResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string Country { get; init; }
    public required string Tier { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }

    public static CustomerResponse From(Customer c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Email = c.Email,
        Country = c.Country,
        Tier = c.Tier.ToString(),
        CreatedAt = c.CreatedAt
    };
}
