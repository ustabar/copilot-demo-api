using Contoso.CustomerApi.Models;

namespace Contoso.CustomerApi.Repositories;

public interface ICustomerRepository
{
    Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default);

    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default);

    Task<int> CountAsync(string? country = null, CancellationToken ct = default);

    Task AddAsync(Customer customer, CancellationToken ct = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
