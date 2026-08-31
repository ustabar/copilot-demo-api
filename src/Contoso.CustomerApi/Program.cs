using Contoso.CustomerApi.Endpoints;
using Contoso.CustomerApi.Repositories;
using Contoso.CustomerApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ICustomerRepository, InMemoryCustomerRepository>();
builder.Services.AddScoped<CustomerService>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
   .WithName("Health");

app.MapCustomerEndpoints();

app.Run();

/// <summary>
/// Exposed so the integration tests can drive the real host through WebApplicationFactory.
/// </summary>
public partial class Program;
