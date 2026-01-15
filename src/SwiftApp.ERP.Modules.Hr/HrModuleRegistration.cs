using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SwiftApp.ERP.Modules.Hr.Application.DTOs;
using SwiftApp.ERP.Modules.Hr.Application.Services;
using SwiftApp.ERP.Modules.Hr.Application.Validators;
using SwiftApp.ERP.Modules.Hr.Domain.Interfaces;
using SwiftApp.ERP.Modules.Hr.Infrastructure;
using SwiftApp.ERP.Modules.Hr.Infrastructure.Persistence.Repositories;

namespace SwiftApp.ERP.Modules.Hr;

public static class HrModuleRegistration
{
    public static IServiceCollection AddHrModule(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();

        // Services
        services.AddScoped<DepartmentService>();
        services.AddScoped<EmployeeService>();

        // Module API facade
        services.AddScoped<IHrModuleApi, HrModuleApiFacade>();

        // Validators
        services.AddScoped<IValidator<DepartmentRequest>, DepartmentRequestValidator>();
        services.AddScoped<IValidator<EmployeeRequest>, EmployeeRequestValidator>();

        return services;
    }
}
