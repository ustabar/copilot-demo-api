using System.Collections.Concurrent;
using Contoso.CustomerApi.Models;

namespace Contoso.CustomerApi.Repositories;

/// <summary>
/// In-memory store. Seeded with a deterministic set so demo runs are reproducible
/// and so pagination boundaries land on numbers that are easy to reason about live.
/// </summary>
public sealed class InMemoryCustomerRepository : ICustomerRepository
{
    private readonly ConcurrentDictionary<Guid, Customer> _store = new();

    public InMemoryCustomerRepository()
    {
        foreach (var c in Seed())
        {
            _store[c.Id] = c;
        }
    }

    public Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<Customer> all = _store.Values
            .OrderBy(c => c.CreatedAt)
            .ThenBy(c => c.Id)
            .ToList();

        return Task.FromResult(all);
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_store.TryGetValue(id, out var c) ? c : null);

    public Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var match = _store.Values
            .FirstOrDefault(c => string.Equals(c.Email, email, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(match);
    }

    public Task<int> CountAsync(string? country = null, CancellationToken ct = default)
    {
        var count = country is null
            ? _store.Count
            : _store.Values.Count(c => string.Equals(c.Country, country, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(count);
    }

    public Task AddAsync(Customer customer, CancellationToken ct = default)
    {
        _store[customer.Id] = customer;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_store.TryRemove(id, out _));

    /// <summary>
    /// 23 customers. A prime-ish count on purpose: with a page size of 10 the last
    /// page is partial, which is exactly where off-by-one pagination bugs surface.
    /// </summary>
    public static IReadOnlyList<Customer> Seed()
    {
        var baseDate = new DateTimeOffset(2026, 1, 1, 9, 0, 0, TimeSpan.Zero);

        var rows = new (string Name, string Email, string Country, CustomerTier Tier)[]
        {
            ("Ayse Demir",       "ayse.demir@fabrikam.com",      "TR", CustomerTier.Enterprise),
            ("Bob Fernandez",    "bob.fernandez@contoso.com",    "US", CustomerTier.Standard),
            ("Chen Wei",         "chen.wei@northwind.com",       "SG", CustomerTier.Premium),
            ("Dilara Kaya",      "dilara.kaya@fabrikam.com",     "TR", CustomerTier.Standard),
            ("Erik Lindqvist",   "erik.lindqvist@contoso.com",   "SE", CustomerTier.Premium),
            ("Fatima Al-Sayed",  "fatima.alsayed@northwind.com", "AE", CustomerTier.Enterprise),
            ("Giulia Rossi",     "giulia.rossi@contoso.com",     "IT", CustomerTier.Standard),
            ("Hans Weber",       "hans.weber@fabrikam.com",      "DE", CustomerTier.Premium),
            ("Ines Duarte",      "ines.duarte@northwind.com",    "PT", CustomerTier.Standard),
            ("Jonas Berg",       "jonas.berg@contoso.com",       "NO", CustomerTier.Standard),
            ("Kemal Yildiz",     "kemal.yildiz@fabrikam.com",    "TR", CustomerTier.Premium),
            ("Laura Novak",      "laura.novak@northwind.com",    "CZ", CustomerTier.Standard),
            ("Mateo Silva",      "mateo.silva@contoso.com",      "BR", CustomerTier.Enterprise),
            ("Nadia Petrova",    "nadia.petrova@fabrikam.com",   "BG", CustomerTier.Standard),
            ("Omar Haddad",      "omar.haddad@northwind.com",    "MA", CustomerTier.Premium),
            ("Priya Nair",       "priya.nair@contoso.com",       "IN", CustomerTier.Standard),
            ("Quentin Moreau",   "quentin.moreau@fabrikam.com",  "FR", CustomerTier.Standard),
            ("Rosa Martinez",    "rosa.martinez@northwind.com",  "ES", CustomerTier.Premium),
            ("Selin Arslan",     "selin.arslan@fabrikam.com",    "TR", CustomerTier.Enterprise),
            ("Tomas Novotny",    "tomas.novotny@contoso.com",    "SK", CustomerTier.Standard),
            ("Ulrike Schmidt",   "ulrike.schmidt@northwind.com", "DE", CustomerTier.Standard),
            ("Viktor Ivanov",    "viktor.ivanov@contoso.com",    "UA", CustomerTier.Premium),
            ("Wanda Kowalski",   "wanda.kowalski@fabrikam.com",  "PL", CustomerTier.Standard)
        };

        return rows.Select((r, i) => new Customer
        {
            // Deterministic GUIDs so tests and demo scripts can reference a known id.
            Id = new Guid($"00000000-0000-0000-0000-{(i + 1):D12}"),
            Name = r.Name,
            Email = r.Email,
            Country = r.Country,
            Tier = r.Tier,
            CreatedAt = baseDate.AddDays(i),
            InternalNotes = r.Tier == CustomerTier.Enterprise
                ? "Credit limit reviewed manually. Do not expose."
                : null
        }).ToList();
    }
}
