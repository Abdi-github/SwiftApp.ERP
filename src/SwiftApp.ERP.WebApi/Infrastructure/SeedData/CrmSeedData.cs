using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Crm.Domain.Entities;
using SwiftApp.ERP.Modules.Crm.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.WebApi.Infrastructure.SeedData;

public static class CrmSeedData
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        if (await db.Set<Contact>().AnyAsync())
        {
            logger.LogInformation("CRM seed data already exists, skipping");
            return;
        }

        logger.LogInformation("Seeding CRM module...");

        // ── Contacts (linked to Sales customers) ──
        var contacts = new List<Contact>
        {
            new()
            {
                FirstName = "Markus", LastName = "Hofer",
                Company = "LuxTime AG", Email = "m.hofer@luxtime.ch", Phone = "+41 44 555 10 21",
                Street = "Paradeplatz 8", City = "Zürich", PostalCode = "8001", Country = "CH",
                Notes = "Einkaufsleiter. Bevorzugt E-Mail-Kontakt. Besucht Baselworld jährlich.",
                CustomerId = SalesSeedData.CustLuxTimeId, Active = true,
            },
            new()
            {
                FirstName = "Isabelle", LastName = "Rochat",
                Company = "ChronoSwiss Boutique", Email = "i.rochat@chronoswiss.ch", Phone = "+41 22 700 50 61",
                Street = "Rue du Rhône 62", City = "Genève", PostalCode = "1204", Country = "CH",
                Notes = "Boutique-Managerin. Interessiert an limitierten Editionen und Tourbillons.",
                CustomerId = SalesSeedData.CustChronoSwissId, Active = true,
            },
            new()
            {
                FirstName = "Peter", LastName = "Müller",
                Company = "Müller Juwelier", Email = "p.mueller@mueller-juwelier.ch", Phone = "+41 31 300 20 11",
                Street = "Kramgasse 52", City = "Bern", PostalCode = "3011", Country = "CH",
                Notes = "Inhaber. Familienbetrieb in dritter Generation. Loyal, bevorzugt persönliche Besuche.",
                CustomerId = SalesSeedData.CustMuellerJuwelierId, Active = true,
            },
            new()
            {
                FirstName = "Elena", LastName = "Vogel",
                Email = "elena.vogel@outlook.com", Phone = "+41 78 900 11 22",
                Street = "Kirchweg 10", City = "Luzern", PostalCode = "6003", Country = "CH",
                Notes = "Potenzielle Grosskundin. Interesse an Corporate-Gifts für Bankkunden.",
                Active = true,
            },
        };
        await db.Set<Contact>().AddRangeAsync(contacts);
        await db.SaveChangesAsync();

        // ── Interactions ──
        var interactions = new List<Interaction>
        {
            new()
            {
                ContactId = contacts[0].Id,
                InteractionType = InteractionType.Meeting,
                Subject = "Quartalsmeeting Q1 2026",
                Description = "Besprechung der neuen Alpine Chronograph Serie. LuxTime will 50 Stück für Sommer-Kampagne. Follow-up nach Watches & Wonders Genf.",
                InteractionDate = DateTimeOffset.UtcNow.AddDays(-21),
                Completed = true,
            },
            new()
            {
                ContactId = contacts[0].Id,
                InteractionType = InteractionType.Email,
                Subject = "Bestellung SO-2026-00001 bestätigt",
                Description = "Bestellbestätigung per E-Mail verschickt. Express-Lieferung zugesagt.",
                InteractionDate = DateTimeOffset.UtcNow.AddDays(-7),
                Completed = true,
            },
            new()
            {
                ContactId = contacts[1].Id,
                InteractionType = InteractionType.Call,
                Subject = "Anfrage Luzern Tourbillon",
                Description = "Isabelle interessiert an 2 Stück Luzern Tourbillon für VIP-Kunden. Preisverhandlung steht noch aus. Möchte Gravur-Optionen besprechen.",
                InteractionDate = DateTimeOffset.UtcNow.AddDays(-3),
                FollowUpDate = DateTimeOffset.UtcNow.AddDays(7),
            },
            new()
            {
                ContactId = contacts[2].Id,
                InteractionType = InteractionType.Meeting,
                Subject = "Showroom-Besuch Bern Minimal",
                Description = "Peter hat 10 Stück Bern Minimal begutachtet. Qualität überzeugt. Preisvergleich mit Konkurrenz steht aus.",
                InteractionDate = DateTimeOffset.UtcNow.AddDays(-10),
                Completed = true,
            },
            new()
            {
                ContactId = contacts[3].Id,
                InteractionType = InteractionType.Call,
                Subject = "Erstgespräch Corporate Gifts",
                Description = "Elena erwägt 100 Bern Minimal als Corporate Gifts für Bankkunden. Budget ca. CHF 200'000. Nächster Schritt: Angebot erstellen.",
                InteractionDate = DateTimeOffset.UtcNow.AddDays(-5),
                FollowUpDate = DateTimeOffset.UtcNow.AddDays(3),
            },
        };
        await db.Set<Interaction>().AddRangeAsync(interactions);
        await db.SaveChangesAsync();

        logger.LogInformation("CRM seeded: {ContCount} contacts, {IntCount} interactions",
            contacts.Count, interactions.Count);
    }
}
