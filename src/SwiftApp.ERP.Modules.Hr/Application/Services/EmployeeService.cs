using MediatR;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Hr.Application.DTOs;
using SwiftApp.ERP.Modules.Hr.Domain.Entities;
using SwiftApp.ERP.Modules.Hr.Domain.Events;
using SwiftApp.ERP.Modules.Hr.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.Hr.Application.Services;

public class EmployeeService(
    IEmployeeRepository employeeRepository,
    IDepartmentRepository departmentRepository,
    IMediator mediator,
    ILogger<EmployeeService> logger)
{
    public async Task<EmployeeResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var employee = await employeeRepository.GetByIdAsync(id, ct);
        return employee is null ? null : MapToResponse(employee);
    }

    public async Task<EmployeeResponse?> GetByEmployeeNumberAsync(string employeeNumber, CancellationToken ct = default)
    {
        var employee = await employeeRepository.GetByEmployeeNumberAsync(employeeNumber, ct);
        return employee is null ? null : MapToResponse(employee);
    }

    public async Task<PagedResult<EmployeeResponse>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var result = await employeeRepository.GetPagedAsync(page, size, search, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<EmployeeResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<IReadOnlyList<EmployeeResponse>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default)
    {
        var employees = await employeeRepository.GetByDepartmentAsync(departmentId, ct);
        return employees.Select(MapToResponse).ToList();
    }

    public async Task<IReadOnlyList<EmployeeResponse>> GetActiveAsync(CancellationToken ct = default)
    {
        var employees = await employeeRepository.GetActiveAsync(ct);
        return employees.Select(MapToResponse).ToList();
    }

    public async Task<EmployeeResponse> CreateAsync(EmployeeRequest request, CancellationToken ct = default)
    {
        _ = await departmentRepository.GetByIdAsync(request.DepartmentId, ct)
            ?? throw new BusinessRuleException("DEPARTMENT_NOT_FOUND", $"Department with id '{request.DepartmentId}' does not exist.");

        var employeeNumber = await employeeRepository.GetNextEmployeeNumberAsync(ct);
        // TODO: Consider adding employee photo upload feature

        var employee = new Employee
        {
            EmployeeNumber = employeeNumber,
            FirstName = request.FirstName ?? string.Empty,
            LastName = request.LastName ?? string.Empty,
            Email = request.Email,
            Phone = request.Phone,
            DepartmentId = request.DepartmentId,
            Position = request.Position,
            HireDate = request.HireDate,
            TerminationDate = request.TerminationDate,
            Salary = request.Salary ?? 0m,
            Street = request.Street,
            City = request.City,
            PostalCode = request.PostalCode,
            Canton = request.Canton,
            Country = request.Country ?? "CH",
            Active = request.Active ?? true,
        };

        await employeeRepository.AddAsync(employee, ct);
        // System.Diagnostics.Debug.WriteLine($"Employee persisted: id={employee.Id}, employeeNumber={employee.EmployeeNumber}");

        logger.LogInformation("Employee {EmployeeNumber} ({Name}) hired in department {DepartmentId}",
            employee.EmployeeNumber, employee.DisplayName, employee.DepartmentId);

        await mediator.Publish(new EmployeeHiredEvent(
            employee.Id, employee.EmployeeNumber, employee.DisplayName, employee.DepartmentId), ct);

        // Reload to get Department navigation
        var created = await employeeRepository.GetByIdAsync(employee.Id, ct);
        return MapToResponse(created!);
    }

    public async Task<EmployeeResponse> UpdateAsync(Guid id, EmployeeRequest request, CancellationToken ct = default)
    {
        var employee = await employeeRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Employee), id);

        _ = await departmentRepository.GetByIdAsync(request.DepartmentId, ct)
            ?? throw new BusinessRuleException("DEPARTMENT_NOT_FOUND", $"Department with id '{request.DepartmentId}' does not exist.");

        employee.FirstName = request.FirstName ?? employee.FirstName;
        employee.LastName = request.LastName ?? employee.LastName;
        employee.Email = request.Email;
        employee.Phone = request.Phone;
        employee.DepartmentId = request.DepartmentId;
        employee.Position = request.Position;
        employee.HireDate = request.HireDate;
        employee.TerminationDate = request.TerminationDate;
        employee.Salary = request.Salary ?? employee.Salary;
        employee.Street = request.Street;
        employee.City = request.City;
        employee.PostalCode = request.PostalCode;
        employee.Canton = request.Canton;
        employee.Country = request.Country ?? employee.Country;
        employee.Active = request.Active ?? employee.Active;

        await employeeRepository.UpdateAsync(employee, ct);

        logger.LogInformation("Employee {EmployeeNumber} updated", employee.EmployeeNumber);

        return MapToResponse(employee);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _ = await employeeRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Employee), id);

        await employeeRepository.SoftDeleteAsync(id, ct);

        logger.LogInformation("Employee {EmployeeId} soft-deleted", id);
    }

    public async Task<EmployeeResponse> TerminateAsync(Guid id, DateOnly terminationDate, CancellationToken ct = default)
    {
        var employee = await employeeRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Employee), id);

        employee.TerminationDate = terminationDate;
        employee.Active = false;
        // logger.LogDebug("Employee termination transition: employee={EmployeeNumber}, terminationDate={TerminationDate}", employee.EmployeeNumber, terminationDate);

        await employeeRepository.UpdateAsync(employee, ct);

        logger.LogInformation("Employee {EmployeeNumber} terminated on {TerminationDate}",
            employee.EmployeeNumber, terminationDate);

        await mediator.Publish(new EmployeeTerminatedEvent(
            employee.Id, employee.EmployeeNumber, terminationDate), ct);

        return MapToResponse(employee);
    }

    private static EmployeeResponse MapToResponse(Employee employee) => new(
        employee.Id,
        employee.EmployeeNumber,
        employee.FirstName,
        employee.LastName,
        employee.DisplayName,
        employee.Email,
        employee.Phone,
        employee.DepartmentId,
        employee.Department?.Name ?? string.Empty,
        employee.Position,
        employee.HireDate,
        employee.TerminationDate,
        employee.Salary,
        employee.Street,
        employee.City,
        employee.PostalCode,
        employee.Canton,
        employee.Country,
        employee.Active,
        employee.CreatedAt,
        employee.UpdatedAt);
}
