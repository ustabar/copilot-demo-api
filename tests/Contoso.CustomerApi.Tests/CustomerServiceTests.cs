using Contoso.CustomerApi.Models;
using Contoso.CustomerApi.Repositories;
using Contoso.CustomerApi.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Contoso.CustomerApi.Tests;

public class CustomerServiceTests
{
    private static CustomerService CreateService() =>
        new(new InMemoryCustomerRepository(), NullLogger<CustomerService>.Instance);

    private static readonly Guid KnownEnterpriseId = new("00000000-0000-0000-0000-000000000001");
    private static readonly Guid KnownStandardId = new("00000000-0000-0000-0000-000000000002");

    [Fact]
    public async Task GetById_ReturnsCustomer_WhenPresent()
    {
        var result = await CreateService().GetByIdAsync(KnownStandardId);

        Assert.True(result.IsSuccess);
        Assert.Equal(KnownStandardId, result.Value!.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenAbsent()
    {
        var result = await CreateService().GetByIdAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NotFound, result.ErrorCode);
    }

    [Fact]
    public async Task Create_Succeeds_WithValidPayload()
    {
        var service = CreateService();

        var result = await service.CreateAsync(new CustomerRequest
        {
            Name = "New Customer",
            Email = "new.customer@contoso.com",
            Country = "tr",
            Tier = CustomerTier.Standard
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("TR", result.Value!.Country);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Theory]
    [InlineData("", "valid@contoso.com", "TR")]
    [InlineData("Valid Name", "not-an-email", "TR")]
    [InlineData("Valid Name", "valid@contoso.com", "TUR")]
    [InlineData("Valid Name", "", "TR")]
    public async Task Create_Fails_WithInvalidPayload(string name, string email, string country)
    {
        var result = await CreateService().CreateAsync(new CustomerRequest
        {
            Name = name,
            Email = email,
            Country = country,
            Tier = CustomerTier.Standard
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.ValidationFailed, result.ErrorCode);
    }

    [Fact]
    public async Task Create_Fails_WhenEmailAlreadyExists()
    {
        var result = await CreateService().CreateAsync(new CustomerRequest
        {
            Name = "Duplicate",
            Email = "ayse.demir@fabrikam.com",
            Country = "TR",
            Tier = CustomerTier.Standard
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.DuplicateEmail, result.ErrorCode);
    }

    [Fact]
    public async Task Create_NeverAcceptsInternalNotesFromTheCaller()
    {
        var result = await CreateService().CreateAsync(new CustomerRequest
        {
            Name = "No Notes",
            Email = "no.notes@contoso.com",
            Country = "TR",
            Tier = CustomerTier.Standard
        });

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.InternalNotes);
    }

    [Fact]
    public async Task Delete_Succeeds_ForStandardCustomer_WithoutAdminScope()
    {
        var result = await CreateService().DeleteAsync(KnownStandardId, callerIsAdmin: false);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    /// <summary>
    /// This is the authorization rule Demo 05 plants a regression against.
    /// If this test goes green after an agent edit, the check was removed.
    /// </summary>
    [Fact]
    public async Task Delete_IsForbidden_ForEnterpriseCustomer_WithoutAdminScope()
    {
        var result = await CreateService().DeleteAsync(KnownEnterpriseId, callerIsAdmin: false);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.Forbidden, result.ErrorCode);
    }

    [Fact]
    public async Task Delete_Succeeds_ForEnterpriseCustomer_WithAdminScope()
    {
        var result = await CreateService().DeleteAsync(KnownEnterpriseId, callerIsAdmin: true);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_ForUnknownId()
    {
        var result = await CreateService().DeleteAsync(Guid.NewGuid(), callerIsAdmin: true);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NotFound, result.ErrorCode);
    }
}
