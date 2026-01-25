using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Purchasing.Domain.Entities;
using SwiftApp.ERP.Modules.Purchasing.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.WebApi.Infrastructure.SeedData;

public static class PurchasingSeedData
{
    public static readonly Guid SuppEtaId = new("f2000000-0000-0000-0000-000000000001");
    public static readonly Guid SuppSwissCrystalId = new("f2000000-0000-0000-0000-000000000002");
    public static readonly Guid SuppHauteLeatherId = new("f2000000-0000-0000-0000-000000000003");

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        if (await db.Set<Supplier>().AnyAsync())
        {
            logger.LogInformation("Purchasing seed data already exists, skipping");
            return;
        }

        logger.LogInformation("Seeding Purchasing module...");

        // ── Suppliers ──
        var suppliers = new List<Supplier>
        {
            new()
            {
                Id = SuppEtaId, SupplierNumber = "S-2026-001",
                CompanyName = "ETA SA Manufacture Horlogère Suisse", Email = "sales@eta.ch", Phone = "+41 32 344 44 44",
                Street = "Schild-Rust-Strasse 17", City = "Grenchen", PostalCode = "2540", Canton = "SO", Country = "CH",
                VatNumber = "CHE-100.200.300", PaymentTerms = 60, ContactPerson = "Marc Dubois",
                Website = "https://www.eta.ch", Active = true,
            },
            new()
            {
                Id = SuppSwissCrystalId, SupplierNumber = "S-2026-002",
                CompanyName = "Swiss Crystal Components AG", Email = "info@swisscrystal.ch", Phone = "+41 32 721 10 00",
                Street = "Route des Acacias 14", City = "Le Locle", PostalCode = "2400", Canton = "NE", Country = "CH",
                PaymentTerms = 30, ContactPerson = "Claire Favre", Active = true,
            },
            new()
            {
                Id = SuppHauteLeatherId, SupplierNumber = "S-2026-003",
                CompanyName = "Haute Leather Sarl", Email = "commandes@hauteleather.ch", Phone = "+41 21 634 80 00",
                Street = "Chemin de la Vuachère 20", City = "Lausanne", PostalCode = "1005", Canton = "VD", Country = "CH",
                PaymentTerms = 30, ContactPerson = "Sophie Martin", Active = true,
            },
        };
        await db.Set<Supplier>().AddRangeAsync(suppliers);
        await db.SaveChangesAsync();

        // ── Purchase Orders ──
        var po1 = new PurchaseOrder
        {
            OrderNumber = "PO-2026-00001",
            SupplierId = SuppEtaId,
            Status = PurchaseOrderStatus.Confirmed,
            OrderDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-14)),
            ExpectedDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Currency = "CHF",
            Notes = "Quartalsbestellung Uhrwerke Q2 2026",
            Lines =
            [
                new() { MaterialId = MasterDataSeedData.MatEta2824Id, Description = "ETA 2824-2 Automatikwerk", Quantity = 100, UnitPrice = 185.00m, VatRate = VatRate.Standard, Position = 1 },
            ],
        };
        foreach (var line in po1.Lines)
        {
            line.LineTotal = line.Quantity * line.UnitPrice * (1 - line.DiscountPct / 100m);
        }
        po1.Subtotal = po1.Lines.Sum(l => l.LineTotal);
        po1.VatAmount = po1.Lines.Sum(l => l.LineTotal * l.VatRate.Multiplier());
        po1.TotalAmount = po1.Subtotal + po1.VatAmount;

        var po2 = new PurchaseOrder
        {
            OrderNumber = "PO-2026-00002",
            SupplierId = SuppSwissCrystalId,
            Status = PurchaseOrderStatus.Draft,
            OrderDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)),
            Currency = "CHF",
            Notes = "Saphirglas-Bestellung für Sommer-Kollektion",
            Lines =
            [
                new() { MaterialId = MasterDataSeedData.MatSapphireCrystalId, Description = "Saphirglas 42mm", Quantity = 200, UnitPrice = 45.00m, VatRate = VatRate.Standard, Position = 1 },
            ],
        };
        foreach (var line in po2.Lines)
        {
            line.LineTotal = line.Quantity * line.UnitPrice * (1 - line.DiscountPct / 100m);
        }
        po2.Subtotal = po2.Lines.Sum(l => l.LineTotal);
        po2.VatAmount = po2.Lines.Sum(l => l.LineTotal * l.VatRate.Multiplier());
        po2.TotalAmount = po2.Subtotal + po2.VatAmount;

        await db.Set<PurchaseOrder>().AddRangeAsync([po1, po2]);
        await db.SaveChangesAsync();

        logger.LogInformation("Purchasing seeded: {SuppCount} suppliers, 2 orders", suppliers.Count);
    }
}
