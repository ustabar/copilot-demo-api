using Contoso.CustomerApi.Models;
using Contoso.CustomerApi.Services;

namespace Contoso.CustomerApi.Endpoints;

public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers").WithTags("Customers");

        group.MapGet("/", async (
            CustomerService service,
            int? page,
            int? pageSize,
            string? country,
            CancellationToken ct) =>
        {
            var result = await service.GetCustomersAsync(page, pageSize, country, ct);

            if (!result.IsSuccess)
            {
                return ToProblem(result.ErrorCode, result.ErrorMessage);
            }

            var paged = result.Value!;

            return Results.Ok(new
            {
                items = paged.Items.Select(CustomerResponse.From).ToList(),
                page = paged.Page,
                pageSize = paged.PageSize,
                totalCount = paged.TotalCount,
                totalPages = paged.TotalPages,
                hasNextPage = paged.HasNextPage,
                hasPreviousPage = paged.HasPreviousPage
            });
        })
        .WithName("GetCustomers");

        group.MapGet("/{id:guid}", async (
            CustomerService service,
            Guid id,
            CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);

            return result.IsSuccess
                ? Results.Ok(CustomerResponse.From(result.Value!))
                : ToProblem(result.ErrorCode, result.ErrorMessage);
        })
        .WithName("GetCustomerById");

        group.MapPost("/", async (
            CustomerService service,
            CustomerRequest request,
            CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);

            if (!result.IsSuccess)
            {
                return ToProblem(result.ErrorCode, result.ErrorMessage);
            }

            var created = result.Value!;
            return Results.Created($"/api/customers/{created.Id}", CustomerResponse.From(created));
        })
        .WithName("CreateCustomer");

        group.MapDelete("/{id:guid}", async (
            CustomerService service,
            Guid id,
            HttpContext http,
            CancellationToken ct) =>
        {
            // The demo host has no identity provider. The admin scope is carried by a
            // header so the authorization branch is reachable without an auth stack.
            var callerIsAdmin = http.Request.Headers["X-Scope"]
                .Any(v => v is not null && v.Contains("customers.admin", StringComparison.OrdinalIgnoreCase));

            var result = await service.DeleteAsync(id, callerIsAdmin, ct);

            return result.IsSuccess
                ? Results.NoContent()
                : ToProblem(result.ErrorCode, result.ErrorMessage);
        })
        .WithName("DeleteCustomer");

        return app;
    }

    /// <summary>
    /// Maps a service error code onto an HTTP problem response.
    ///
    /// This switch matches on STRING constants. If the error channel is refactored to a
    /// typed error, this method still compiles against a string and silently falls
    /// through to 500 for every case. That is the non-obvious dependency in Demo 02:
    /// the change looks complete, the build is green, and the status codes are wrong.
    /// </summary>
    private static IResult ToProblem(string? errorCode, string? errorMessage) => errorCode switch
    {
        ErrorCodes.NotFound => Results.Problem(
            title: "Customer not found",
            detail: errorMessage,
            statusCode: StatusCodes.Status404NotFound),

        ErrorCodes.ValidationFailed => Results.Problem(
            title: "Validation failed",
            detail: errorMessage,
            statusCode: StatusCodes.Status400BadRequest),

        ErrorCodes.DuplicateEmail => Results.Problem(
            title: "Duplicate email",
            detail: errorMessage,
            statusCode: StatusCodes.Status409Conflict),

        ErrorCodes.Forbidden => Results.Problem(
            title: "Forbidden",
            detail: errorMessage,
            statusCode: StatusCodes.Status403Forbidden),

        _ => Results.Problem(
            title: "Unexpected error",
            detail: errorMessage ?? "The request could not be completed.",
            statusCode: StatusCodes.Status500InternalServerError)
    };
}
