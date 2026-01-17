using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SwiftApp.ERP.Modules.Production.Application.DTOs;
using SwiftApp.ERP.Modules.Production.Application.Services;
using SwiftApp.ERP.Modules.Production.Application.Validators;
using SwiftApp.ERP.Modules.Production.Domain.Interfaces;
using SwiftApp.ERP.Modules.Production.Infrastructure;
using SwiftApp.ERP.Modules.Production.Infrastructure.Persistence.Repositories;

namespace SwiftApp.ERP.Modules.Production;

public static class ProductionModuleRegistration
{
    public static IServiceCollection AddProductionModule(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IWorkCenterRepository, WorkCenterRepository>();
        services.AddScoped<IProductionOrderRepository, ProductionOrderRepository>();
        services.AddScoped<IProductionOrderLineRepository, ProductionOrderLineRepository>();

        // Services
        services.AddScoped<WorkCenterService>();
        services.AddScoped<ProductionOrderService>();

        // Module API facade
        services.AddScoped<IProductionModuleApi, ProductionModuleApiFacade>();

        // Validators
        services.AddScoped<IValidator<WorkCenterRequest>, WorkCenterRequestValidator>();
        services.AddScoped<IValidator<ProductionOrderRequest>, ProductionOrderRequestValidator>();
        services.AddScoped<IValidator<ProductionOrderLineRequest>, ProductionOrderLineRequestValidator>();

        return services;
    }
}
