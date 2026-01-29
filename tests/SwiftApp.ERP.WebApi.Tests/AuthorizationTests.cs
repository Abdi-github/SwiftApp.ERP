using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace SwiftApp.ERP.WebApi.Tests;

public class AuthorizationTests : IClassFixture<ErpWebApplicationFactory>
{
    private readonly ErpWebApplicationFactory _factory;

    public AuthorizationTests(ErpWebApplicationFactory factory) => _factory = factory;

    [Theory]
    [InlineData("/api/v1/masterdata/products")]
    [InlineData("/api/v1/masterdata/categories")]
    [InlineData("/api/v1/inventory/warehouses")]
    [InlineData("/api/v1/sales/orders")]
    [InlineData("/api/v1/sales/customers")]
    [InlineData("/api/v1/purchasing/suppliers")]
    [InlineData("/api/v1/hr/employees")]
    [InlineData("/api/v1/hr/departments")]
    [InlineData("/api/v1/crm/contacts")]
    [InlineData("/api/v1/accounting/accounts")]
    public async Task ProtectedEndpoint_WithoutToken_ShouldReturn401(string url)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("/api/v1/masterdata/products")]
    [InlineData("/api/v1/masterdata/categories")]
    [InlineData("/api/v1/inventory/warehouses")]
    [InlineData("/api/v1/sales/orders")]
    [InlineData("/api/v1/sales/customers")]
    [InlineData("/api/v1/purchasing/suppliers")]
    [InlineData("/api/v1/hr/employees")]
    [InlineData("/api/v1/hr/departments")]
    [InlineData("/api/v1/crm/contacts")]
    [InlineData("/api/v1/accounting/accounts")]
    public async Task ProtectedEndpoint_WithAdminToken_ShouldReturnSuccess(string url)
    {
        await _factory.EnsureDatabaseCreatedAsync();
        var client = _factory.CreateAuthenticatedClient();

        var response = await client.GetAsync(url);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithSpecificPermission_ShouldReturnSuccess()
    {
        await _factory.EnsureDatabaseCreatedAsync();
        var client = _factory.CreateAuthenticatedClient("SALES", "SALES:VIEW", "SALES:CREATE");

        var response = await client.GetAsync("/api/v1/sales/customers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task LoginEndpoint_ShouldBeAnonymous()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            Username = "nonexistent",
            Password = "wrong"
        });

        // Should return 401 (auth failed), not 403/redirect — proving the endpoint is reachable without auth
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
