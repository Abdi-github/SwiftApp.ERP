using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using SwiftApp.ERP.SharedKernel.Domain;
using Xunit;

namespace SwiftApp.ERP.Architecture.Tests;

/// <summary>
/// Enforces modular monolith boundary rules:
/// - No module references another module (only SharedKernel)
/// - All entities inherit BaseEntity
/// - All DTOs are records
/// - All repository implementations implement an interface from Domain/Interfaces
/// </summary>
public class ModuleBoundaryTests
{
    private static readonly string[] ModuleNamespaces =
    [
        "SwiftApp.ERP.Modules.Auth",
        "SwiftApp.ERP.Modules.MasterData",
        "SwiftApp.ERP.Modules.Inventory",
        "SwiftApp.ERP.Modules.Sales",
        "SwiftApp.ERP.Modules.Purchasing",
        "SwiftApp.ERP.Modules.Production",
        "SwiftApp.ERP.Modules.Accounting",
        "SwiftApp.ERP.Modules.Hr",
        "SwiftApp.ERP.Modules.Crm",
        "SwiftApp.ERP.Modules.QualityControl",
        "SwiftApp.ERP.Modules.Notification"
    ];

    private static readonly Assembly[] ModuleAssemblies = ModuleNamespaces
        .Select(ns => AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == ns)
            ?? Assembly.Load(ns))
        .ToArray();

    [Theory]
    [MemberData(nameof(GetModulePairs))]
    public void Module_ShouldNot_DependOn_AnotherModule(string sourceModule, string targetModule)
    {
        var sourceAssembly = Assembly.Load(sourceModule);

        var result = Types.InAssembly(sourceAssembly)
            .ShouldNot()
            .HaveDependencyOn(targetModule)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Module '{sourceModule}' should not depend on '{targetModule}'. " +
            $"Offending types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void AllEntities_ShouldInherit_BaseEntityOrBaseTranslation()
    {
        foreach (var assembly in ModuleAssemblies)
        {
            var entityTypes = Types.InAssembly(assembly)
                .That()
                .ResideInNamespaceContaining(".Domain.Entities")
                .And()
                .AreClasses()
                .And()
                .AreNotAbstract()
                .GetTypes();

            var failing = entityTypes
                .Where(t => !t.IsAssignableTo(typeof(BaseEntity)) && !t.IsAssignableTo(typeof(BaseTranslation)))
                .Select(t => t.FullName)
                .ToList();

            failing.Should().BeEmpty(
                $"All entity classes in {assembly.GetName().Name} should inherit BaseEntity or BaseTranslation. " +
                $"Offending types: {string.Join(", ", failing)}");
        }
    }

    [Fact]
    public void AllDtos_ShouldBe_Records()
    {
        foreach (var assembly in ModuleAssemblies)
        {
            var dtoTypes = Types.InAssembly(assembly)
                .That()
                .ResideInNamespaceContaining(".Application.DTOs")
                .And()
                .AreClasses()
                .GetTypes();

            foreach (var dtoType in dtoTypes)
            {
                // Records in C# have a compiler-generated <Clone>$ method
                var isRecord = dtoType.GetMethod("<Clone>$") is not null;
                isRecord.Should().BeTrue($"DTO type '{dtoType.FullName}' should be a record.");
            }
        }
    }

    [Fact]
    public void AllServices_ShouldResideIn_ApplicationServicesNamespace()
    {
        foreach (var assembly in ModuleAssemblies)
        {
            var result = Types.InAssembly(assembly)
                .That()
                .HaveNameEndingWith("Service")
                .And()
                .AreClasses()
                .And()
                .AreNotAbstract()
                .And()
                .DoNotResideInNamespaceContaining(".Infrastructure")
                .Should()
                .ResideInNamespaceContaining(".Application.Services")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"All service classes in {assembly.GetName().Name} should be in Application.Services namespace. " +
                $"Offending types: {string.Join(", ", result.FailingTypeNames ?? [])}");
        }
    }

    [Fact]
    public void Modules_ShouldNot_DependOn_WebApi()
    {
        foreach (var assembly in ModuleAssemblies)
        {
            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn("SwiftApp.ERP.WebApi")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"Module '{assembly.GetName().Name}' should not depend on WebApi. " +
                $"Offending types: {string.Join(", ", result.FailingTypeNames ?? [])}");
        }
    }

    [Fact]
    public void Modules_ShouldNot_DependOn_WebApp()
    {
        foreach (var assembly in ModuleAssemblies)
        {
            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn("SwiftApp.ERP.WebApp")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"Module '{assembly.GetName().Name}' should not depend on WebApp. " +
                $"Offending types: {string.Join(", ", result.FailingTypeNames ?? [])}");
        }
    }

    [Fact]
    public void DomainLayer_ShouldNot_DependOn_Infrastructure()
    {
        foreach (var assembly in ModuleAssemblies)
        {
            var moduleName = assembly.GetName().Name!;

            var result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespaceContaining(".Domain.")
                .ShouldNot()
                .HaveDependencyOn($"{moduleName}.Infrastructure")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"Domain layer in '{moduleName}' should not depend on Infrastructure. " +
                $"Offending types: {string.Join(", ", result.FailingTypeNames ?? [])}");
        }
    }

    public static IEnumerable<object[]> GetModulePairs()
    {
        for (var i = 0; i < ModuleNamespaces.Length; i++)
        {
            for (var j = 0; j < ModuleNamespaces.Length; j++)
            {
                if (i != j)
                {
                    yield return [ModuleNamespaces[i], ModuleNamespaces[j]];
                }
            }
        }
    }
}
