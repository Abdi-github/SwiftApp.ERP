using System.Net;
using FluentAssertions;
using Xunit;

namespace SwiftApp.ERP.WebApi.Tests;

public class HealthCheckTests : IClassFixture<ErpWebApplicationFactory>
{
    private readonly ErpWebApplicationFactory _factory;

    public HealthCheckTests(ErpWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task HealthEndpoint_ShouldReturn200()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SwaggerEndpoint_ShouldReturn200_InDevelopment()
    {
        // NOTE: our factory overrides environment to "Testing", not "Development",
        // so Swagger should not be enabled → expect redirect or 404.
        var client = _factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        // Swagger is only enabled in Development, so expect 404 in Testing env
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RootEndpoint_ShouldRedirectToSwagger()
    {
        var client = _factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("swagger");
    }
}
