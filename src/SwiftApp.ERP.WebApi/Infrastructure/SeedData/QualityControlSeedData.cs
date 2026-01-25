using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.QualityControl.Domain.Entities;
using SwiftApp.ERP.Modules.QualityControl.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.WebApi.Infrastructure.SeedData;

public static class QualityControlSeedData
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        if (await db.Set<InspectionPlan>().AnyAsync())
        {
            logger.LogInformation("QualityControl seed data already exists, skipping");
            return;
        }

        logger.LogInformation("Seeding QualityControl module...");

        // ── Inspection Plans ──
        var plans = new List<InspectionPlan>
        {
            new()
            {
                PlanNumber = "IP-001", Name = "Ganggenauigkeit / Accuracy Test",
                Description = "Zeitmessung über 5 Positionen und 2 Temperaturen nach COSC-Standard",
                ProductId = MasterDataSeedData.ProdAlpineChronoId,
                Criteria = "Abweichung max. -4/+6 s/Tag (COSC)", Frequency = "Jedes Stück / 100%",
                Active = true,
            },
            new()
            {
                PlanNumber = "IP-002", Name = "Wasserdichtigkeit / Water Resistance",
                Description = "Druckprüfung bis 200m (20 ATM) für Taucheruhren",
                ProductId = MasterDataSeedData.ProdMatterhornDiverId,
                Criteria = "Kein Eindringen bei 25 ATM (125% Nennwert)", Frequency = "Jedes Stück / 100%",
                Active = true,
            },
            new()
            {
                PlanNumber = "IP-003", Name = "Saphirglas Eingangskonrolle",
                Description = "Optische Prüfung und Härtetest der angelieferten Saphirgläser",
                MaterialId = MasterDataSeedData.MatSapphireCrystalId,
                Criteria = "Keine Kratzer, Mohs-Härte 9, Transparenz >95%", Frequency = "Stichprobe 10%",
                Active = true,
            },
            new()
            {
                PlanNumber = "IP-004", Name = "Endkontrolle / Final Inspection",
                Description = "Visuelle Prüfung und Funktionskontrolle vor Verpackung",
                Criteria = "Keine sichtbaren Mängel, alle Funktionen intakt, Gravur korrekt", Frequency = "Jedes Stück / 100%",
                Active = true,
            },
        };
        await db.Set<InspectionPlan>().AddRangeAsync(plans);
        await db.SaveChangesAsync();

        // ── Quality Checks ──
        var checks = new List<QualityCheck>
        {
            new()
            {
                CheckNumber = "QC-2026-00001",
                InspectionPlanId = plans[0].Id,
                InspectorName = "Marco Rossi",
                CheckDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
                Result = QualityCheckResult.Pass,
                ItemId = MasterDataSeedData.ProdAlpineChronoId,
                BatchNumber = "PRD-2026-00001",
                SampleSize = 20, DefectCount = 0,
                Notes = "Alle 20 Stück innerhalb COSC-Toleranz. Durchschnitt: +2.1 s/Tag.",
            },
            new()
            {
                CheckNumber = "QC-2026-00002",
                InspectionPlanId = plans[2].Id,
                InspectorName = "Marco Rossi",
                CheckDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)),
                Result = QualityCheckResult.Fail,
                ItemId = MasterDataSeedData.MatSapphireCrystalId,
                BatchNumber = "LOT-SC-2026-004",
                SampleSize = 20, DefectCount = 3,
                Notes = "3 von 20 Saphirgläsern mit Mikrokratzern. Lieferung teilweise zurückgewiesen.",
            },
            new()
            {
                CheckNumber = "QC-2026-00003",
                InspectionPlanId = plans[3].Id,
                InspectorName = "Sophie Dubois",
                CheckDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                Result = QualityCheckResult.Conditional,
                ItemId = MasterDataSeedData.ProdGenevaClassicId,
                SampleSize = 5, DefectCount = 1,
                Notes = "1 Stück mit leichtem Versatz der Datumsanzeige. Nacharbeit und erneute Prüfung erforderlich.",
            },
        };
        await db.Set<QualityCheck>().AddRangeAsync(checks);
        await db.SaveChangesAsync();

        // ── Non-Conformance Reports ──
        var ncrs = new List<NonConformanceReport>
        {
            new()
            {
                NcrNumber = "NCR-2026-00001",
                QualityCheckId = checks[1].Id,
                Description = "Mikrokratzer auf Saphirgläsern Charge LOT-SC-2026-004",
                Severity = NcrSeverity.Major,
                Status = NcrStatus.InProgress,
                RootCause = "Verpackungsmangel beim Lieferanten — ungenügende Polsterung im Transportbehälter",
                CorrectiveAction = "1) Reklamation an Swiss Crystal Components AG. 2) Verschärfte Eingangskontrolle. 3) Verpackungsanforderungen in Lieferantenvereinbarung aufnehmen.",
                ResponsiblePerson = "Marco Rossi",
                DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            },
            new()
            {
                NcrNumber = "NCR-2026-00002",
                QualityCheckId = checks[2].Id,
                Description = "Datumsanzeige-Versatz bei Geneva Classic SN GC-2026-0042",
                Severity = NcrSeverity.Minor,
                Status = NcrStatus.Open,
                CorrectiveAction = "Nacharbeit in Montage — Datumsscheibe nachjustieren",
                ResponsiblePerson = "Marie Favre",
                DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            },
        };
        await db.Set<NonConformanceReport>().AddRangeAsync(ncrs);
        await db.SaveChangesAsync();

        logger.LogInformation("QualityControl seeded: {PlanCount} plans, {CheckCount} checks, {NcrCount} NCRs",
            plans.Count, checks.Count, ncrs.Count);
    }
}
