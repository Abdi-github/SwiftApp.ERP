using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Auth.Infrastructure.Persistence.SeedData;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.WebApi.Infrastructure.SeedData;

/// <summary>
/// Orchestrates all module seeders in dependency order.
/// Called from Program.cs during development startup.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAllAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        logger.LogInformation("Starting database seeding...");

        // 1. Auth (roles, permissions, admin user) — no dependencies
        await AuthSeedData.SeedAsync(serviceProvider);

        // 2. MasterData (categories, UoMs, products, materials, BOMs) — no dependencies
        await MasterDataSeedData.SeedAsync(serviceProvider);

        // 3. Inventory (warehouses, stock levels) — depends on MasterData
        await InventorySeedData.SeedAsync(serviceProvider);

        // 4. Sales (customers, orders) — depends on MasterData
        await SalesSeedData.SeedAsync(serviceProvider);

        // 5. Purchasing (suppliers, orders) — depends on MasterData
        await PurchasingSeedData.SeedAsync(serviceProvider);

        // 6. Production (work centers, orders) — depends on MasterData
        await ProductionSeedData.SeedAsync(serviceProvider);

        // 7. Accounting (chart of accounts, journal entries)
        await AccountingSeedData.SeedAsync(serviceProvider);

        // 8. HR (departments, employees)
        await HrSeedData.SeedAsync(serviceProvider);

        // 9. CRM (contacts, interactions) — depends on Sales (customers)
        await CrmSeedData.SeedAsync(serviceProvider);

        // 10. QualityControl (inspection plans, checks) — depends on MasterData
        await QualityControlSeedData.SeedAsync(serviceProvider);

        // 11. Notification (templates for all event types)
        await NotificationSeedData.SeedAsync(serviceProvider);

        logger.LogInformation("Database seeding completed successfully");
    }
}
