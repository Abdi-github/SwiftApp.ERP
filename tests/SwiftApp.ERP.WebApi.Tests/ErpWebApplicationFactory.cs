using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.WebApi.Tests;

/// <summary>
/// Test factory that replaces PostgreSQL with an in-memory database
/// and disables Redis for fast API tests.
/// Configuration is loaded from appsettings.Testing.json.
/// </summary>
public class ErpWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestJwtSecret = "TEST-ONLY-super-secret-key-change-in-production-at-least-256-bits!!";
    private const string TestIssuer = "swiftapp-erp-api";
    private const string TestAudience = "swiftapp-erp-client";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove ALL AppDbContext and EF Core provider registrations
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<DbContextOptions>();

            // Build a dedicated internal EF service provider with ONLY InMemory services.
            // This completely bypasses any Npgsql services still in the main DI container.
            var efServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            // Replace with in-memory database using the dedicated EF provider.
            // Use a fixed name per factory instance so all scopes share the same DB.
            var dbName = "SwiftAppErpTest_" + Guid.NewGuid().ToString("N");
            services.AddDbContext<AppDbContext>((_, options) =>
                options.UseInMemoryDatabase(dbName)
                       .UseInternalServiceProvider(efServiceProvider));

            // Remove Redis cache and ICacheService registrations
            var cacheDescriptors = services.Where(
                d => d.ServiceType.FullName?.Contains("IDistributedCache") == true
                     || d.ServiceType.FullName?.Contains("ICacheService") == true).ToList();
            foreach (var d in cacheDescriptors) services.Remove(d);

            // Add simple in-memory distributed cache
            services.AddDistributedMemoryCache();

            // Remove Quartz hosted service to prevent background jobs from running during tests
            var quartzDescriptors = services.Where(d =>
                d.ServiceType.FullName?.Contains("Quartz") == true ||
                (d.ImplementationType?.FullName?.Contains("Quartz") ?? false)).ToList();
            foreach (var d in quartzDescriptors) services.Remove(d);
        });
    }

    /// <summary>
    /// Creates a JWT token with the specified role and permissions for integration testing.
    /// </summary>
    public string CreateTestToken(string username = "testadmin", string role = "ADMIN", params string[] permissions)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(JwtRegisteredClaimNames.Email, $"{username}@swiftapp.ch"),
            new(JwtRegisteredClaimNames.GivenName, "Test"),
            new(JwtRegisteredClaimNames.FamilyName, "User"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, role),
        };

        foreach (var permission in permissions)
            claims.Add(new Claim("permission", permission));

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Creates an HttpClient pre-configured with an ADMIN JWT token.
    /// </summary>
    public HttpClient CreateAuthenticatedClient(string role = "ADMIN", params string[] permissions)
    {
        var client = CreateClient();
        var token = CreateTestToken(role: role, permissions: permissions);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>
    /// Ensures the in-memory database is created and seeded.
    /// </summary>
    public async Task EnsureDatabaseCreatedAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
    }
}
