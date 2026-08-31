using Contoso.CustomerApi.Models;
using Contoso.CustomerApi.Repositories;
using Contoso.CustomerApi.Validation;

namespace Contoso.CustomerApi.Services;

/// <summary>
/// Application logic for customers.
///
/// Business rules live here, not in the endpoint layer. The endpoint layer is
/// responsible only for binding, calling this service, and translating the result
/// into an HTTP response.
/// </summary>
public sealed class CustomerService(ICustomerRepository repository, ILogger<CustomerService> logger)
{
    private readonly ICustomerRepository _repository = repository;
    private readonly ILogger<CustomerService> _logger = logger;

    /// <summary>
    /// Returns one page of customers, optionally filtered by country.
    ///
    /// Pagination is 1-based: page 1 is the first page. The skip calculation therefore
    /// has to subtract one before multiplying, which is precisely the line that Demo 03
    /// breaks on purpose.
    /// </summary>
    public async Task<Result<PagedResult<Customer>>> GetCustomersAsync(
        int? page,
        int? pageSize,
        string? country = null,
        CancellationToken ct = default)
    {
        var (p, size) = PagingDefaults.Normalize(page, pageSize);

        var all = await _repository.GetAllAsync(ct);

        IEnumerable<Customer> query = all;
        if (!string.IsNullOrWhiteSpace(country))
        {
            query = query.Where(c => string.Equals(c.Country, country, StringComparison.OrdinalIgnoreCase));
        }

        var filtered = query.ToList();
        var skip = (p - 1) * size;

        var items = filtered
            .Skip(skip)
            .Take(size)
            .ToList();

        var result = new PagedResult<Customer>
        {
            Items = items,
            Page = p,
            PageSize = size,
            TotalCount = filtered.Count
        };

        return Result<PagedResult<Customer>>.Success(result);
    }

    public async Task<Result<Customer>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await _repository.GetByIdAsync(id, ct);

        return customer is null
            ? Result<Customer>.Failure(ErrorCodes.NotFound, $"Customer {id} was not found.")
            : Result<Customer>.Success(customer);
    }

    public async Task<Result<Customer>> CreateAsync(CustomerRequest request, CancellationToken ct = default)
    {
        var errors = CustomerValidator.Validate(request);
        if (errors.Count > 0)
        {
            var detail = string.Join(" ", errors.Select(e => $"{e.Field}: {e.Message}"));
            return Result<Customer>.Failure(ErrorCodes.ValidationFailed, detail);
        }

        var existing = await _repository.GetByEmailAsync(request.Email, ct);
        if (existing is not null)
        {
            return Result<Customer>.Failure(
                ErrorCodes.DuplicateEmail,
                $"A customer with email {request.Email} already exists.");
        }

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Country = request.Country.Trim().ToUpperInvariant(),
            Tier = request.Tier,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _repository.AddAsync(customer, ct);
        _logger.LogInformation("Created customer {CustomerId}", customer.Id);

        return Result<Customer>.Success(customer);
    }

    /// <summary>
    /// Deletes a customer.
    ///
    /// Deleting an Enterprise customer is a privileged operation: those records carry
    /// contractual obligations and are removed only by an operator with the
    /// customers.admin scope. Demo 05 plants a change that quietly drops this check.
    /// </summary>
    public async Task<Result<bool>> DeleteAsync(Guid id, bool callerIsAdmin, CancellationToken ct = default)
    {
        var customer = await _repository.GetByIdAsync(id, ct);
        if (customer is null)
        {
            return Result<bool>.Failure(ErrorCodes.NotFound, $"Customer {id} was not found.");
        }

        if (customer.Tier == CustomerTier.Enterprise && !callerIsAdmin)
        {
            _logger.LogWarning("Blocked non-admin delete of enterprise customer {CustomerId}", id);
            return Result<bool>.Failure(
                ErrorCodes.Forbidden,
                "Deleting an enterprise customer requires the customers.admin scope.");
        }

        var deleted = await _repository.DeleteAsync(id, ct);
        return Result<bool>.Success(deleted);
    }

    // ---------------------------------------------------------------------
    // DEMO 01 SCRATCH AREA
    //
    // During the baseline demo, type a NEW method signature below and stop, so
    // inline completion has to produce the body from the surrounding file. A good
    // one to use, because it is plausible but not implemented anywhere:
    //
    //     public async Task<IReadOnlyList<Customer>> GetTopCustomersByTierAsync(
    //         CustomerTier tier, int take, CancellationToken ct = default)
    //
    // Delete whatever it generates before moving on to the next demo.
    // ---------------------------------------------------------------------
}
