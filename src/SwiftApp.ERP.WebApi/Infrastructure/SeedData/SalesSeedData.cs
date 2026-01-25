using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Sales.Domain.Entities;
using SwiftApp.ERP.Modules.Sales.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.WebApi.Infrastructure.SeedData;

public static class SalesSeedData
{
    public static readonly Guid CustLuxTimeId = new("f1000000-0000-0000-0000-000000000001");
    public static readonly Guid CustChronoSwissId = new("f1000000-0000-0000-0000-000000000002");
    public static readonly Guid CustMuellerJuwelierId = new("f1000000-0000-0000-0000-000000000003");
    public static readonly Guid CustBernWatchId = new("f1000000-0000-0000-0000-000000000004");

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        if (await db.Set<Customer>().AnyAsync())
        {
            logger.LogInformation("Sales seed data already exists, skipping");
            return;
        }

        logger.LogInformation("Seeding Sales module...");

        // ── Customers ──
        var customers = new List<Customer>
        {
            new()
            {
                Id = CustLuxTimeId, CustomerNumber = "C-2026-001",
                CompanyName = "LuxTime AG", Email = "info@luxtime.ch", Phone = "+41 44 555 10 20",
                Street = "Paradeplatz 8", City = "Zürich", PostalCode = "8001", Canton = "ZH", Country = "CH",
                VatNumber = "CHE-111.222.333", PaymentTerms = 30, CreditLimit = 500000m, Active = true,
            },
            new()
            {
                Id = CustChronoSwissId, CustomerNumber = "C-2026-002",
                CompanyName = "ChronoSwiss Boutique", Email = "orders@chronoswiss.ch", Phone = "+41 22 700 50 60",
                Street = "Rue du Rhône 62", City = "Genève", PostalCode = "1204", Canton = "GE", Country = "CH",
                VatNumber = "CHE-444.555.666", PaymentTerms = 45, CreditLimit = 750000m, Active = true,
            },
            new()
            {
                Id = CustMuellerJuwelierId, CustomerNumber = "C-2026-003",
                CompanyName = "Müller Juwelier", Email = "einkauf@mueller-juwelier.ch", Phone = "+41 31 300 20 10",
                Street = "Kramgasse 52", City = "Bern", PostalCode = "3011", Canton = "BE", Country = "CH",
                PaymentTerms = 30, CreditLimit = 200000m, Active = true,
            },
            new()
            {
                Id = CustBernWatchId, CustomerNumber = "C-2026-004",
                FirstName = "Hans", LastName = "Berger",
                Email = "hans.berger@bernwatch.ch", Phone = "+41 79 543 21 00",
                Street = "Marktgasse 18", City = "Bern", PostalCode = "3011", Canton = "BE", Country = "CH",
                PaymentTerms = 15, CreditLimit = 100000m, Active = true,
            },
        };
        await db.Set<Customer>().AddRangeAsync(customers);
        await db.SaveChangesAsync();

        // ── Sales Orders ──
        var orderDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));

        var order1 = new SalesOrder
        {
            OrderNumber = "SO-2026-00001",
            CustomerId = CustLuxTimeId,
            Status = SalesOrderStatus.Confirmed,
            OrderDate = orderDate,
            DeliveryDate = orderDate.AddDays(14),
            Currency = "CHF",
            Notes = "Erstbestellung LuxTime — Express-Lieferung gewünscht",
            ShippingStreet = "Paradeplatz 8", ShippingCity = "Zürich", ShippingPostalCode = "8001", ShippingCanton = "ZH", ShippingCountry = "CH",
            Lines =
            [
                new() { ProductId = MasterDataSeedData.ProdAlpineChronoId, Description = "Alpine Chronograph", Quantity = 5, UnitPrice = 3950.00m, VatRate = VatRate.Standard, Position = 1 },
                new() { ProductId = MasterDataSeedData.ProdGenevaClassicId, Description = "Geneva Classic", Quantity = 3, UnitPrice = 5200.00m, VatRate = VatRate.Standard, Position = 2 },
            ],
        };
        // Calculate totals
        foreach (var line in order1.Lines)
        {
            line.LineTotal = line.Quantity * line.UnitPrice * (1 - line.DiscountPct / 100m);
        }
        order1.Subtotal = order1.Lines.Sum(l => l.LineTotal);
        order1.VatAmount = order1.Lines.Sum(l => l.LineTotal * l.VatRate.Multiplier());
        order1.TotalAmount = order1.Subtotal + order1.VatAmount;

        var order2 = new SalesOrder
        {
            OrderNumber = "SO-2026-00002",
            CustomerId = CustChronoSwissId,
            Status = SalesOrderStatus.Draft,
            OrderDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)),
            Currency = "CHF",
            Notes = "Anfrage für Boutique-Sonderedition",
            Lines =
            [
                new() { ProductId = MasterDataSeedData.ProdLuzernTourbillonId, Description = "Luzern Tourbillon", Quantity = 2, UnitPrice = 48000.00m, VatRate = VatRate.Standard, Position = 1 },
                new() { ProductId = MasterDataSeedData.ProdMatterhornDiverId, Description = "Matterhorn Diver", Quantity = 10, UnitPrice = 6800.00m, DiscountPct = 5m, VatRate = VatRate.Standard, Position = 2 },
            ],
        };
        foreach (var line in order2.Lines)
        {
            line.LineTotal = line.Quantity * line.UnitPrice * (1 - line.DiscountPct / 100m);
        }
        order2.Subtotal = order2.Lines.Sum(l => l.LineTotal);
        order2.VatAmount = order2.Lines.Sum(l => l.LineTotal * l.VatRate.Multiplier());
        order2.TotalAmount = order2.Subtotal + order2.VatAmount;

        await db.Set<SalesOrder>().AddRangeAsync([order1, order2]);
        await db.SaveChangesAsync();

        logger.LogInformation("Sales seeded: {CustCount} customers, 2 orders",
            customers.Count);
    }
}
