using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Hr.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.WebApi.Infrastructure.SeedData;

public static class HrSeedData
{
    public static readonly Guid DeptManagementId = new("f5000000-0000-0000-0000-000000000001");
    public static readonly Guid DeptProductionId = new("f5000000-0000-0000-0000-000000000002");
    public static readonly Guid DeptSalesId = new("f5000000-0000-0000-0000-000000000003");
    public static readonly Guid DeptFinanceId = new("f5000000-0000-0000-0000-000000000004");
    public static readonly Guid DeptQualityId = new("f5000000-0000-0000-0000-000000000005");

    public static readonly Guid EmpCeoId = new("f5100000-0000-0000-0000-000000000001");

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        if (await db.Set<Department>().AnyAsync())
        {
            logger.LogInformation("HR seed data already exists, skipping");
            return;
        }

        logger.LogInformation("Seeding HR module...");

        // ── Departments (without managers first) ──
        var departments = new List<Department>
        {
            new() { Id = DeptManagementId, Code = "MGMT", Name = "Geschäftsleitung", Description = "Direktion / Management", Active = true },
            new() { Id = DeptProductionId, Code = "PROD", Name = "Produktion", Description = "Uhrenherstellung und Montage", Active = true },
            new() { Id = DeptSalesId, Code = "SALES", Name = "Verkauf", Description = "Vertrieb und Kundenbetreuung", Active = true },
            new() { Id = DeptFinanceId, Code = "FIN", Name = "Finanzen", Description = "Buchhaltung und Controlling", Active = true },
            new() { Id = DeptQualityId, Code = "QC", Name = "Qualitätssicherung", Description = "Qualitätskontrolle und Zertifizierung", Active = true },
        };
        await db.Set<Department>().AddRangeAsync(departments);
        await db.SaveChangesAsync();

        // ── Employees ──
        var employees = new List<Employee>
        {
            new()
            {
                Id = EmpCeoId, EmployeeNumber = "EMP-001",
                FirstName = "Thomas", LastName = "Widmer",
                Email = "thomas.widmer@swiftapp.ch", Phone = "+41 79 100 10 01",
                DepartmentId = DeptManagementId, Position = "Geschäftsführer / CEO",
                HireDate = new DateOnly(2020, 3, 1), Salary = 250000.00m,
                Street = "Seestrasse 45", City = "Zürich", PostalCode = "8002", Canton = "ZH", Country = "CH",
                Active = true,
            },
            new()
            {
                EmployeeNumber = "EMP-002",
                FirstName = "Marie", LastName = "Favre",
                Email = "marie.favre@swiftapp.ch", Phone = "+41 79 200 20 02",
                DepartmentId = DeptProductionId, Position = "Uhrmachermeisterin / Master Watchmaker",
                HireDate = new DateOnly(2021, 6, 15), Salary = 120000.00m,
                Street = "Rue du Marché 12", City = "Biel/Bienne", PostalCode = "2502", Canton = "BE", Country = "CH",
                Active = true,
            },
            new()
            {
                EmployeeNumber = "EMP-003",
                FirstName = "Lukas", LastName = "Brunner",
                Email = "lukas.brunner@swiftapp.ch", Phone = "+41 79 300 30 03",
                DepartmentId = DeptSalesId, Position = "Verkaufsleiter / Sales Manager",
                HireDate = new DateOnly(2022, 1, 10), Salary = 110000.00m,
                Street = "Marktplatz 7", City = "Basel", PostalCode = "4001", Canton = "BS", Country = "CH",
                Active = true,
            },
            new()
            {
                EmployeeNumber = "EMP-004",
                FirstName = "Anna", LastName = "Keller",
                Email = "anna.keller@swiftapp.ch", Phone = "+41 79 400 40 04",
                DepartmentId = DeptFinanceId, Position = "Leiterin Finanzen / CFO",
                HireDate = new DateOnly(2021, 9, 1), Salary = 145000.00m,
                Street = "Bundesplatz 3", City = "Bern", PostalCode = "3011", Canton = "BE", Country = "CH",
                Active = true,
            },
            new()
            {
                EmployeeNumber = "EMP-005",
                FirstName = "Marco", LastName = "Rossi",
                Email = "marco.rossi@swiftapp.ch", Phone = "+41 79 500 50 05",
                DepartmentId = DeptQualityId, Position = "Qualitätsmanager / QC Manager",
                HireDate = new DateOnly(2023, 3, 15), Salary = 105000.00m,
                Street = "Via Nassa 22", City = "Lugano", PostalCode = "6900", Canton = "TI", Country = "CH",
                Active = true,
            },
            new()
            {
                EmployeeNumber = "EMP-006",
                FirstName = "Sophie", LastName = "Dubois",
                Email = "sophie.dubois@swiftapp.ch", Phone = "+41 79 600 60 06",
                DepartmentId = DeptProductionId, Position = "Uhrmacherin / Watchmaker",
                HireDate = new DateOnly(2024, 2, 1), Salary = 85000.00m,
                Street = "Quai du Mont-Blanc 5", City = "Genève", PostalCode = "1201", Canton = "GE", Country = "CH",
                Active = true,
            },
        };
        await db.Set<Employee>().AddRangeAsync(employees);
        await db.SaveChangesAsync();

        // ── Assign department managers ──
        var mgmt = await db.Set<Department>().FindAsync(DeptManagementId);
        if (mgmt is not null) mgmt.ManagerId = EmpCeoId;
        await db.SaveChangesAsync();

        logger.LogInformation("HR seeded: {DeptCount} departments, {EmpCount} employees", departments.Count, employees.Count);
    }
}
