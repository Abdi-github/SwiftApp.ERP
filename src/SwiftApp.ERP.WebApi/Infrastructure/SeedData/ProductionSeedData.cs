using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Production.Domain.Entities;
using SwiftApp.ERP.Modules.Production.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.WebApi.Infrastructure.SeedData;

public static class ProductionSeedData
{
    public static readonly Guid WcAssemblyId = new("f3000000-0000-0000-0000-000000000001");
    public static readonly Guid WcPolishingId = new("f3000000-0000-0000-0000-000000000002");
    public static readonly Guid WcQcStationId = new("f3000000-0000-0000-0000-000000000003");
    public static readonly Guid WcEngravingId = new("f3000000-0000-0000-0000-000000000004");

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        if (await db.Set<WorkCenter>().AnyAsync())
        {
            logger.LogInformation("Production seed data already exists, skipping");
            return;
        }

        logger.LogInformation("Seeding Production module...");

        // ── Work Centers ──
        var workCenters = new List<WorkCenter>
        {
            new() { Id = WcAssemblyId, Code = "WC-ASS", Name = "Montage / Assembly", Description = "Endmontage der Uhren — Werk einschalen, Zeiger setzen, Gehäuse verschliessen", CapacityPerDay = 20, CostPerHour = 95.00m, Active = true },
            new() { Id = WcPolishingId, Code = "WC-POL", Name = "Polissage / Polishing", Description = "Gehäuse- und Bandsatinierung, Hochglanzpolitur", CapacityPerDay = 30, CostPerHour = 75.00m, Active = true },
            new() { Id = WcQcStationId, Code = "WC-QC", Name = "Qualitätskontrolle", Description = "Gangkontrolle, Wasserdichtigkeit, optische Prüfung", CapacityPerDay = 40, CostPerHour = 85.00m, Active = true },
            new() { Id = WcEngravingId, Code = "WC-ENG", Name = "Gravur / Engraving", Description = "Lasergravur von Seriennummer und Bodendeckel", CapacityPerDay = 50, CostPerHour = 60.00m, Active = true },
        };
        await db.Set<WorkCenter>().AddRangeAsync(workCenters);
        await db.SaveChangesAsync();

        // ── Production Orders ──
        var prodOrder1 = new ProductionOrder
        {
            OrderNumber = "PRD-2026-00001",
            ProductId = MasterDataSeedData.ProdAlpineChronoId,
            WorkCenterId = WcAssemblyId,
            Status = ProductionOrderStatus.Released,
            PlannedQuantity = 20,
            PlannedStartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PlannedEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            EstimatedCost = 8500.00m,
            Notes = "Batch 2026-Q2 Alpine Chronograph",
            Lines =
            [
                new() { MaterialId = MasterDataSeedData.MatEta2824Id, Description = "ETA 2824-2 Automatikwerk", RequiredQuantity = 20, Position = 1 },
                new() { MaterialId = MasterDataSeedData.MatSs316lId, Description = "Edelstahlgehäuse 316L 42mm", RequiredQuantity = 20, Position = 2 },
                new() { MaterialId = MasterDataSeedData.MatSapphireCrystalId, Description = "Saphirglas 42mm", RequiredQuantity = 20, Position = 3 },
                new() { MaterialId = MasterDataSeedData.MatDialBlackId, Description = "Zifferblatt Schwarz 42mm", RequiredQuantity = 20, Position = 4 },
                new() { MaterialId = MasterDataSeedData.MatRubberStrapId, Description = "Kautschuk-Armband Schwarz", RequiredQuantity = 20, Position = 5 },
            ],
        };

        var prodOrder2 = new ProductionOrder
        {
            OrderNumber = "PRD-2026-00002",
            ProductId = MasterDataSeedData.ProdGenevaClassicId,
            WorkCenterId = WcAssemblyId,
            Status = ProductionOrderStatus.Draft,
            PlannedQuantity = 10,
            PlannedStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            PlannedEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            EstimatedCost = 12000.00m,
            Notes = "Geneva Classic Roségold-Edition",
            Lines =
            [
                new() { MaterialId = MasterDataSeedData.MatEta2824Id, Description = "ETA 2824-2 Automatikwerk", RequiredQuantity = 10, Position = 1 },
                new() { MaterialId = MasterDataSeedData.MatGold18kId, Description = "18K Roségold", RequiredQuantity = 425, Position = 2 },
                new() { MaterialId = MasterDataSeedData.MatSapphireCrystalId, Description = "Saphirglas 42mm", RequiredQuantity = 10, Position = 3 },
                new() { MaterialId = MasterDataSeedData.MatDialWhiteId, Description = "Zifferblatt Weiss 40mm", RequiredQuantity = 10, Position = 4 },
                new() { MaterialId = MasterDataSeedData.MatHandsRoseGoldId, Description = "Zeigersatz Roségold", RequiredQuantity = 10, Position = 5 },
                new() { MaterialId = MasterDataSeedData.MatLeatherStrapId, Description = "Kalbsleder-Armband Braun", RequiredQuantity = 10, Position = 6 },
            ],
        };

        await db.Set<ProductionOrder>().AddRangeAsync([prodOrder1, prodOrder2]);
        await db.SaveChangesAsync();

        logger.LogInformation("Production seeded: {WcCount} work centers, 2 production orders", workCenters.Count);
    }
}
