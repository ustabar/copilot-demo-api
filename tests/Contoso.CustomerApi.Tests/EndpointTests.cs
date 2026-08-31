using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Contoso.CustomerApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Contoso.CustomerApi.Tests;

/// <summary>
/// Endpoint tests that drive the real host.
///
/// These are the tests that catch the Demo 02 trap: if the error channel is refactored
/// but the status-code mapping is not updated with it, the service still returns a
/// failure, the build is still green, and these assertions turn red because everything
/// has collapsed to 500.
/// </summary>
public class EndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory = factory;

    private static readonly Guid KnownEnterpriseId = new("00000000-0000-0000-0000-000000000001");

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomers_ReturnsPagedPayloadWithMetadata()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/customers?page=1&pageSize=10");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;

        Assert.Equal(10, root.GetProperty("items").GetArrayLength());
        Assert.Equal(1, root.GetProperty("page").GetInt32());
        Assert.Equal(23, root.GetProperty("totalCount").GetInt32());
        Assert.Equal(3, root.GetProperty("totalPages").GetInt32());
        Assert.True(root.GetProperty("hasNextPage").GetBoolean());
        Assert.False(root.GetProperty("hasPreviousPage").GetBoolean());
    }

    [Fact]
    public async Task GetCustomers_NeverLeaksInternalNotes()
    {
        var client = _factory.CreateClient();

        var body = await client.GetStringAsync("/api/customers?page=1&pageSize=100");

        Assert.DoesNotContain("internalNotes", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Credit limit reviewed", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetCustomerById_Returns404_ForUnknownId()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/customers/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_Returns400_ForInvalidPayload()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/customers", new CustomerRequest
        {
            Name = "",
            Email = "nope",
            Country = "TUR"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_Returns409_ForDuplicateEmail()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/customers", new CustomerRequest
        {
            Name = "Duplicate",
            Email = "ayse.demir@fabrikam.com",
            Country = "TR"
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEnterpriseCustomer_Returns403_WithoutAdminScope()
    {
        var client = _factory.CreateClient();

        var response = await client.DeleteAsync($"/api/customers/{KnownEnterpriseId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
