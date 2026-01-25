using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Auth.Domain.Entities;
using SwiftApp.ERP.Modules.Notification.Domain.Entities;
using SwiftApp.ERP.Modules.Notification.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Persistence;
using NotificationEntity = SwiftApp.ERP.Modules.Notification.Domain.Entities.Notification;

namespace SwiftApp.ERP.WebApi.Infrastructure.SeedData;

public static class NotificationSeedData
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        if (!await db.Set<NotificationTemplate>().AnyAsync())
        {
            logger.LogInformation("Seeding Notification templates...");

        var templates = new List<NotificationTemplate>
        {
            // ── Sales ──
            Template("sales.order.created", NotificationChannel.Email, "de",
                "Neuer Kundenauftrag: {{ order_number }}",
                "<h2>Kundenauftrag erstellt</h2><p>Der Kundenauftrag <strong>{{ order_number }}</strong> wurde erstellt.</p>"),
            Template("sales.order.created", NotificationChannel.Email, "en",
                "New Sales Order: {{ order_number }}",
                "<h2>Sales Order Created</h2><p>Sales order <strong>{{ order_number }}</strong> has been created.</p>"),
            Template("sales.order.confirmed", NotificationChannel.Email, "de",
                "Kundenauftrag bestätigt: {{ order_number }}",
                "<h2>Kundenauftrag bestätigt</h2><p>Der Kundenauftrag <strong>{{ order_number }}</strong> wurde bestätigt.</p>"),
            Template("sales.order.confirmed", NotificationChannel.Email, "en",
                "Sales Order Confirmed: {{ order_number }}",
                "<h2>Sales Order Confirmed</h2><p>Sales order <strong>{{ order_number }}</strong> has been confirmed.</p>"),
            Template("sales.order.cancelled", NotificationChannel.Email, "de",
                "Kundenauftrag storniert: {{ order_number }}",
                "<h2>Kundenauftrag storniert</h2><p>Der Kundenauftrag <strong>{{ order_number }}</strong> wurde storniert.</p>"),
            Template("sales.order.cancelled", NotificationChannel.Email, "en",
                "Sales Order Cancelled: {{ order_number }}",
                "<h2>Sales Order Cancelled</h2><p>Sales order <strong>{{ order_number }}</strong> has been cancelled.</p>"),

            // ── Purchasing ──
            Template("purchasing.order.created", NotificationChannel.Email, "de",
                "Neue Bestellung: {{ order_number }}",
                "<h2>Bestellung erstellt</h2><p>Die Bestellung <strong>{{ order_number }}</strong> wurde erstellt.</p>"),
            Template("purchasing.order.confirmed", NotificationChannel.Email, "de",
                "Bestellung bestätigt: {{ order_number }}",
                "<h2>Bestellung bestätigt</h2><p>Die Bestellung <strong>{{ order_number }}</strong> wurde bestätigt.</p>"),
            Template("purchasing.order.received", NotificationChannel.InApp, "de",
                "Wareneingang: {{ order_number }}",
                "<p>Bestellung <strong>{{ order_number }}</strong> wurde empfangen.</p>"),
            Template("purchasing.order.cancelled", NotificationChannel.Email, "de",
                "Bestellung storniert: {{ order_number }}",
                "<h2>Bestellung storniert</h2><p>Bestellung <strong>{{ order_number }}</strong> wurde storniert.</p>"),

            // ── Production ──
            Template("production.order.created", NotificationChannel.InApp, "de",
                "Neuer Produktionsauftrag: {{ order_number }}",
                "<p>Produktionsauftrag <strong>{{ order_number }}</strong> wurde erstellt.</p>"),
            Template("production.order.released", NotificationChannel.Email, "de",
                "Produktionsauftrag freigegeben: {{ order_number }}",
                "<h2>Freigabe</h2><p>Produktionsauftrag <strong>{{ order_number }}</strong> wurde zur Produktion freigegeben.</p>"),
            Template("production.order.completed", NotificationChannel.Email, "de",
                "Produktionsauftrag abgeschlossen: {{ order_number }}",
                "<h2>Abschluss</h2><p>Produktionsauftrag <strong>{{ order_number }}</strong> wurde abgeschlossen.</p>"),
            Template("production.order.cancelled", NotificationChannel.Email, "de",
                "Produktionsauftrag storniert: {{ order_number }}",
                "<h2>Stornierung</h2><p>Produktionsauftrag <strong>{{ order_number }}</strong> wurde storniert.</p>"),

            // ── Quality Control ──
            Template("qc.check.completed", NotificationChannel.Email, "de",
                "Qualitätsprüfung abgeschlossen: {{ check_number }}",
                "<h2>QC Ergebnis</h2><p>Qualitätsprüfung <strong>{{ check_number }}</strong> abgeschlossen. Ergebnis: {{ result }}</p>"),
            Template("qc.ncr.created", NotificationChannel.Email, "de",
                "Neue Reklamation: {{ ncr_number }}",
                "<h2>Non-Conformance Report</h2><p>NCR <strong>{{ ncr_number }}</strong> wurde erstellt.</p>"),
            Template("qc.ncr.closed", NotificationChannel.InApp, "de",
                "Reklamation geschlossen: {{ ncr_number }}",
                "<p>NCR <strong>{{ ncr_number }}</strong> wurde geschlossen.</p>"),

            // ── Accounting ──
            Template("accounting.entry.posted", NotificationChannel.InApp, "de",
                "Buchung erfasst: {{ entry_number }}",
                "<p>Journalbuchung <strong>{{ entry_number }}</strong> erfasst. Betrag: CHF {{ total_amount }}</p>"),
            Template("accounting.entry.reversed", NotificationChannel.InApp, "de",
                "Buchung storniert: {{ entry_number }}",
                "<p>Journalbuchung <strong>{{ entry_number }}</strong> wurde storniert.</p>"),

            // ── HR ──
            Template("hr.employee.hired", NotificationChannel.Email, "de",
                "Neuer Mitarbeiter: {{ name }}",
                "<h2>Willkommen</h2><p>Mitarbeiter <strong>{{ name }}</strong> ({{ employee_number }}) wurde eingestellt.</p>"),
            Template("hr.employee.terminated", NotificationChannel.Email, "de",
                "Mitarbeiter ausgetreten: {{ employee_number }}",
                "<h2>Austritt</h2><p>Mitarbeiter <strong>{{ employee_number }}</strong> tritt per {{ termination_date }} aus.</p>"),

            // ── MasterData ──
            Template("masterdata.product.created", NotificationChannel.InApp, "de",
                "Neues Produkt: {{ name }}",
                "<p>Produkt <strong>{{ name }}</strong> (SKU: {{ sku }}) wurde erstellt.</p>"),
            Template("masterdata.product.updated", NotificationChannel.InApp, "de",
                "Produkt aktualisiert: {{ sku }}",
                "<p>Produkt <strong>{{ sku }}</strong> wurde aktualisiert.</p>"),
            Template("masterdata.product.deleted", NotificationChannel.Email, "de",
                "Produkt gelöscht: {{ sku }}",
                "<h2>Löschung</h2><p>Produkt <strong>{{ sku }}</strong> wurde gelöscht.</p>"),

            // ── CRM ──
            Template("crm.contact.created", NotificationChannel.InApp, "de",
                "Neuer Kontakt: {{ name }}",
                "<p>CRM-Kontakt <strong>{{ name }}</strong> wurde angelegt.</p>"),
            Template("crm.interaction.created", NotificationChannel.InApp, "de",
                "Neue Interaktion: {{ interaction_type }}",
                "<p>Neue <strong>{{ interaction_type }}</strong>-Interaktion wurde erfasst.</p>"),
        };

        db.Set<NotificationTemplate>().AddRange(templates);
        await db.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} notification templates", templates.Count);
        }

        // ── Seed demo in-app notifications (always check independently) ──
        await SeedDemoNotificationsAsync(db, logger);
    }

    private static async Task SeedDemoNotificationsAsync(AppDbContext db, ILogger logger)
    {
        var adminUser = await db.Set<User>().FirstOrDefaultAsync(u => u.Username == "admin");
        if (adminUser is null)
        {
            logger.LogWarning("Admin user not found, skipping demo notification seeding");
            return;
        }

        // Check if notifications already exist for the real admin user
        if (await db.Set<NotificationEntity>()
            .IgnoreQueryFilters()
            .AnyAsync(n => n.RecipientUserId == adminUser.Id))
            return;

        // Clean up any orphaned seed notifications with wrong user IDs
        var orphaned = await db.Set<NotificationEntity>()
            .IgnoreQueryFilters()
            .Where(n => n.RecipientUserId == Guid.Parse("00000000-0000-0000-0000-000000000001"))
            .ToListAsync();
        if (orphaned.Count > 0)
        {
            db.Set<NotificationEntity>().RemoveRange(orphaned);
            await db.SaveChangesAsync();
            logger.LogInformation("Removed {Count} orphaned seed notifications", orphaned.Count);
        }

        var baseDate = DateTimeOffset.UtcNow;
        var notifications = new List<NotificationEntity>
        {
            new()
            {
                RecipientUserId = adminUser.Id,
                Channel = NotificationChannel.InApp,
                Status = NotificationStatus.Sent,
                Subject = "New sales order SO-2026-042",
                Body = "Customer Bühler Uhren AG placed a new order worth CHF 45,800.00",
                ReferenceType = "SalesOrder",
                CreatedAt = baseDate.AddHours(-1),
            },
            new()
            {
                RecipientUserId = adminUser.Id,
                Channel = NotificationChannel.InApp,
                Status = NotificationStatus.Sent,
                Subject = "Low stock alert: Sapphire Crystal",
                Body = "Material 'Sapphire Crystal 40mm' has dropped below the minimum level (12 remaining)",
                ReferenceType = "Inventory",
                CreatedAt = baseDate.AddHours(-3),
            },
            new()
            {
                RecipientUserId = adminUser.Id,
                Channel = NotificationChannel.InApp,
                Status = NotificationStatus.Sent,
                Subject = "Production order PO-2026-018 completed",
                Body = "Batch of 25 Nautilus chronographs finished quality inspection",
                ReferenceType = "ProductionOrder",
                CreatedAt = baseDate.AddHours(-6),
            },
            new()
            {
                RecipientUserId = adminUser.Id,
                Channel = NotificationChannel.InApp,
                Status = NotificationStatus.Sent,
                Subject = "Quality check failed: NCR-2026-007",
                Body = "Non-conformance reported on movement caliber 3120 — dial alignment issue",
                ReferenceType = "QualityCheck",
                CreatedAt = baseDate.AddHours(-12),
                ReadAt = baseDate.AddHours(-10),
            },
            new()
            {
                RecipientUserId = adminUser.Id,
                Channel = NotificationChannel.InApp,
                Status = NotificationStatus.Sent,
                Subject = "Purchase order PUR-2026-031 delivered",
                Body = "Shipment from Swatch Group received — 500 watch straps, 200 buckles",
                ReferenceType = "PurchaseOrder",
                CreatedAt = baseDate.AddDays(-1),
                ReadAt = baseDate.AddHours(-20),
            },
            new()
            {
                RecipientUserId = adminUser.Id,
                Channel = NotificationChannel.InApp,
                Status = NotificationStatus.Sent,
                Subject = "New employee onboarded",
                Body = "Lena Fischer (Senior Watchmaker) has been added to the Produktion department",
                ReferenceType = "Employee",
                CreatedAt = baseDate.AddDays(-2),
                ReadAt = baseDate.AddDays(-1),
            },
            new()
            {
                RecipientUserId = adminUser.Id,
                Channel = NotificationChannel.InApp,
                Status = NotificationStatus.Sent,
                Subject = "Monthly revenue report ready",
                Body = "March 2026 revenue: CHF 1,245,000 — 15% above target",
                ReferenceType = "JournalEntry",
                CreatedAt = baseDate.AddDays(-3),
            }
        };

        db.Set<NotificationEntity>().AddRange(notifications);
        await db.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} demo notifications for admin user", notifications.Count);
    }

    private static NotificationTemplate Template(string code, NotificationChannel channel,
        string locale, string subject, string bodyTemplate) => new()
    {
        Code = code,
        Channel = channel,
        Locale = locale,
        Subject = subject,
        BodyTemplate = bodyTemplate,
        Active = true
    };
}
