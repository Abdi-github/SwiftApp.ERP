using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.MasterData.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.WebApi.Infrastructure.SeedData;

public static class MasterDataSeedData
{
    // Expose IDs for cross-module seeding
    public static readonly Guid CatWatchesId = new("a0000000-0000-0000-0000-000000000001");
    public static readonly Guid CatMovementsId = new("a0000000-0000-0000-0000-000000000002");
    public static readonly Guid CatCasesId = new("a0000000-0000-0000-0000-000000000003");
    public static readonly Guid CatDialsId = new("a0000000-0000-0000-0000-000000000004");
    public static readonly Guid CatStrapsId = new("a0000000-0000-0000-0000-000000000005");
    public static readonly Guid CatToolsId = new("a0000000-0000-0000-0000-000000000006");

    public static readonly Guid UomPcsId = new("b0000000-0000-0000-0000-000000000001");
    public static readonly Guid UomGrmId = new("b0000000-0000-0000-0000-000000000002");
    public static readonly Guid UomMtrId = new("b0000000-0000-0000-0000-000000000003");
    public static readonly Guid UomLtrId = new("b0000000-0000-0000-0000-000000000004");
    public static readonly Guid UomSetId = new("b0000000-0000-0000-0000-000000000005");

    // Products (finished watches)
    public static readonly Guid ProdAlpineChronoId = new("c0000000-0000-0000-0000-000000000001");
    public static readonly Guid ProdGenevaClassicId = new("c0000000-0000-0000-0000-000000000002");
    public static readonly Guid ProdMatterhornDiverId = new("c0000000-0000-0000-0000-000000000003");
    public static readonly Guid ProdBernMinimalId = new("c0000000-0000-0000-0000-000000000004");
    public static readonly Guid ProdLuzernTourbillonId = new("c0000000-0000-0000-0000-000000000005");

    // Materials (components)
    public static readonly Guid MatEta2824Id = new("d0000000-0000-0000-0000-000000000001");
    public static readonly Guid MatSapphireCrystalId = new("d0000000-0000-0000-0000-000000000002");
    public static readonly Guid MatSs316lId = new("d0000000-0000-0000-0000-000000000003");
    public static readonly Guid MatLeatherStrapId = new("d0000000-0000-0000-0000-000000000004");
    public static readonly Guid MatGold18kId = new("d0000000-0000-0000-0000-000000000005");
    public static readonly Guid MatTitaniumCaseId = new("d0000000-0000-0000-0000-000000000006");
    public static readonly Guid MatRubberStrapId = new("d0000000-0000-0000-0000-000000000007");
    public static readonly Guid MatDialBlackId = new("d0000000-0000-0000-0000-000000000008");
    public static readonly Guid MatDialWhiteId = new("d0000000-0000-0000-0000-000000000009");
    public static readonly Guid MatHandsRoseGoldId = new("d0000000-0000-0000-0000-00000000000a");

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        if (await db.Set<Category>().AnyAsync())
        {
            logger.LogInformation("MasterData seed data already exists, skipping");
            return;
        }

        logger.LogInformation("Seeding MasterData module...");

        // ── Categories ──
        var categories = CreateCategories();
        await db.Set<Category>().AddRangeAsync(categories);
        await db.SaveChangesAsync();

        // ── Units of Measure ──
        var uoms = CreateUnitsOfMeasure();
        await db.Set<UnitOfMeasure>().AddRangeAsync(uoms);
        await db.SaveChangesAsync();

        // ── Products (finished watches) ──
        var products = CreateProducts();
        await db.Set<Product>().AddRangeAsync(products);
        await db.SaveChangesAsync();

        // ── Materials (watch components) ──
        var materials = CreateMaterials();
        await db.Set<Material>().AddRangeAsync(materials);
        await db.SaveChangesAsync();

        // ── Bills of Material ──
        var boms = CreateBillsOfMaterial();
        await db.Set<BillOfMaterial>().AddRangeAsync(boms);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "MasterData seeded: {CatCount} categories, {UomCount} UoMs, {ProdCount} products, {MatCount} materials, {BomCount} BOMs",
            categories.Count, uoms.Count, products.Count, materials.Count, boms.Count);
    }

    private static List<Category> CreateCategories() =>
    [
        new() { Id = CatWatchesId, Name = "Uhren", Description = "Fertige Uhren / Finished Watches" },
        new() { Id = CatMovementsId, Name = "Uhrwerke", Description = "Mechanische und Quarz-Uhrwerke", ParentCategoryId = CatWatchesId },
        new() { Id = CatCasesId, Name = "Gehäuse", Description = "Uhrengehäuse aus verschiedenen Materialien" },
        new() { Id = CatDialsId, Name = "Zifferblätter", Description = "Zifferblätter und Zeiger" },
        new() { Id = CatStrapsId, Name = "Armbänder", Description = "Leder-, Metall- und Kautschukarmbänder" },
        new() { Id = CatToolsId, Name = "Werkzeuge", Description = "Uhrmacherwerkzeuge und Hilfsmittel" },
    ];

    private static List<UnitOfMeasure> CreateUnitsOfMeasure() =>
    [
        new() { Id = UomPcsId, Code = "PCS", Name = "Stück", Description = "Pieces / Stück" },
        new() { Id = UomGrmId, Code = "GRM", Name = "Gramm", Description = "Grams / Gramm" },
        new() { Id = UomMtrId, Code = "MTR", Name = "Meter", Description = "Meters / Meter" },
        new() { Id = UomLtrId, Code = "LTR", Name = "Liter", Description = "Liters / Liter" },
        new() { Id = UomSetId, Code = "SET", Name = "Set", Description = "Set / Satz" },
    ];

    private static List<Product> CreateProducts() =>
    [
        new()
        {
            Id = ProdAlpineChronoId,
            Sku = "SW-ALP-CHR-001",
            Name = "Alpine Chronograph",
            Description = "Sportlicher Chronograph mit Edelstahlgehäuse und Saphirglas, 42mm",
            CategoryId = CatWatchesId,
            UnitPrice = 3950.00m,
            ListPrice = 4500.00m,
            VatRate = VatRate.Standard,
            Active = true,
        },
        new()
        {
            Id = ProdGenevaClassicId,
            Sku = "SW-GEN-CLS-001",
            Name = "Geneva Classic",
            Description = "Elegante Dreizeigeruhr mit Lederarmband, 40mm Roségold-Gehäuse",
            CategoryId = CatWatchesId,
            UnitPrice = 5200.00m,
            ListPrice = 5900.00m,
            VatRate = VatRate.Standard,
            Active = true,
        },
        new()
        {
            Id = ProdMatterhornDiverId,
            Sku = "SW-MAT-DIV-001",
            Name = "Matterhorn Diver",
            Description = "Professionelle Taucheruhr, 300m wasserdicht, Titangehäuse, 44mm",
            CategoryId = CatWatchesId,
            UnitPrice = 6800.00m,
            ListPrice = 7500.00m,
            VatRate = VatRate.Standard,
            Active = true,
        },
        new()
        {
            Id = ProdBernMinimalId,
            Sku = "SW-BRN-MIN-001",
            Name = "Bern Minimal",
            Description = "Minimalistisches Design, ultraflaches Quarzwerk, 38mm",
            CategoryId = CatWatchesId,
            UnitPrice = 1850.00m,
            ListPrice = 2200.00m,
            VatRate = VatRate.Standard,
            Active = true,
        },
        new()
        {
            Id = ProdLuzernTourbillonId,
            Sku = "SW-LUZ-TRB-001",
            Name = "Luzern Tourbillon",
            Description = "Haute Horlogerie Tourbillon mit 18K Goldgehäuse, limitierte Edition",
            CategoryId = CatWatchesId,
            UnitPrice = 48000.00m,
            ListPrice = 55000.00m,
            VatRate = VatRate.Standard,
            Active = true,
        },
    ];

    private static List<Material> CreateMaterials() =>
    [
        new()
        {
            Id = MatEta2824Id,
            Sku = "MAT-ETA-2824",
            Name = "ETA 2824-2 Automatikwerk",
            Description = "Swiss Made Automatikkaliber, 25 Steine, 28'800 A/h",
            CategoryId = CatMovementsId,
            UnitOfMeasureId = UomPcsId,
            UnitPrice = 185.00m,
            VatRate = VatRate.Standard,
            MinimumStock = 50,
            Active = true,
        },
        new()
        {
            Id = MatSapphireCrystalId,
            Sku = "MAT-SAPH-42",
            Name = "Saphirglas 42mm",
            Description = "Beidseitig entspiegeltes Saphirglas, doppelt gewölbt",
            CategoryId = CatCasesId,
            UnitOfMeasureId = UomPcsId,
            UnitPrice = 45.00m,
            VatRate = VatRate.Standard,
            MinimumStock = 100,
            Active = true,
        },
        new()
        {
            Id = MatSs316lId,
            Sku = "MAT-SS316L-CASE",
            Name = "Edelstahlgehäuse 316L 42mm",
            Description = "Gehäusemittelteil aus chirurgischem Edelstahl 316L",
            CategoryId = CatCasesId,
            UnitOfMeasureId = UomPcsId,
            UnitPrice = 120.00m,
            VatRate = VatRate.Standard,
            MinimumStock = 75,
            Active = true,
        },
        new()
        {
            Id = MatLeatherStrapId,
            Sku = "MAT-LEATH-BRN",
            Name = "Kalbsleder-Armband Braun",
            Description = "Handgenähtes Kalbslederarmband, 20mm Stegbreite",
            CategoryId = CatStrapsId,
            UnitOfMeasureId = UomPcsId,
            UnitPrice = 65.00m,
            VatRate = VatRate.Standard,
            MinimumStock = 100,
            Active = true,
        },
        new()
        {
            Id = MatGold18kId,
            Sku = "MAT-GOLD-18K",
            Name = "18K Roségold",
            Description = "18 Karat Roségold-Legierung für Gehäuse und Lünette",
            CategoryId = CatCasesId,
            UnitOfMeasureId = UomGrmId,
            UnitPrice = 58.50m,
            VatRate = VatRate.Standard,
            MinimumStock = 500,
            Active = true,
        },
        new()
        {
            Id = MatTitaniumCaseId,
            Sku = "MAT-TIT-CASE-44",
            Name = "Titangehäuse Grade 5 44mm",
            Description = "Leichtes und korrosionsbeständiges Titangehäuse",
            CategoryId = CatCasesId,
            UnitOfMeasureId = UomPcsId,
            UnitPrice = 280.00m,
            VatRate = VatRate.Standard,
            MinimumStock = 30,
            Active = true,
        },
        new()
        {
            Id = MatRubberStrapId,
            Sku = "MAT-RUBB-BLK",
            Name = "Kautschuk-Armband Schwarz",
            Description = "Vulkanisiertes FKM-Kautschukarmband, 22mm",
            CategoryId = CatStrapsId,
            UnitOfMeasureId = UomPcsId,
            UnitPrice = 35.00m,
            VatRate = VatRate.Standard,
            MinimumStock = 80,
            Active = true,
        },
        new()
        {
            Id = MatDialBlackId,
            Sku = "MAT-DIAL-BLK-42",
            Name = "Zifferblatt Schwarz 42mm",
            Description = "Mattschwarz lackiertes Zifferblatt mit Super-LumiNova Indizes",
            CategoryId = CatDialsId,
            UnitOfMeasureId = UomPcsId,
            UnitPrice = 55.00m,
            VatRate = VatRate.Standard,
            MinimumStock = 60,
            Active = true,
        },
        new()
        {
            Id = MatDialWhiteId,
            Sku = "MAT-DIAL-WHT-40",
            Name = "Zifferblatt Weiss 40mm",
            Description = "Silbriges Sonnenschliff-Zifferblatt, applizierte Indizes",
            CategoryId = CatDialsId,
            UnitOfMeasureId = UomPcsId,
            UnitPrice = 65.00m,
            VatRate = VatRate.Standard,
            MinimumStock = 40,
            Active = true,
        },
        new()
        {
            Id = MatHandsRoseGoldId,
            Sku = "MAT-HANDS-RG",
            Name = "Zeigersatz Roségold",
            Description = "Stunden-, Minuten- und Sekundenzeiger in Roségold PVD",
            CategoryId = CatDialsId,
            UnitOfMeasureId = UomSetId,
            UnitPrice = 28.00m,
            VatRate = VatRate.Standard,
            MinimumStock = 100,
            Active = true,
        },
    ];

    private static List<BillOfMaterial> CreateBillsOfMaterial() =>
    [
        // Alpine Chronograph BOM
        new() { ProductId = ProdAlpineChronoId, MaterialId = MatEta2824Id, Quantity = 1, UnitOfMeasureId = UomPcsId, Position = 1, Notes = "Automatikwerk" },
        new() { ProductId = ProdAlpineChronoId, MaterialId = MatSs316lId, Quantity = 1, UnitOfMeasureId = UomPcsId, Position = 2, Notes = "Edelstahlgehäuse" },
        new() { ProductId = ProdAlpineChronoId, MaterialId = MatSapphireCrystalId, Quantity = 1, UnitOfMeasureId = UomPcsId, Position = 3, Notes = "Saphirglas" },
        new() { ProductId = ProdAlpineChronoId, MaterialId = MatDialBlackId, Quantity = 1, UnitOfMeasureId = UomPcsId, Position = 4, Notes = "Schwarzes Zifferblatt" },
        new() { ProductId = ProdAlpineChronoId, MaterialId = MatRubberStrapId, Quantity = 1, UnitOfMeasureId = UomPcsId, Position = 5, Notes = "Kautschukarmband" },

        // Geneva Classic BOM
        new() { ProductId = ProdGenevaClassicId, MaterialId = MatEta2824Id, Quantity = 1, UnitOfMeasureId = UomPcsId, Position = 1, Notes = "Automatikwerk" },
        new() { ProductId = ProdGenevaClassicId, MaterialId = MatGold18kId, Quantity = 42.5m, UnitOfMeasureId = UomGrmId, Position = 2, Notes = "Roségold-Gehäuse" },
        new() { ProductId = ProdGenevaClassicId, MaterialId = MatSapphireCrystalId, Quantity = 1, UnitOfMeasureId = UomPcsId, Position = 3, Notes = "Saphirglas" },
        new() { ProductId = ProdGenevaClassicId, MaterialId = MatDialWhiteId, Quantity = 1, UnitOfMeasureId = UomPcsId, Position = 4, Notes = "Weisses Zifferblatt" },
        new() { ProductId = ProdGenevaClassicId, MaterialId = MatHandsRoseGoldId, Quantity = 1, UnitOfMeasureId = UomSetId, Position = 5, Notes = "Roségold-Zeiger" },
        new() { ProductId = ProdGenevaClassicId, MaterialId = MatLeatherStrapId, Quantity = 1, UnitOfMeasureId = UomPcsId, Position = 6, Notes = "Lederarmband" },

        // Matterhorn Diver BOM
        new() { ProductId = ProdMatterhornDiverId, MaterialId = MatEta2824Id, Quantity = 1, UnitOfMeasureId = UomPcsId, Position = 1, Notes = "Automatikwerk" },
        new() { ProductId = ProdMatterhornDiverId, MaterialId = MatTitaniumCaseId, Quantity = 1, UnitOfMeasureId = UomPcsId, Position = 2, Notes = "Titangehäuse" },
        new() { ProductId = ProdMatterhornDiverId, MaterialId = MatSapphireCrystalId, Quantity = 1, UnitOfMeasureId = UomPcsId, Position = 3, Notes = "Saphirglas" },
        new() { ProductId = ProdMatterhornDiverId, MaterialId = MatDialBlackId, Quantity = 1, UnitOfMeasureId = UomPcsId, Position = 4, Notes = "Schwarzes Zifferblatt" },
        new() { ProductId = ProdMatterhornDiverId, MaterialId = MatRubberStrapId, Quantity = 1, UnitOfMeasureId = UomPcsId, Position = 5, Notes = "Kautschukarmband" },
    ];
}
