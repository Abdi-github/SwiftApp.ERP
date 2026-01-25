using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Inventory.Domain.Entities;
using SwiftApp.ERP.Modules.Inventory.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.WebApi.Infrastructure.SeedData;

public static class InventorySeedData
{
    public static readonly Guid WarehouseMainId = new("e0000000-0000-0000-0000-000000000001");
    public static readonly Guid WarehouseComponentsId = new("e0000000-0000-0000-0000-000000000002");
    public static readonly Guid WarehouseShippingId = new("e0000000-0000-0000-0000-000000000003");

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        if (await db.Set<Warehouse>().AnyAsync())
        {
            logger.LogInformation("Inventory seed data already exists, skipping");
            return;
        }

        logger.LogInformation("Seeding Inventory module...");

        // ── Warehouses ──
        var warehouses = new List<Warehouse>
        {
            new() { Id = WarehouseMainId, Code = "WH-MAIN", Name = "Hauptlager Zürich", Description = "Zentrales Fertigwarenlager", Address = "Bahnhofstrasse 42, 8001 Zürich", Active = true },
            new() { Id = WarehouseComponentsId, Code = "WH-COMP", Name = "Komponentenlager", Description = "Lager für Uhrwerk-Komponenten und Rohmaterialien", Address = "Industriestrasse 15, 2502 Biel/Bienne", Active = true },
            new() { Id = WarehouseShippingId, Code = "WH-SHIP", Name = "Versandlager", Description = "Versandfertige Ware und Verpackungsmaterial", Address = "Bahnhofstrasse 42, 8001 Zürich", Active = true },
        };
        await db.Set<Warehouse>().AddRangeAsync(warehouses);
        await db.SaveChangesAsync();

        // ── Stock Levels (finished products in main warehouse) ──
        var stockLevels = new List<StockLevel>
        {
            // Products in main warehouse
            new() { ItemId = MasterDataSeedData.ProdAlpineChronoId, ItemType = StockItemType.Product, WarehouseId = WarehouseMainId, QuantityOnHand = 25, QuantityReserved = 3 },
            new() { ItemId = MasterDataSeedData.ProdGenevaClassicId, ItemType = StockItemType.Product, WarehouseId = WarehouseMainId, QuantityOnHand = 18, QuantityReserved = 2 },
            new() { ItemId = MasterDataSeedData.ProdMatterhornDiverId, ItemType = StockItemType.Product, WarehouseId = WarehouseMainId, QuantityOnHand = 12, QuantityReserved = 1 },
            new() { ItemId = MasterDataSeedData.ProdBernMinimalId, ItemType = StockItemType.Product, WarehouseId = WarehouseMainId, QuantityOnHand = 40, QuantityReserved = 5 },
            new() { ItemId = MasterDataSeedData.ProdLuzernTourbillonId, ItemType = StockItemType.Product, WarehouseId = WarehouseMainId, QuantityOnHand = 3, QuantityReserved = 1 },

            // Materials in components warehouse
            new() { ItemId = MasterDataSeedData.MatEta2824Id, ItemType = StockItemType.Material, WarehouseId = WarehouseComponentsId, QuantityOnHand = 120, QuantityReserved = 15 },
            new() { ItemId = MasterDataSeedData.MatSapphireCrystalId, ItemType = StockItemType.Material, WarehouseId = WarehouseComponentsId, QuantityOnHand = 200, QuantityReserved = 20 },
            new() { ItemId = MasterDataSeedData.MatSs316lId, ItemType = StockItemType.Material, WarehouseId = WarehouseComponentsId, QuantityOnHand = 85, QuantityReserved = 10 },
            new() { ItemId = MasterDataSeedData.MatLeatherStrapId, ItemType = StockItemType.Material, WarehouseId = WarehouseComponentsId, QuantityOnHand = 150, QuantityReserved = 8 },
            new() { ItemId = MasterDataSeedData.MatGold18kId, ItemType = StockItemType.Material, WarehouseId = WarehouseComponentsId, QuantityOnHand = 850, QuantityReserved = 42 },
            new() { ItemId = MasterDataSeedData.MatTitaniumCaseId, ItemType = StockItemType.Material, WarehouseId = WarehouseComponentsId, QuantityOnHand = 45, QuantityReserved = 5 },
            new() { ItemId = MasterDataSeedData.MatRubberStrapId, ItemType = StockItemType.Material, WarehouseId = WarehouseComponentsId, QuantityOnHand = 180, QuantityReserved = 12 },
            new() { ItemId = MasterDataSeedData.MatDialBlackId, ItemType = StockItemType.Material, WarehouseId = WarehouseComponentsId, QuantityOnHand = 95, QuantityReserved = 8 },
            new() { ItemId = MasterDataSeedData.MatDialWhiteId, ItemType = StockItemType.Material, WarehouseId = WarehouseComponentsId, QuantityOnHand = 70, QuantityReserved = 5 },
            new() { ItemId = MasterDataSeedData.MatHandsRoseGoldId, ItemType = StockItemType.Material, WarehouseId = WarehouseComponentsId, QuantityOnHand = 130, QuantityReserved = 10 },
        };
        await db.Set<StockLevel>().AddRangeAsync(stockLevels);
        await db.SaveChangesAsync();

        // ── Initial Stock Movements (goods receipts) ──
        var movements = new List<StockMovement>
        {
            new() { ReferenceNumber = "SM-2026-00001", MovementType = MovementType.GoodsReceipt, ItemId = MasterDataSeedData.ProdAlpineChronoId, ItemType = StockItemType.Product, TargetWarehouseId = WarehouseMainId, Quantity = 25, MovementDate = DateTimeOffset.UtcNow.AddDays(-30), Reason = "Erstbestand / Initial stock" },
            new() { ReferenceNumber = "SM-2026-00002", MovementType = MovementType.GoodsReceipt, ItemId = MasterDataSeedData.MatEta2824Id, ItemType = StockItemType.Material, TargetWarehouseId = WarehouseComponentsId, Quantity = 120, MovementDate = DateTimeOffset.UtcNow.AddDays(-30), Reason = "Erstbestand / Initial stock" },
        };
        await db.Set<StockMovement>().AddRangeAsync(movements);
        await db.SaveChangesAsync();

        logger.LogInformation("Inventory seeded: {WhCount} warehouses, {SlCount} stock levels, {SmCount} movements",
            warehouses.Count, stockLevels.Count, movements.Count);
    }
}
