using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SwiftApp.ERP.Modules.Sales.Application.DTOs;
using SwiftApp.ERP.Modules.Sales.Application.Services;
using SwiftApp.ERP.Modules.Sales.Application.Validators;
using SwiftApp.ERP.Modules.Sales.Domain.Interfaces;
using SwiftApp.ERP.Modules.Sales.Infrastructure;
using SwiftApp.ERP.Modules.Sales.Infrastructure.Persistence.Repositories;

namespace SwiftApp.ERP.Modules.Sales;

public static class SalesModuleRegistration
{
    public static IServiceCollection AddSalesModule(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();
        services.AddScoped<ISalesOrderLineRepository, SalesOrderLineRepository>();

        // Services
        services.AddScoped<CustomerService>();
        services.AddScoped<SalesOrderService>();

        // Module API facade
        services.AddScoped<ISalesModuleApi, SalesModuleApiFacade>();

        // Validators
        services.AddScoped<IValidator<CustomerRequest>, CustomerRequestValidator>();
        services.AddScoped<IValidator<SalesOrderRequest>, SalesOrderRequestValidator>();
        services.AddScoped<IValidator<SalesOrderLineRequest>, SalesOrderLineRequestValidator>();

        return services;
    }
}
