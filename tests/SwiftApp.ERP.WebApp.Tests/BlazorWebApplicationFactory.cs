using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.WebApp.Tests;

/// <summary>
/// Test factory for the Blazor SSR WebApp.
/// Replaces PostgreSQL with InMemory and disables Redis.
/// </summary>
public class BlazorWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove ALL AppDbContext and EF Core provider registrations
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<DbContextOptions>();

            // Build a dedicated EF internal service provider with only InMemory services
            var efServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            // Replace with in-memory database
            var dbName = "SwiftAppErpWebAppTest_" + Guid.NewGuid().ToString("N");
            services.AddDbContext<AppDbContext>((_, options) =>
                options.UseInMemoryDatabase(dbName)
                       .UseInternalServiceProvider(efServiceProvider));

            // Remove Redis cache and ICacheService registrations
            var cacheDescriptors = services.Where(
                d => d.ServiceType.FullName?.Contains("IDistributedCache") == true
                     || d.ServiceType.FullName?.Contains("ICacheService") == true).ToList();
            foreach (var d in cacheDescriptors) services.Remove(d);

            services.AddDistributedMemoryCache();
        });
    }
}
