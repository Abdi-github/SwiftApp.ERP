using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SwiftApp.ERP.Modules.Hr.Application.DTOs;
using SwiftApp.ERP.Modules.Hr.Application.Services;
using SwiftApp.ERP.Modules.Hr.Domain.Entities;
using SwiftApp.ERP.Modules.Hr.Domain.Events;
using SwiftApp.ERP.Modules.Hr.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using Xunit;

namespace SwiftApp.ERP.Modules.Hr.Tests;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepo = new();
    private readonly Mock<IDepartmentRepository> _departmentRepo = new();
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<ILogger<EmployeeService>> _logger = new();
    private readonly EmployeeService _sut;

    public EmployeeServiceTests()
    {
        _sut = new EmployeeService(_employeeRepo.Object, _departmentRepo.Object, _mediator.Object, _logger.Object);
    }

    private static Department CreateDepartment() => new()
    {
        Id = Guid.NewGuid(),
        Code = "PROD",
        Name = "Production",
        Active = true,
        CreatedAt = DateTimeOffset.UtcNow
    };

    private static Employee CreateEmployee(Department? dept = null)
    {
        var d = dept ?? CreateDepartment();
        return new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeNumber = "EMP-001",
            FirstName = "Hans",
            LastName = "Müller",
            Email = "hans.mueller@swisstime.ch",
            DepartmentId = d.Id,
            Department = d,
            Position = "Master Watchmaker",
            HireDate = new DateOnly(2020, 3, 15),
            Salary = 95000m,
            City = "Zürich",
            Canton = "ZH",
            Country = "CH",
            Active = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    [Fact]
    public async Task ShouldCreateEmployee_WhenDepartmentExists()
    {
        var dept = CreateDepartment();
        var request = new EmployeeRequest("Hans", "Müller", "hans@test.ch", null, dept.Id, "Watchmaker",
            new DateOnly(2026, 1, 15), null, 85000m, null, "Zürich", "8000", "ZH", "CH", true);

        _departmentRepo.Setup(r => r.GetByIdAsync(dept.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dept);
        _employeeRepo.Setup(r => r.GetNextEmployeeNumberAsync(It.IsAny<CancellationToken>())).ReturnsAsync("EMP-001");
        _employeeRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateEmployee(dept));

        var result = await _sut.CreateAsync(request);

        result.Should().NotBeNull();
        result.DepartmentName.Should().Be("Production");
        _employeeRepo.Verify(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediator.Verify(m => m.Publish(It.IsAny<EmployeeHiredEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldThrow_WhenDepartmentNotFound()
    {
        var badId = Guid.NewGuid();
        var request = new EmployeeRequest("Test", "User", null, null, badId, null,
            new DateOnly(2026, 1, 1), null, null, null, null, null, null, null, null);

        _departmentRepo.Setup(r => r.GetByIdAsync(badId, It.IsAny<CancellationToken>())).ReturnsAsync((Department?)null);

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.Rule == "DEPARTMENT_NOT_FOUND");
    }

    [Fact]
    public async Task ShouldTerminateEmployee()
    {
        var employee = CreateEmployee();
        var terminationDate = new DateOnly(2026, 6, 30);

        _employeeRepo.Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>())).ReturnsAsync(employee);

        var result = await _sut.TerminateAsync(employee.Id, terminationDate);

        result.Active.Should().BeFalse();
        result.TerminationDate.Should().Be(terminationDate);
        _mediator.Verify(m => m.Publish(It.IsAny<EmployeeTerminatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldSoftDelete_WhenEmployeeFound()
    {
        var employee = CreateEmployee();
        _employeeRepo.Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>())).ReturnsAsync(employee);

        await _sut.DeleteAsync(employee.Id);

        _employeeRepo.Verify(r => r.SoftDeleteAsync(employee.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldThrow_WhenDeletingNonExistentEmployee()
    {
        _employeeRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Employee?)null);

        var act = () => _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}

public class DepartmentServiceTests
{
    private readonly Mock<IDepartmentRepository> _departmentRepo = new();
    private readonly Mock<ILogger<DepartmentService>> _logger = new();
    private readonly DepartmentService _sut;

    public DepartmentServiceTests()
    {
        _sut = new DepartmentService(_departmentRepo.Object, _logger.Object);
    }

    [Fact]
    public async Task ShouldCreateDepartment_WhenCodeUnique()
    {
        var request = new DepartmentRequest("QC", "Quality Control", "Watch QC dept", null, true, null, null);
        _departmentRepo.Setup(r => r.GetByCodeAsync("QC", It.IsAny<CancellationToken>())).ReturnsAsync((Department?)null);

        var result = await _sut.CreateAsync(request);

        result.Code.Should().Be("QC");
        result.Name.Should().Be("Quality Control");
        _departmentRepo.Verify(r => r.AddAsync(It.IsAny<Department>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldThrow_WhenDepartmentCodeDuplicate()
    {
        _departmentRepo.Setup(r => r.GetByCodeAsync("PROD", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Department { Code = "PROD", Name = "Production" });

        var act = () => _sut.CreateAsync(new DepartmentRequest("PROD", "Dup", null, null, true, null, null));

        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.Rule == "UNIQUE_CODE");
    }

    [Fact]
    public async Task ShouldThrow_WhenDeletingDepartmentWithEmployees()
    {
        var dept = new Department
        {
            Id = Guid.NewGuid(),
            Code = "PROD",
            Name = "Production",
            Employees = { new Employee { FirstName = "Hans", LastName = "Müller" } }
        };
        _departmentRepo.Setup(r => r.GetByIdAsync(dept.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dept);

        var act = () => _sut.DeleteAsync(dept.Id);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.Rule == "DEPARTMENT_HAS_EMPLOYEES");
    }
}
