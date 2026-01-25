using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Accounting.Domain.Entities;
using SwiftApp.ERP.Modules.Accounting.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.WebApi.Infrastructure.SeedData;

public static class AccountingSeedData
{
    // Swiss Chart of Accounts (Kontenrahmen KMU)
    public static readonly Guid AccCashId = new("f4000000-0000-0000-0000-000000001000");
    public static readonly Guid AccBankId = new("f4000000-0000-0000-0000-000000001020");
    public static readonly Guid AccReceivableId = new("f4000000-0000-0000-0000-000000001100");
    public static readonly Guid AccInventoryId = new("f4000000-0000-0000-0000-000000001200");
    public static readonly Guid AccPayableId = new("f4000000-0000-0000-0000-000000002000");
    public static readonly Guid AccVatPayableId = new("f4000000-0000-0000-0000-000000002200");
    public static readonly Guid AccEquityId = new("f4000000-0000-0000-0000-000000002800");
    public static readonly Guid AccRevenueId = new("f4000000-0000-0000-0000-000000003000");
    public static readonly Guid AccCogsId = new("f4000000-0000-0000-0000-000000004000");
    public static readonly Guid AccSalariesId = new("f4000000-0000-0000-0000-000000005000");
    public static readonly Guid AccRentId = new("f4000000-0000-0000-0000-000000006000");

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        if (await db.Set<Account>().AnyAsync())
        {
            logger.LogInformation("Accounting seed data already exists, skipping");
            return;
        }

        logger.LogInformation("Seeding Accounting module...");

        // ── Chart of Accounts (Swiss KMU Kontenrahmen) ──
        var accounts = new List<Account>
        {
            // 1xxx — Assets (Aktiven)
            new() { Id = AccCashId, AccountNumber = "1000", Name = "Kasse", Description = "Bargeldbestand / Cash on hand", AccountType = AccountType.Asset, Active = true },
            new() { Id = AccBankId, AccountNumber = "1020", Name = "Bank (UBS Geschäftskonto)", Description = "Bankguthaben UBS AG", AccountType = AccountType.Asset, Active = true },
            new() { Id = AccReceivableId, AccountNumber = "1100", Name = "Forderungen aus Lieferungen", Description = "Debitoren / Accounts receivable", AccountType = AccountType.Asset, Active = true },
            new() { Id = AccInventoryId, AccountNumber = "1200", Name = "Vorräte Fertigerzeugnisse", Description = "Lagerbestand Uhren / Finished goods inventory", AccountType = AccountType.Asset, Active = true },
            new() { AccountNumber = "1210", Name = "Vorräte Rohmaterial", Description = "Lagerbestand Komponenten / Raw materials", AccountType = AccountType.Asset, Active = true },
            new() { AccountNumber = "1500", Name = "Maschinen und Einrichtungen", Description = "Produktionsanlagen und Werkzeuge", AccountType = AccountType.Asset, Active = true },

            // 2xxx — Liabilities (Passiven)
            new() { Id = AccPayableId, AccountNumber = "2000", Name = "Verbindlichkeiten aus Lieferungen", Description = "Kreditoren / Accounts payable", AccountType = AccountType.Liability, Active = true },
            new() { Id = AccVatPayableId, AccountNumber = "2200", Name = "Geschuldete MWST", Description = "Mehrwertsteuer Verbindlichkeit / VAT payable", AccountType = AccountType.Liability, Active = true },
            new() { AccountNumber = "2300", Name = "Übrige kurzfristige Verbindlichkeiten", Description = "Sonstige kurzfristige Verbindlichkeiten", AccountType = AccountType.Liability, Active = true },

            // 28xx — Equity (Eigenkapital)
            new() { Id = AccEquityId, AccountNumber = "2800", Name = "Aktienkapital", Description = "Gezeichnetes Kapital / Share capital", AccountType = AccountType.Equity, Active = true },
            new() { AccountNumber = "2900", Name = "Gewinnvortrag", Description = "Retained earnings", AccountType = AccountType.Equity, Active = true },

            // 3xxx — Revenue (Ertrag)
            new() { Id = AccRevenueId, AccountNumber = "3000", Name = "Uhrenverkauf Inland", Description = "Umsatzerlöse Uhrenverkauf Schweiz", AccountType = AccountType.Revenue, Active = true },
            new() { AccountNumber = "3100", Name = "Uhrenverkauf Export", Description = "Umsatzerlöse Uhrenverkauf Ausland", AccountType = AccountType.Revenue, Active = true },
            new() { AccountNumber = "3200", Name = "Service und Reparaturen", Description = "Erträge aus Uhrenservice", AccountType = AccountType.Revenue, Active = true },

            // 4xxx — COGS (Warenaufwand)
            new() { Id = AccCogsId, AccountNumber = "4000", Name = "Materialaufwand", Description = "Einkauf Uhrwerk-Komponenten / Cost of goods", AccountType = AccountType.Expense, Active = true },
            new() { AccountNumber = "4200", Name = "Bestandesänderung Vorräte", Description = "Inventory change", AccountType = AccountType.Expense, Active = true },

            // 5xxx — Personnel (Personalaufwand)
            new() { Id = AccSalariesId, AccountNumber = "5000", Name = "Löhne und Gehälter", Description = "Bruttogehälter Uhrmacher und Verwaltung", AccountType = AccountType.Expense, Active = true },
            new() { AccountNumber = "5700", Name = "Sozialversicherungen", Description = "AHV/IV/EO/ALV Arbeitgeberanteil", AccountType = AccountType.Expense, Active = true },

            // 6xxx — Other expenses (Übriger Betriebsaufwand)
            new() { Id = AccRentId, AccountNumber = "6000", Name = "Raumkosten", Description = "Miete Atelier und Showroom", AccountType = AccountType.Expense, Active = true },
            new() { AccountNumber = "6500", Name = "Verwaltungskosten", Description = "Büromaterial, IT, Kommunikation", AccountType = AccountType.Expense, Active = true },
            new() { AccountNumber = "6800", Name = "Marketing und Werbung", Description = "Messebeteiligungen, Kataloge, Online-Marketing", AccountType = AccountType.Expense, Active = true },
        };
        await db.Set<Account>().AddRangeAsync(accounts);
        await db.SaveChangesAsync();

        // ── Opening Journal Entry ──
        var openingEntry = new JournalEntry
        {
            EntryNumber = "JE-2026-00001",
            Description = "Eröffnungsbuchung / Opening balance 2026",
            EntryDate = new DateOnly(2026, 1, 1),
            Posted = true,
            Reference = "OPENING-2026",
            Lines =
            [
                new() { AccountId = AccBankId, Description = "UBS Geschäftskonto Eröffnung", Debit = 2500000.00m, Credit = 0, Position = 1 },
                new() { AccountId = AccInventoryId, Description = "Fertigwarenlager Eröffnung", Debit = 750000.00m, Credit = 0, Position = 2 },
                new() { AccountId = AccEquityId, Description = "Aktienkapital", Debit = 0, Credit = 3250000.00m, Position = 3 },
            ],
        };
        await db.Set<JournalEntry>().AddAsync(openingEntry);
        await db.SaveChangesAsync();

        // Update account balances
        var accBank = await db.Set<Account>().FindAsync(AccBankId);
        if (accBank is not null) accBank.Balance = 2500000.00m;
        var accInv = await db.Set<Account>().FindAsync(AccInventoryId);
        if (accInv is not null) accInv.Balance = 750000.00m;
        var accEq = await db.Set<Account>().FindAsync(AccEquityId);
        if (accEq is not null) accEq.Balance = 3250000.00m;
        await db.SaveChangesAsync();

        logger.LogInformation("Accounting seeded: {AccCount} accounts, 1 opening journal entry", accounts.Count);
    }
}
