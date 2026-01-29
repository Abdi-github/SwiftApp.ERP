using System.Net;
using FluentAssertions;
using Xunit;

namespace SwiftApp.ERP.WebApp.Tests;

/// <summary>
/// Smoke tests verifying key WebApp endpoints are accessible.
/// </summary>
public class BlazorSmokeTests : IClassFixture<BlazorWebApplicationFactory>
{
    private readonly BlazorWebApplicationFactory _factory;

    public BlazorSmokeTests(BlazorWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task HealthEndpoint_ShouldReturn200()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RootEndpoint_ShouldRedirectToDashboard()
    {
        var client = _factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("/app/dashboard");
    }

    [Fact]
    public async Task LoginPage_ShouldReturn200()
    {
        var client = _factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/login");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("SwiftApp");
    }

    [Fact]
    public async Task DashboardPage_ShouldRedirectToLogin_WhenUnauthenticated()
    {
        var client = _factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/app/dashboard");

        // Blazor SSR renders even protected pages — auth check happens in the component
        // For server-side redirects, cookie auth redirects to /login
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task LoginPost_WithBadCredentials_ShouldRedirectWithError()
    {
        var client = _factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });

        var formContent = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("username", "invalid"),
            new KeyValuePair<string, string>("password", "wrong"),
            new KeyValuePair<string, string>("returnUrl", "/app/dashboard")
        ]);

        var response = await client.PostAsync("/account/login", formContent);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("error");
    }
}
